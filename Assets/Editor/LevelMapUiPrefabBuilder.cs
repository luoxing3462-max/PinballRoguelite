using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinballRoguelite.Editor
{
    [InitializeOnLoad]
    public static class LevelMapUiPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/LevelMapUI.prefab";
        private const string UxmlPath = "Assets/UI/Toolkit/LevelMap/LevelMap.uxml";
        private const string NodeTemplatePath = "Assets/UI/Toolkit/LevelMap/Node.uxml";
        private const string PanelSettingsPath = "Assets/UI/Toolkit/LevelMap/LevelMapPanelSettings.asset";
        private const string UiRootPrefabPath = "Assets/Resources/Prefabs/UI.prefab";

        static LevelMapUiPrefabBuilder()
        {
            EditorApplication.delayCall += EnsurePrefabExists;
        }

        [MenuItem("Pinball Roguelite/UI/Rebuild Level Map UI Toolkit Prefab (Overwrites Layout)")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");
            PanelSettings panelSettings = EnsurePanelSettings();
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            VisualTreeAsset nodeTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(NodeTemplatePath);
            if (visualTree == null || nodeTemplate == null) throw new FileNotFoundException("LevelMap UXML assets must exist before creating LevelMapUI.prefab.");

            GameObject root = new GameObject("LevelMapUI");
            UIDocument document = root.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTree;
            LevelMapUIController controller = root.AddComponent<LevelMapUIController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("_document").objectReferenceValue = document;
            serializedController.FindProperty("_nodeTemplate").objectReferenceValue = nodeTemplate;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            RegisterPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsurePrefabExists()
        {
            ConfigurePanelSettings(EnsurePanelSettings());
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) Rebuild();
            else RegisterPrefab();
        }

        private static PanelSettings EnsurePanelSettings()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panelSettings != null) return panelSettings;

            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
            return panelSettings;
        }

        private static void ConfigurePanelSettings(PanelSettings panelSettings)
        {
            SerializedObject serializedPanelSettings = new SerializedObject(panelSettings);
            serializedPanelSettings.FindProperty("m_ScaleMode").enumValueIndex = 1;
            serializedPanelSettings.FindProperty("m_ReferenceResolution").vector2IntValue = new Vector2Int(780, 1688);
            serializedPanelSettings.FindProperty("m_ScreenMatchMode").enumValueIndex = 0;
            serializedPanelSettings.FindProperty("m_Match").floatValue = 0f;
            serializedPanelSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panelSettings);
            AssetDatabase.SaveAssetIfDirty(panelSettings);
        }

        private static void RegisterPrefab()
        {
            GameObject uiRoot = PrefabUtility.LoadPrefabContents(UiRootPrefabPath);
            LevelUiPrefabRegistry registry = uiRoot.GetComponent<LevelUiPrefabRegistry>();
            if (registry == null) registry = uiRoot.AddComponent<LevelUiPrefabRegistry>();
            LevelMapUIController levelMapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<LevelMapUIController>();
            SerializedObject serializedRegistry = new SerializedObject(registry);
            serializedRegistry.FindProperty("_levelMapUiPrefab").objectReferenceValue = levelMapPrefab;
            serializedRegistry.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(uiRoot, UiRootPrefabPath);
            PrefabUtility.UnloadPrefabContents(uiRoot);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
