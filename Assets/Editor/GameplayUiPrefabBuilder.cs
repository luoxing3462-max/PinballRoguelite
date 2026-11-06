using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite.Editor
{
    public static class GameplayUiPrefabBuilder
    {
        private const string GameplayPrefabPath = "Assets/Prefabs/UI/GameplayUI.prefab";
        private const string UiRootPrefabPath = "Assets/Resources/Prefabs/UI.prefab";
        private const string FontAssetPath = "Assets/UI/Gameplay/Fonts/GameplayUIFont.asset";
        private const string ProjectTmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string TemporaryTmpSettingsPath = "Assets/Resources/TMP Settings.asset";

        [MenuItem("Pinball Roguelite/UI/Rebuild Gameplay UI Prefab")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");
            EnsureFolder("Assets/UI");
            EnsureFolder("Assets/UI/Gameplay");
            EnsureFolder("Assets/UI/Gameplay/Fonts");
            var font = LoadOrCreateFont();
            var root = CreateRect("GameplayUI", null, Vector2.zero, Vector2.one);
            var view = root.gameObject.AddComponent<GameplayUiView>();
            var safeArea = CreateRect("SafeArea", root, Vector2.zero, Vector2.one);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            var topHud = CreateImage("TopHUD", safeArea, new Color(0.04f, 0.08f, 0.14f, 0.96f),
                new Vector2(0.025f, 0.81f), new Vector2(0.975f, 0.99f));
            var battleInfo = CreateImage("BattleInfo", topHud.rectTransform, Color.clear, Vector2.zero, Vector2.one);
            var combat = battleInfo.gameObject.AddComponent<CombatDisplay>();
            var encounterTitle = CreateText("EncounterTitleText", battleInfo.rectTransform, font, "普通关 · 战斗区", 27,
                GamePalette.Blue, TextAlignmentOptions.Left, new Vector2(0.03f, 0.82f), new Vector2(0.34f, 1f));
            var score = CreateText("ScoreText", topHud.rectTransform, font, "积分  0 / 140", 23, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.33f, 0.82f), new Vector2(0.68f, 1f));
            var remainingShots = CreateText("RemainingShotsText", topHud.rectTransform, font, "剩余发射  5 / 5", 21,
                GamePalette.Muted, TextAlignmentOptions.Right, new Vector2(0.67f, 0.82f), new Vector2(0.97f, 1f));
            var previewBack = CreateImage("AttackPreviewBack", battleInfo.rectTransform, GamePalette.Hex("0F2D3C"),
                new Vector2(0.03f, 0.58f), new Vector2(0.32f, 0.78f));
            var attackPreview = CreateText("AttackPreviewText", previewBack.rectTransform, font, "本次攻击  0", 22,
                GamePalette.Cyan, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            var playerAvatar = CreateImage("PlayerAvatarPlaceholder", battleInfo.rectTransform, Color.clear,
                new Vector2(0.14f, 0.29f), new Vector2(0.14f, 0.29f));
            playerAvatar.rectTransform.sizeDelta = new Vector2(112f, 112f);
            var playerName = CreateText("PlayerNameText", battleInfo.rectTransform, font, "弹珠勇者", 18,
                GamePalette.Hex("E2E8F0"), TextAlignmentOptions.Center, new Vector2(0.04f, 0f), new Vector2(0.24f, 0.13f));
            var enemyAvatar = CreateImage("EnemyAvatarPlaceholder", battleInfo.rectTransform, Color.clear,
                new Vector2(0.86f, 0.29f), new Vector2(0.86f, 0.29f));
            enemyAvatar.rectTransform.sizeDelta = new Vector2(112f, 112f);
            var enemyName = CreateText("EnemyNameText", battleInfo.rectTransform, font, "洞穴史莱姆", 21, Color.white,
                TextAlignmentOptions.Left, new Vector2(0.51f, 0.68f), new Vector2(0.73f, 0.80f));
            var enemyHealth = CreateText("EnemyHealthText", battleInfo.rectTransform, font, "140 / 140", 19,
                GamePalette.Hex("CBD5E1"), TextAlignmentOptions.Right, new Vector2(0.72f, 0.68f), new Vector2(0.95f, 0.80f));
            var healthBack = CreateImage("EnemyHealthBar", battleInfo.rectTransform, GamePalette.Hex("020617"),
                new Vector2(0.51f, 0.57f), new Vector2(0.95f, 0.67f));
            var healthFill = CreateImage("EnemyHealthFill", healthBack.rectTransform, GamePalette.Green,
                new Vector2(0.015f, 0.18f), new Vector2(0.985f, 0.82f));
            healthFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            var progressLine = CreateImage("ProgressLine", battleInfo.rectTransform, GamePalette.Hex("475569"),
                new Vector2(0.20f, 0.205f), new Vector2(0.92f, 0.22f));
            CreateText("AttackHintText", battleInfo.rectTransform, font, "每次发射结算为一次攻击", 18, GamePalette.Muted,
                TextAlignmentOptions.Center, new Vector2(0.27f, 0f), new Vector2(0.73f, 0.13f));

            var boardHint = CreateText("BoardHintText", safeArea, font, "", 27, Color.white, TextAlignmentOptions.Center,
                new Vector2(0.04f, 0.765f), new Vector2(0.96f, 0.81f));
            var bottomHud = CreateImage("BottomHUD", safeArea, new Color(0.04f, 0.08f, 0.14f, 0.97f),
                Vector2.zero, new Vector2(1f, 0.20f));
            var selectionPanel = CreateRect("MarbleSelectionPanel", bottomHud.rectTransform,
                new Vector2(0.03f, 0.16f), new Vector2(0.65f, 0.95f));
            CreateText("SelectionTitleText", selectionPanel, font, "本次发射弹珠", 26, Color.white,
                TextAlignmentOptions.Left, new Vector2(0f, 0.75f), new Vector2(1f, 1f));
            var normal = CreateButton("NormalMarbleButton", selectionPanel, font, "普通弹珠", GamePalette.Blue, 30,
                new Vector2(0f, 0f), new Vector2(0.46f, 0.68f), out _);
            var split = CreateButton("SplitMarbleButton", selectionPanel, font, "分裂弹珠 ×2", GamePalette.PanelLight, 30,
                new Vector2(0.50f, 0f), new Vector2(1f, 0.68f), out _);
            var launchPanel = CreateRect("LaunchPanel", bottomHud.rectTransform, new Vector2(0.69f, 0.16f),
                new Vector2(0.97f, 0.95f));
            var chargeBack = CreateImage("ChargeBar", launchPanel, GamePalette.Hex("0F172A"), new Vector2(0f, 0.75f),
                new Vector2(1f, 0.94f));
            var chargeFill = CreateImage("ChargeFill", chargeBack.rectTransform, GamePalette.Yellow, Vector2.zero, Vector2.one);
            chargeFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            var chargeText = CreateText("ChargeValueText", chargeBack.rectTransform, font, "蓄力", 21, Color.white,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            var launch = CreateButton("LaunchButton", launchPanel, font, "按住蓄力\n松开发射", GamePalette.Green, 27,
                Vector2.zero, new Vector2(1f, 0.68f), out var launchText);
            var hold = launch.gameObject.AddComponent<LaunchHoldButton>();

            var feedback = CreateRect("FloatingFeedbackLayer", safeArea, Vector2.zero, Vector2.one);
            var hitFeedback = CreateText("HitFeedbackText", feedback, font, "", 25, GamePalette.Cyan, TextAlignmentOptions.Center,
                new Vector2(0.25f, 0.68f), new Vector2(0.75f, 0.72f));
            var damageFeedback = CreateText("DamageFeedbackText", feedback, font, "", 31, GamePalette.Yellow,
                TextAlignmentOptions.Center, new Vector2(0.25f, 0.64f), new Vector2(0.75f, 0.68f));
            var settlementFeedback = CreateText("SettlementFeedbackText", feedback, font, "", 25, GamePalette.Green,
                TextAlignmentOptions.Center, new Vector2(0.25f, 0.60f), new Vector2(0.75f, 0.64f));

            var resultOverlay = CreateImage("ResultOverlay", safeArea, new Color(0.01f, 0.02f, 0.05f, 0.92f),
                Vector2.zero, Vector2.one).gameObject;
            var victoryPanel = CreateImage("VictoryPanel", resultOverlay.transform, GamePalette.PanelLight,
                new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.77f)).gameObject;
            var victoryTitle = CreateText("VictoryTitleText", victoryPanel.transform, font, "关卡通过", 62, GamePalette.Green,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.75f), new Vector2(0.94f, 0.94f));
            var victoryMessage = CreateText("VictoryMessageText", victoryPanel.transform, font, "", 32, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.51f), new Vector2(0.92f, 0.75f));
            var victoryScore = CreateText("VictoryScoreText", victoryPanel.transform, font, "", 38, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.53f));
            var victoryButton = CreateButton("VictoryPrimaryButton", victoryPanel.transform, font, "返回地图", GamePalette.Blue, 36,
                new Vector2(0.17f, 0.12f), new Vector2(0.83f, 0.28f), out var victoryButtonText);
            var defeatPanel = CreateImage("DefeatPanel", resultOverlay.transform, GamePalette.PanelLight,
                new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.77f)).gameObject;
            var defeatTitle = CreateText("DefeatTitleText", defeatPanel.transform, font, "挑战失败", 62, GamePalette.Pink,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.75f), new Vector2(0.94f, 0.94f));
            var defeatMessage = CreateText("DefeatMessageText", defeatPanel.transform, font, "", 32, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.51f), new Vector2(0.92f, 0.75f));
            var defeatScore = CreateText("DefeatScoreText", defeatPanel.transform, font, "", 38, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.53f));
            var retryButton = CreateButton("RetryButton", defeatPanel.transform, font, "重新挑战", GamePalette.Blue, 34,
                new Vector2(0.17f, 0.21f), new Vector2(0.83f, 0.35f), out var retryText);
            var restartButton = CreateButton("RestartButton", defeatPanel.transform, font, "重新开始冒险", GamePalette.Hex("475569"), 34,
                new Vector2(0.17f, 0.05f), new Vector2(0.83f, 0.18f), out var restartText);

            ApplyBindings(view, combat, battleInfo.rectTransform, playerAvatar.rectTransform, playerName, attackPreview,
                enemyAvatar.rectTransform, enemyName, healthFill, enemyHealth, progressLine.rectTransform, encounterTitle,
                score, remainingShots, boardHint, normal, split, launch, hold, chargeFill, chargeText, launchText,
                hitFeedback, damageFeedback, settlementFeedback, resultOverlay, victoryPanel, victoryTitle, victoryMessage,
                victoryScore, victoryButton, victoryButtonText, defeatPanel, defeatTitle, defeatMessage, defeatScore,
                retryButton, retryText, restartButton, restartText);
            resultOverlay.SetActive(false);
            victoryPanel.SetActive(true);
            defeatPanel.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, GameplayPrefabPath);
            Object.DestroyImmediate(root.gameObject);
            RegisterPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Pinball Roguelite/UI/Import TMP Essential Resources")]
        public static void ImportTmpEssentials()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(ProjectTmpSettingsPath) != null) return;
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(TemporaryTmpSettingsPath) != null)
                AssetDatabase.DeleteAsset(TemporaryTmpSettingsPath);
            var package = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.ugui");
            if (package == null) throw new System.InvalidOperationException("The UGUI package is unavailable.");
            var packagePath = Path.Combine(package.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static TMP_FontAsset LoadOrCreateFont()
        {
            EnsureTmpSettings();
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null && AssetDatabase.LoadAllAssetsAtPath(FontAssetPath).Length >= 3) return existing;
            if (existing != null) AssetDatabase.DeleteAsset(FontAssetPath);
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/NotoSansSC-VF.ttf");
            if (source == null) throw new System.InvalidOperationException("The gameplay UI source font is missing.");
            var font = TMP_FontAsset.CreateFontAsset(source);
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(font, FontAssetPath);
            AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
            AssetDatabase.AddObjectToAsset(font.material, font);
            TMP_Settings.defaultFontAsset = font;
            EditorUtility.SetDirty(TMP_Settings.instance);
            AssetDatabase.SaveAssets();
            return font;
        }

        private static void EnsureTmpSettings()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(ProjectTmpSettingsPath) == null)
                ImportTmpEssentials();
            TMP_Settings.LoadDefaultSettings();
            if (TMP_Settings.instance == null)
                throw new System.InvalidOperationException("Unable to initialize TMP Settings.");
        }

        private static void RegisterPrefab()
        {
            var uiRoot = PrefabUtility.LoadPrefabContents(UiRootPrefabPath);
            var scaler = uiRoot.GetComponentInChildren<CanvasScaler>(true);
            if (scaler == null) throw new System.InvalidOperationException("UI.prefab must contain a CanvasScaler.");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(780f, 1688f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            var registry = uiRoot.GetComponent<GameplayUiPrefabRegistry>();
            if (registry == null) registry = uiRoot.AddComponent<GameplayUiPrefabRegistry>();
            var gameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayPrefabPath).GetComponent<GameplayUiView>();
            var serialized = new SerializedObject(registry);
            serialized.FindProperty("_gameplayUiPrefab").objectReferenceValue = gameplayPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(uiRoot, UiRootPrefabPath);
            PrefabUtility.UnloadPrefabContents(uiRoot);
        }

        private static void ApplyBindings(GameplayUiView view, CombatDisplay combat, RectTransform battleInfo,
            RectTransform playerAvatar, TMP_Text playerName, TMP_Text attackPreview, RectTransform enemyAvatar,
            TMP_Text enemyName, Image healthFill, TMP_Text enemyHealth, RectTransform progressLine, TMP_Text encounterTitle,
            TMP_Text score, TMP_Text remainingShots, TMP_Text boardHint, Button normal, Button split, Button launch,
            LaunchHoldButton hold, Image chargeFill, TMP_Text chargeText, TMP_Text launchText, TMP_Text hitFeedback,
            TMP_Text damageFeedback, TMP_Text settlementFeedback, GameObject resultOverlay, GameObject victoryPanel,
            TMP_Text victoryTitle, TMP_Text victoryMessage, TMP_Text victoryScore, Button victoryButton,
            TMP_Text victoryButtonText, GameObject defeatPanel, TMP_Text defeatTitle, TMP_Text defeatMessage,
            TMP_Text defeatScore, Button retryButton, TMP_Text retryText, Button restartButton, TMP_Text restartText)
        {
            var serialized = new SerializedObject(view);
            SetReference(serialized, "_combatDisplay", combat);
            SetReference(serialized, "_battleInfo", battleInfo);
            SetReference(serialized, "_playerAvatarPlaceholder", playerAvatar);
            SetReference(serialized, "_playerNameText", playerName);
            SetReference(serialized, "_attackPreviewText", attackPreview);
            SetReference(serialized, "_enemyAvatarPlaceholder", enemyAvatar);
            SetReference(serialized, "_enemyNameText", enemyName);
            SetReference(serialized, "_enemyHealthFill", healthFill);
            SetReference(serialized, "_enemyHealthText", enemyHealth);
            SetReference(serialized, "_progressLine", progressLine);
            SetReference(serialized, "_encounterTitleText", encounterTitle);
            SetReference(serialized, "_scoreText", score);
            SetReference(serialized, "_remainingShotsText", remainingShots);
            SetReference(serialized, "_boardHintText", boardHint);
            SetReference(serialized, "_normalMarbleButton", normal);
            SetReference(serialized, "_splitMarbleButton", split);
            SetReference(serialized, "_launchButton", launch);
            SetReference(serialized, "_launchHoldButton", hold);
            SetReference(serialized, "_chargeFill", chargeFill);
            SetReference(serialized, "_chargeValueText", chargeText);
            SetReference(serialized, "_launchButtonText", launchText);
            SetReference(serialized, "_hitFeedbackText", hitFeedback);
            SetReference(serialized, "_damageFeedbackText", damageFeedback);
            SetReference(serialized, "_settlementFeedbackText", settlementFeedback);
            SetReference(serialized, "_resultOverlay", resultOverlay);
            SetReference(serialized, "_victoryPanel", victoryPanel);
            SetReference(serialized, "_victoryTitleText", victoryTitle);
            SetReference(serialized, "_victoryMessageText", victoryMessage);
            SetReference(serialized, "_victoryScoreText", victoryScore);
            SetReference(serialized, "_victoryPrimaryButton", victoryButton);
            SetReference(serialized, "_victoryPrimaryText", victoryButtonText);
            SetReference(serialized, "_defeatPanel", defeatPanel);
            SetReference(serialized, "_defeatTitleText", defeatTitle);
            SetReference(serialized, "_defeatMessageText", defeatMessage);
            SetReference(serialized, "_defeatScoreText", defeatScore);
            SetReference(serialized, "_defeatRetryButton", retryButton);
            SetReference(serialized, "_defeatRetryText", retryText);
            SetReference(serialized, "_defeatRestartButton", restartButton);
            SetReference(serialized, "_defeatRestartText", restartText);
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

        private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, string value, float fontSize,
            Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            var text = CreateRect(name, parent, anchorMin, anchorMax).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, TMP_FontAsset font, string label, Color color,
            float fontSize, Vector2 anchorMin, Vector2 anchorMax, out TMP_Text labelText)
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
            labelText = CreateText("Label", image.transform, font, label, fontSize, Color.white,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            labelText.rectTransform.offsetMin = new Vector2(8f, 5f);
            labelText.rectTransform.offsetMax = new Vector2(-8f, -5f);
            return button;
        }
    }
}
