using System.IO;
using UnityEditor;
using UnityEngine;

namespace PinballRoguelite.Editor
{
    /// <summary>
    /// Imports the PSD-derived gameplay art as sprites and keeps the runtime catalog stable.
    /// Visual assets live in Assets/Art/UI; only this small reference asset lives in Resources.
    /// </summary>
    public static class BattleArtCatalogBuilder
    {
        private const string CatalogPath = "Assets/Resources/UI/Battle/BattleArtCatalog.asset";

        [MenuItem("Pinball Roguelite/UI/Import Battle Art And Rebuild Gameplay UI")]
        public static void ImportAndRebuild()
        {
            ConfigureSpriteImporters();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureCatalog();
            GameplayUiPrefabBuilder.Rebuild();
            AssetDatabase.SaveAssets();
            Debug.Log("PINBALL BATTLE ART PASS: sprite imports, catalog, and GameplayUI prefab are ready.");
        }

        public static BattleArtCatalog EnsureCatalog()
        {
            ConfigureSpriteImporters();
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder("Assets/Resources/UI/Battle");

            var catalog = AssetDatabase.LoadAssetAtPath<BattleArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<BattleArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var serialized = new SerializedObject(catalog);
            SetSprite(serialized, "battleBackground", "Assets/Art/UI/Battle/Backgrounds/ui_battle_background_top.png");
            SetSprite(serialized, "battleGround", "Assets/Art/UI/Battle/Backgrounds/ui_battle_background_road.png");
            SetSprite(serialized, "bottomBackground", "Assets/Art/UI/Battle/Backgrounds/ui_battle_background_bottom.png");
            SetSprite(serialized, "boardTexture", "Assets/Art/UI/Battle/Board/ui_battle_board_texture.png");
            SetSprite(serialized, "boardFrame", "Assets/Art/UI/Battle/Board/ui_battle_board_frame.png");
            SetSprite(serialized, "playerCharacter", "Assets/Art/UI/Battle/Characters/ui_battle_character_player.png");
            SetSprite(serialized, "playerWeapon", "Assets/Art/UI/Battle/Characters/ui_battle_prop_player_weapon.png");
            SetSprite(serialized, "enemyCharacter", "Assets/Art/UI/Battle/Characters/ui_battle_character_enemy.png");
            SetSprite(serialized, "playerInfoBack", "Assets/Art/UI/Battle/HUD/ui_battle_hud_player_info_back.png");
            SetSprite(serialized, "playerAvatar", "Assets/Art/UI/Battle/HUD/ui_battle_hud_player_avatar.png");
            SetSprite(serialized, "playerHealthBack", "Assets/Art/UI/Battle/HUD/ui_battle_hud_player_hp_back.png");
            SetSprite(serialized, "playerHealthFill", "Assets/Art/UI/Battle/HUD/ui_battle_hud_player_hp_fill.png");
            SetSprite(serialized, "enemyInfoBack", "Assets/Art/UI/Battle/HUD/ui_battle_hud_enemy_info_back.png");
            SetSprite(serialized, "enemyAvatar", "Assets/Art/UI/Battle/HUD/ui_battle_hud_enemy_avatar.png");
            SetSprite(serialized, "enemyHealthBack", "Assets/Art/UI/Battle/HUD/ui_battle_hud_enemy_hp_back.png");
            SetSprite(serialized, "enemyHealthFill", "Assets/Art/UI/Battle/HUD/ui_battle_hud_enemy_hp_fill.png");
            SetSprite(serialized, "damageBack", "Assets/Art/UI/Battle/HUD/ui_battle_damage_back.png");
            SetSprite(serialized, "damageTitle", "Assets/Art/UI/Battle/HUD/ui_battle_damage_title.png");
            SetSprite(serialized, "normalPeg", "Assets/Art/UI/Battle/Pegs/ui_battle_peg_normal.png");
            SetSprite(serialized, "slotDivider", "Assets/Art/UI/Battle/Slots/ui_battle_slot_divider.png");
            SetSprite(serialized, "starSlot", "Assets/Art/UI/Battle/Slots/ui_battle_slot_star.png");
            SetSprite(serialized, "launcherSpring", "Assets/Art/UI/Battle/Launcher/ui_battle_launcher_spring.png");
            SetSprite(serialized, "launcherSpringCap", "Assets/Art/UI/Battle/Launcher/ui_battle_launcher_spring_cap.png");
            SetSprite(serialized, "launcherLoadedMarble", "Assets/Art/UI/Battle/Launcher/ui_battle_launcher_loaded_marble.png");
            SetSprite(serialized, "normalButton", "Assets/Art/UI/Battle/Controls/ui_battle_button_normal.png");
            SetSprite(serialized, "normalButtonIcon", "Assets/Art/UI/Battle/Controls/ui_battle_icon_normal.png");
            SetSprite(serialized, "splitButton", "Assets/Art/UI/Battle/Controls/ui_battle_button_split.png");
            SetSprite(serialized, "splitButtonIcon", "Assets/Art/UI/Battle/Controls/ui_battle_icon_split.png");
            SetSprite(serialized, "launchButton", "Assets/Art/UI/Battle/Controls/ui_battle_button_launch.png");
            SetSprite(serialized, "launchButtonIcon", "Assets/Art/UI/Battle/Controls/ui_battle_icon_launch.png");
            SetSprite(serialized, "launchButtonDecoration", "Assets/Art/UI/Battle/Controls/ui_battle_button_launch_baked_label_reference.png");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void ConfigureSpriteImporters()
        {
            foreach (var path in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/UI" }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(path);
                if (!(AssetImporter.GetAtPath(assetPath) is TextureImporter importer)) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.compressionQuality = 100;
                importer.crunchedCompression = false;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteBorder = GetBorder(assetPath);
                ConfigurePlatform(importer, "Standalone");
                ConfigurePlatform(importer, "WebGL");
                importer.SaveAndReimport();
            }
        }

        private static void ConfigurePlatform(TextureImporter importer, string platformName)
        {
            var settings = importer.GetPlatformTextureSettings(platformName);
            settings.name = platformName;
            settings.overridden = true;
            settings.maxTextureSize = 2048;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.compressionQuality = 100;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }

        private static Vector4 GetBorder(string path)
        {
            if (path.Contains("button_launch")) return new Vector4(32f, 28f, 32f, 28f);
            if (path.Contains("button_normal") || path.Contains("button_split")) return new Vector4(24f, 24f, 24f, 24f);
            if (path.Contains("info_back")) return new Vector4(18f, 18f, 18f, 18f);
            if (path.Contains("hp_back") || path.Contains("hp_fill")) return new Vector4(14f, 14f, 14f, 14f);
            if (path.Contains("damage_back")) return new Vector4(16f, 16f, 16f, 16f);
            return Vector4.zero;
        }

        private static void SetSprite(SerializedObject serialized, string propertyName, string path)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null) property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
