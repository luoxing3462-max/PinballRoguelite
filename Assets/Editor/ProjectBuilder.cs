using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PinballRoguelite.Editor
{
    public static class ProjectBuilder
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string UiPrefabPath = "Assets/Resources/Prefabs/UI.prefab";

        [MenuItem("Pinball Roguelite/Build Project Assets")]
        public static void BuildProjectAssets()
        {
            CreatePrefabs();
            CreateMainScene();
            ConfigureProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PINBALL BUILD PASS: prefabs, Main scene and portrait project settings are ready.");
        }

        [MenuItem("Pinball Roguelite/Open Main And Play _F8")]
        public static void OpenMainAndPlay()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Pinball Roguelite/Rebuild UI Prefab")]
        public static void CreateUiPrefabAsset()
        {
            Directory.CreateDirectory("Assets/Resources/Prefabs");
            SaveUiPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PINBALL UI PREFAB PASS: UI.prefab contains the Canvas and EventSystem roots.");
        }

        public static void BuildAndValidate()
        {
            BuildProjectAssets();
            var failures = ValidateCore();
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Library", "PinballRogueliteValidation.txt");
            if (failures.Count == 0)
            {
                File.WriteAllText(reportPath,
                    "PASS\nUnity 6 project assets created.\nMap, run-state, score, level configuration, scene and prefab checks passed.\n");
                Debug.Log("PINBALL VALIDATION PASS: map -> node -> gameplay configuration -> score settlement -> boss route.");
                return;
            }

            File.WriteAllLines(reportPath, failures);
            throw new InvalidOperationException("PINBALL VALIDATION FAILED:\n" + string.Join("\n", failures));
        }

        private static void CreatePrefabs()
        {
            Directory.CreateDirectory("Assets/Resources/Prefabs");
            SaveComponentPrefab<Marble>("Marble");
            SaveComponentPrefab<Bumper2D>("Bumper");
            SaveComponentPrefab<ScoringZone>("ScoringZone");
            SaveUiPrefab();
        }

        private static void SaveComponentPrefab<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.AddComponent<T>();
            PrefabUtility.SaveAsPrefabAsset(go, $"Assets/Resources/Prefabs/{name}.prefab");
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static void SaveUiPrefab()
        {
            GameObject uiRoot = new GameObject("UI");
            GameObject canvasObject = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(uiRoot.transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(780f, 1688f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(uiRoot.transform, false);
            PrefabUtility.SaveAsPrefabAsset(uiRoot, UiPrefabPath);
            UnityEngine.Object.DestroyImmediate(uiRoot);
        }

        private static void CreateMainScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("GameRoot");
            root.AddComponent<GameRoot>();
            EditorSceneManager.SaveScene(scene, MainScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
        }

        private static void ConfigureProject()
        {
            PlayerSettings.productName = "PinballRoguelite";
            PlayerSettings.companyName = "Demo";
            PlayerSettings.defaultScreenWidth = 780;
            PlayerSettings.defaultScreenHeight = 1688;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.demo.pinballroguelite");
            EditorSettings.enterPlayModeOptionsEnabled = false;
        }

        private static List<string> ValidateCore()
        {
            var failures = new List<string>();
            for (var sample = 0; sample < 100; sample++)
            {
                var map = TowerMapGenerator.Generate(sample * 7919 + 17);
                Check(map.Layers.Count >= 6 && map.Layers.Count <= 8, "Map layers must be 6..8", failures);
                Check(map.Layers[0].Count == 1 && map.Layers[0][0].Type == MapNodeType.Normal,
                    "Map must have one normal entry", failures);
                Check(map.Layers[^1].Count == 1 && map.Layers[^1][0].Type == MapNodeType.Boss,
                    "Map must have one final Boss", failures);
                foreach (var layer in map.Layers)
                    Check(layer.Count <= 4, "A map layer exceeded four nodes", failures);
                for (var i = 1; i < map.Layers.Count; i++)
                foreach (var node in map.Layers[i])
                {
                    var incoming = false;
                    foreach (var previous in map.Layers[i - 1])
                        incoming |= previous.Connections.Contains(node.Id);
                    Check(incoming, $"Node {node.Id} has no incoming route", failures);
                }
            }

            var run = new RunState();
            run.StartNewRun(42);
            var safety = 0;
            var reachedBoss = false;
            while (run.AvailableIds.Count > 0 && safety++ < 12)
            {
                string id = null;
                foreach (var candidate in run.AvailableIds) { id = candidate; break; }
                var node = run.Map.Find(id);
                Check(run.ChooseNode(node), "An available map node could not be chosen", failures);
                if (node.Type == MapNodeType.Boss) { reachedBoss = true; break; }
                run.CompleteActiveNode();
            }
            Check(reachedBoss, "Following available routes did not reach Boss", failures);

            var score = new ScoreManager();
            score.Reset();
            score.RegisterMarble(1);
            score.AddBumperScore(1, 10);
            var carried = score.DetachMarble(1);
            score.RegisterMarble(2, carried);
            score.RegisterMarble(3);
            Check(score.SettleMarble(2, 2) == 20, "Split child did not inherit parent score once", failures);
            Check(score.SettleMarble(3, 3) == 0, "Second split child duplicated inherited score", failures);
            Check(score.FinishLaunch() == 20 && score.TotalScore == 20, "Unified launch settlement is incorrect", failures);

            Check(LevelConfig.For(MapNodeType.Normal).TargetScore == 140, "Normal target mismatch", failures);
            Check(LevelConfig.For(MapNodeType.Elite).TargetScore == 260, "Elite target mismatch", failures);
            Check(LevelConfig.For(MapNodeType.Boss).TargetScore == 480, "Boss target mismatch", failures);
            Check(AssetDatabase.LoadAssetAtPath<GameObject>(MainScenePath) == null,
                "Scene path unexpectedly loaded as a prefab", failures);
            Check(File.Exists(MainScenePath), "Main scene was not created", failures);
            Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Marble.prefab") != null,
                "Marble prefab missing", failures);
            Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Bumper.prefab") != null,
                "Bumper prefab missing", failures);
            Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/ScoringZone.prefab") != null,
                "ScoringZone prefab missing", failures);
            GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UiPrefabPath);
            Check(uiPrefab != null, "UI prefab missing", failures);
            Check(uiPrefab != null && uiPrefab.GetComponentInChildren<Canvas>(true) != null,
                "UI prefab Canvas missing", failures);
            Check(uiPrefab != null && uiPrefab.GetComponentInChildren<EventSystem>(true) != null,
                "UI prefab EventSystem missing", failures);
            return failures;
        }

        private static void Check(bool condition, string message, List<string> failures)
        {
            if (!condition && !failures.Contains(message)) failures.Add(message);
        }
    }
}
