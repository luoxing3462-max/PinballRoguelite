using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinballRoguelite.Editor
{
    [InitializeOnLoad]
    public static class MarbleSelectionUiPrefabBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/MarbleSelectionUI.prefab";
        private const string UxmlPath = "Assets/UI/Toolkit/MarbleSelection/MarbleSelection.uxml";
        private const string CardTemplatePath = "Assets/UI/Toolkit/MarbleSelection/MarbleCard.uxml";
        private const string PanelSettingsPath = "Assets/UI/Toolkit/MarbleSelection/MarbleSelectionPanelSettings.asset";
        private const string ArtCatalogPath = "Assets/Resources/UI/MarbleSelection/MarbleSelectionArtCatalog.asset";
        private const string UiRootPrefabPath = "Assets/Resources/Prefabs/UI.prefab";

        static MarbleSelectionUiPrefabBuilder()
        {
            EditorApplication.delayCall += EnsurePrefabExists;
        }

        [MenuItem("Pinball Roguelite/UI/Rebuild Marble Selection UI Toolkit Prefab (Overwrites Layout)")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder("Assets/Resources/UI/MarbleSelection");
            PanelSettings panelSettings = EnsurePanelSettings();
            ConfigurePanelSettings(panelSettings);
            ConfigureImportedSprites();
            MarbleSelectionArtCatalog artCatalog = EnsureArtCatalog();
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            VisualTreeAsset cardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CardTemplatePath);
            if (visualTree == null || cardTemplate == null)
                throw new FileNotFoundException("MarbleSelection UXML assets must exist before creating MarbleSelectionUI.prefab.");

            GameObject root = new GameObject("MarbleSelectionUI");
            UIDocument document = root.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTree;
            MarbleSelectionUIController controller = root.AddComponent<MarbleSelectionUIController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("_document").objectReferenceValue = document;
            serializedController.FindProperty("_cardTemplate").objectReferenceValue = cardTemplate;
            serializedController.FindProperty("_artCatalog").objectReferenceValue = artCatalog;
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
            ConfigureImportedSprites();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) Rebuild();
            else RegisterPrefab();
        }

        private static PanelSettings EnsurePanelSettings()
        {
            PanelSettings settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (settings != null) return settings;
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            return settings;
        }

        private static void ConfigurePanelSettings(PanelSettings settings)
        {
            SerializedObject serialized = new SerializedObject(settings);
            serialized.FindProperty("m_ScaleMode").enumValueIndex = 1;
            serialized.FindProperty("m_ReferenceResolution").vector2IntValue = new Vector2Int(750, 1334);
            serialized.FindProperty("m_ScreenMatchMode").enumValueIndex = 0;
            serialized.FindProperty("m_Match").floatValue = 0f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        private static MarbleSelectionArtCatalog EnsureArtCatalog()
        {
            MarbleSelectionArtCatalog catalog = AssetDatabase.LoadAssetAtPath<MarbleSelectionArtCatalog>(ArtCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<MarbleSelectionArtCatalog>();
                AssetDatabase.CreateAsset(catalog, ArtCatalogPath);
            }

            SerializedObject serialized = new SerializedObject(catalog);
            SetSprite(serialized, "_background", "Assets/Art/UI/MarbleSelection/Backgrounds/ui_marble_selection_background.png");
            SetSprite(serialized, "_title", "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_title.png");
            SetSprite(serialized, "_instructionPanel", "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_instruction_panel.png");
            SetSprite(serialized, "_footer", "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_footer.png");
            SetSprite(serialized, "_selectionCheck", "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_check.png");
            SetSprite(serialized, "_confirmButton", "Assets/Art/UI/MarbleSelection/Buttons/ui_marble_button_confirm.png");
            SerializedProperty cards = serialized.FindProperty("_cards");
            string[] ids = { MarbleCatalog.Normal, MarbleCatalog.Split, MarbleCatalog.Pierce, MarbleCatalog.Gravity, MarbleCatalog.Bounce, MarbleCatalog.Lucky };
            cards.arraySize = ids.Length;
            for (int index = 0; index < ids.Length; index++)
            {
                SerializedProperty entry = cards.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("_marbleId").stringValue = ids[index];
                entry.FindPropertyRelative("_card").objectReferenceValue = LoadSprite($"Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_{ids[index]}.png");
                entry.FindPropertyRelative("_icon").objectReferenceValue = LoadSprite($"Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_{ids[index]}.png");
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        private static void ConfigureImportedSprites()
        {
            string[] paths =
            {
                "Assets/Art/UI/MarbleSelection/Backgrounds/ui_marble_selection_background.png",
                "Assets/Art/UI/MarbleSelection/Buttons/ui_marble_button_confirm.png",
                "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_check.png",
                "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_footer.png",
                "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_instruction_panel.png",
                "Assets/Art/UI/MarbleSelection/Decorations/ui_marble_selection_title.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_normal.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_split.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_pierce.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_gravity.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_bounce.png",
                "Assets/Art/UI/MarbleSelection/Cards/ui_marble_card_lucky.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_normal.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_split.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_pierce.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_gravity.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_bounce.png",
                "Assets/Art/UI/MarbleSelection/Icons/ui_marble_icon_lucky.png"
            };
            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
                TextureImporterPlatformSettings webGl = importer.GetPlatformTextureSettings("WebGL");
                bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                    importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.filterMode != FilterMode.Bilinear ||
                    importer.maxTextureSize < 2048 || standalone.textureCompression != TextureImporterCompression.Uncompressed ||
                    webGl.textureCompression != TextureImporterCompression.Uncompressed || standalone.format != TextureImporterFormat.RGBA32 ||
                    webGl.format != TextureImporterFormat.RGBA32;
                if (!changed) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                ConfigurePlatformTexture(importer, "DefaultTexturePlatform", false);
                ConfigurePlatformTexture(importer, "Standalone", true);
                ConfigurePlatformTexture(importer, "WebGL", true);
                importer.SaveAndReimport();
            }
        }

        private static void ConfigurePlatformTexture(TextureImporter importer, string platform, bool overridden)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            settings.name = platform;
            settings.overridden = overridden;
            settings.maxTextureSize = 2048;
            settings.format = TextureImporterFormat.RGBA32;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }

        private static void SetSprite(SerializedObject target, string property, string path)
        {
            target.FindProperty(property).objectReferenceValue = LoadSprite(path);
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new FileNotFoundException($"PSD export missing or not imported as Sprite: {path}");
            return sprite;
        }

        private static void RegisterPrefab()
        {
            GameObject uiRoot = PrefabUtility.LoadPrefabContents(UiRootPrefabPath);
            MarbleSelectionUiPrefabRegistry registry = uiRoot.GetComponent<MarbleSelectionUiPrefabRegistry>();
            if (registry == null) registry = uiRoot.AddComponent<MarbleSelectionUiPrefabRegistry>();
            MarbleSelectionUIController prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<MarbleSelectionUIController>();
            SerializedObject serialized = new SerializedObject(registry);
            serialized.FindProperty("_marbleSelectionUiPrefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(uiRoot, UiRootPrefabPath);
            PrefabUtility.UnloadPrefabContents(uiRoot);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
