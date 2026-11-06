using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite.Editor
{
    [InitializeOnLoad]
    public static class LevelUiPrefabBuilder
    {
        private const string LevelPrefabPath = "Assets/Prefabs/UI/LevelUI.prefab";
        private const string UiRootPrefabPath = "Assets/Resources/Prefabs/UI.prefab";

        static LevelUiPrefabBuilder()
        {
            EditorApplication.delayCall += EnsurePrefabExists;
        }

        [MenuItem("Pinball Roguelite/UI/Rebuild Level UI Prefab")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");
            var root = CreateRect("LevelUI", null, Vector2.zero, Vector2.one);
            var view = root.gameObject.AddComponent<LevelUiView>();
            var safeArea = CreateRect("SafeArea", root, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            CreateImage("Background", safeArea, GamePalette.Background, Vector2.zero, Vector2.one);

            var header = CreateImage("Header", safeArea, new Color(0.04f, 0.08f, 0.14f, 0.96f),
                new Vector2(0.025f, 0.90f), new Vector2(0.975f, 0.985f));
            CreateText("TitleText", header.transform, "爬塔路线", 52, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.04f, 0f), new Vector2(0.58f, 1f));
            var progress = CreateText("ProgressText", header.transform, "", 25, GamePalette.Muted, TextAnchor.MiddleRight,
                new Vector2(0.54f, 0f), new Vector2(0.96f, 1f));
            CreateText("InstructionText", safeArea, "选择发光节点，路线会向下推进", 28, GamePalette.Cyan,
                TextAnchor.MiddleCenter, new Vector2(0.05f, 0.852f), new Vector2(0.95f, 0.895f));

            var mapPanel = CreateImage("MapPanel", safeArea, GamePalette.Panel,
                new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.85f));
            var mapArea = CreateRect("MapArea", mapPanel.transform, Vector2.zero, Vector2.one);
            CreateText("LegendText", safeArea, "蓝 普通   橙 精英   紫 事件   黄 商店   绿 休息   红 Boss", 23,
                GamePalette.Muted, TextAnchor.MiddleCenter, new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.105f));
            CreateText("StateLegendText", safeArea, "● 可选    ✓ 已完成    ○ 未解锁", 22, GamePalette.Hex("64748B"),
                TextAnchor.MiddleCenter, new Vector2(0.03f, 0.012f), new Vector2(0.97f, 0.057f));

            var modal = CreateRect("PlaceholderModal", safeArea, Vector2.zero, Vector2.one).gameObject;
            CreateImage("Shade", modal.transform, new Color(0.01f, 0.02f, 0.05f, 0.90f), Vector2.zero, Vector2.one);
            var panel = CreateImage("Panel", modal.transform, GamePalette.PanelLight,
                new Vector2(0.10f, 0.31f), new Vector2(0.90f, 0.69f));
            var modalTitle = CreateText("TitleText", panel.transform, "", 52, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.92f));
            var modalBody = CreateText("BodyText", panel.transform, "", 31, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.70f));
            var continueButton = CreateButton("ContinueButton", panel.transform, "继续前进", GamePalette.Blue, 34,
                new Vector2(0.20f, 0.08f), new Vector2(0.80f, 0.28f));

            ApplyBindings(view, mapArea, progress, modal, modalTitle, modalBody, continueButton);
            modal.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, LevelPrefabPath);
            Object.DestroyImmediate(root.gameObject);
            RegisterPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsurePrefabExists()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LevelPrefabPath) == null) Rebuild();
            else RegisterPrefab();
        }

        private static void RegisterPrefab()
        {
            var uiRoot = PrefabUtility.LoadPrefabContents(UiRootPrefabPath);
            var registry = uiRoot.GetComponent<LevelUiPrefabRegistry>();
            if (registry == null) registry = uiRoot.AddComponent<LevelUiPrefabRegistry>();
            var levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelPrefabPath).GetComponent<LevelUiView>();
            var serialized = new SerializedObject(registry);
            serialized.FindProperty("_levelUiPrefab").objectReferenceValue = levelPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(uiRoot, UiRootPrefabPath);
            PrefabUtility.UnloadPrefabContents(uiRoot);
        }

        private static void ApplyBindings(LevelUiView view, RectTransform mapArea, Text progress, GameObject modal,
            Text modalTitle, Text modalBody, Button continueButton)
        {
            var serialized = new SerializedObject(view);
            SetReference(serialized, "_mapArea", mapArea);
            SetReference(serialized, "_progressText", progress);
            SetReference(serialized, "_placeholderModal", modal);
            SetReference(serialized, "_placeholderTitleText", modalTitle);
            SetReference(serialized, "_placeholderBodyText", modalBody);
            SetReference(serialized, "_placeholderContinueButton", continueButton);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(SerializedObject serialized, string propertyName, Object value)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = value;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var image = CreateRect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            var text = CreateRect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<Text>();
            text.font = UiFactory.RuntimeFont;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var image = CreateImage(name, parent, color, anchorMin, anchorMax);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(0.20f, 0.24f, 0.31f, 0.8f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            CreateText("Label", image.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            return button;
        }
    }
}
