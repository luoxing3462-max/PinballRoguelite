using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class CombatDisplay : MonoBehaviour
    {
        private const float PlayerAnchorX = 0.14f;
        private const float EnemyStartAnchorX = 0.86f;
        private const float EnemyEndAnchorX = 0.46f;
        private const float FighterAnchorY = 0.29f;

        public int MaxHealth { get; private set; } = 1;
        public int CurrentHealth { get; private set; } = 1;
        public int LastDamage { get; private set; }
        public const int DefaultPlayerMaxHealth = 100;
        public int PlayerMaxHealth { get; private set; } = DefaultPlayerMaxHealth;
        public int PlayerCurrentHealth { get; private set; } = DefaultPlayerMaxHealth;
        public float EnemyProgress { get; private set; }
        public bool IsEnemyAtPlayer => EnemyProgress >= 0.999f;
        public float PlayerPositionX => playerRoot != null ? playerRoot.anchorMin.x : PlayerAnchorX;
        public float EnemyPositionX => enemyRoot != null ? enemyRoot.anchorMin.x : EnemyStartAnchorX;
        public int StepMarkerCount => stepMarkers.Count;
        public bool IsAnimating { get; private set; }

        private readonly List<Image> stepMarkers = new List<Image>();
        private readonly List<Image> enemyTintParts = new List<Image>();

        private RectTransform root;
        private RectTransform playerRoot;
        private RectTransform enemyRoot;
        private RectTransform markerRoot;
        private TMP_Text encounterLabel;
        private TMP_Text scoreLabel;
        private TMP_Text shotsLabel;
        private TMP_Text pendingLabel;
        private TMP_Text enemyNameLabel;
        private TMP_Text healthLabel;
        private Image healthFill;
        private Image playerHealthFill;
        private TMP_Text playerHealthLabel;
        private GameplayUiView view;

        private MapNodeType levelType;
        private Color enemyColor = GamePalette.Green;
        private int totalShots = 1;

        public void Initialize(GameplayUiView gameplayUiView)
        {
            if (view != null) return;
            view = gameplayUiView;
            root = view.BattleInfo;
            if (root.GetComponent<RectMask2D>() == null)
                root.gameObject.AddComponent<RectMask2D>();
            playerRoot = view.PlayerAvatarPlaceholder;
            enemyRoot = view.EnemyAvatarPlaceholder;
            markerRoot = view.ProgressLine;
            encounterLabel = view.EncounterTitleText;
            scoreLabel = view.ScoreText;
            shotsLabel = view.RemainingShotsText;
            pendingLabel = view.AttackPreviewText;
            enemyNameLabel = view.EnemyNameText;
            healthLabel = view.EnemyHealthText;
            healthFill = view.EnemyHealthFill;
            view.PlayerNameText.text = "弹珠勇者";
            BuildPlayerVisual();
            BuildPlayerHealthBar();
        }

        public void Configure(MapNodeType newLevelType, string title, int maxHealth, Color accent, int shotLimit)
        {
            levelType = newLevelType;
            MaxHealth = Mathf.Max(1, maxHealth);
            CurrentHealth = MaxHealth;
            totalShots = Mathf.Max(1, shotLimit);
            LastDamage = 0;
            EnemyProgress = 0f;
            encounterLabel.text = $"{title} · 战斗区";
            encounterLabel.color = accent;

            switch (levelType)
            {
                case MapNodeType.Elite:
                    enemyNameLabel.text = "铁甲尖兵";
                    enemyColor = GamePalette.Orange;
                    break;
                case MapNodeType.Boss:
                    enemyNameLabel.text = "深渊巨像";
                    enemyColor = GamePalette.Red;
                    break;
                default:
                    enemyNameLabel.text = "洞穴史莱姆";
                    enemyColor = GamePalette.Green;
                    break;
            }

            RebuildEnemyVisual();
            RebuildStepMarkers();
            SetEnemyProgress(0f);
            SetHealthFromTotal(0);
            ConfigurePlayerHealth(DefaultPlayerMaxHealth);
            SetHudState(0, 0, totalShots);
        }

        public void ConfigurePlayerHealth(int maxHealth)
        {
            PlayerMaxHealth = Mathf.Max(1, maxHealth);
            PlayerCurrentHealth = PlayerMaxHealth;
            UpdatePlayerHealthVisual();
        }

        public void SetPlayerHealth(int currentHealth)
        {
            PlayerCurrentHealth = Mathf.Clamp(currentHealth, 0, PlayerMaxHealth);
            UpdatePlayerHealthVisual();
        }

        public void SetHudState(int totalScore, int pendingDamage, int remainingShots)
        {
            scoreLabel.text = $"积分  {Mathf.Max(0, totalScore)} / {MaxHealth}";
            shotsLabel.text = $"剩余发射  {Mathf.Max(0, remainingShots)} / {totalShots}";
            pendingLabel.text = $"本次攻击  {Mathf.Max(0, pendingDamage)}";
        }

        public void SetHealthFromTotal(int totalDamage)
        {
            CurrentHealth = Mathf.Max(0, MaxHealth - Mathf.Max(0, totalDamage));
            UpdateHealthVisual();
        }

        public IEnumerator PlayAttack(int damage, int shotIndex)
        {
            IsAnimating = true;
            LastDamage = Mathf.Max(0, damage);

            var projectile = CreateCircle("AttackProjectile", root, GamePalette.Hex("67E8F9"),
                new Vector2(22f, 22f), Vector2.zero);
            SetAnchor(projectile.rectTransform, PlayerAnchorX, FighterAnchorY);
            projectile.transform.SetAsLastSibling();

            var start = new Vector2(PlayerAnchorX, FighterAnchorY);
            var target = new Vector2(EnemyPositionX, FighterAnchorY);
            var elapsed = 0f;
            while (elapsed < 0.28f)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / 0.28f);
                var eased = normalized * normalized;
                var anchor = Vector2.LerpUnclamped(start, target, eased);
                SetAnchor(projectile.rectTransform, anchor.x, anchor.y);
                yield return null;
            }
            Destroy(projectile.gameObject);

            CurrentHealth = Mathf.Max(0, CurrentHealth - LastDamage);
            UpdateHealthVisual();
            view.ShowDamageFeedback(LastDamage > 0 ? $"攻击 -{LastDamage}" : "攻击被格挡");

            elapsed = 0f;
            while (elapsed < 0.36f)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / 0.36f);
                var strength = 1f - normalized;
                SetEnemyTint(Color.Lerp(enemyColor, Color.white, strength * 0.85f));
                enemyRoot.anchoredPosition = new Vector2(
                    Mathf.Sin(strength * Mathf.PI * 6f) * 11f * strength, 0f);
                yield return null;
            }
            SetEnemyTint(enemyColor);
            enemyRoot.anchoredPosition = Vector2.zero;

            if (CurrentHealth > 0)
            {
                var startProgress = EnemyProgress;
                var targetProgress = Mathf.Clamp01((float)shotIndex / totalShots);
                elapsed = 0f;
                while (elapsed < 0.28f)
                {
                    elapsed += Time.deltaTime;
                    var normalized = Mathf.Clamp01(elapsed / 0.28f);
                    var eased = EaseOutBack(normalized);
                    SetEnemyProgress(Mathf.LerpUnclamped(startProgress, targetProgress, eased));
                    yield return null;
                }
                SetEnemyProgress(targetProgress);
            }

            IsAnimating = false;
        }

        public IEnumerator PlayEnemyAttack(int damage)
        {
            IsAnimating = true;
            LastDamage = Mathf.Max(0, damage);
            var projectile = CreateCircle("EnemyAttackProjectile", root, GamePalette.Pink, new Vector2(22f, 22f), Vector2.zero);
            SetAnchor(projectile.rectTransform, EnemyPositionX, FighterAnchorY);
            projectile.transform.SetAsLastSibling();

            var start = new Vector2(EnemyPositionX, FighterAnchorY);
            var target = new Vector2(PlayerAnchorX, FighterAnchorY);
            var elapsed = 0f;
            while (elapsed < 0.28f)
            {
                elapsed += Time.deltaTime;
                var normalized = Mathf.Clamp01(elapsed / 0.28f);
                var anchor = Vector2.LerpUnclamped(start, target, normalized * normalized);
                SetAnchor(projectile.rectTransform, anchor.x, anchor.y);
                yield return null;
            }
            Destroy(projectile.gameObject);

            PlayerCurrentHealth = Mathf.Max(0, PlayerCurrentHealth - LastDamage);
            UpdatePlayerHealthVisual();
            view.ShowDamageFeedback(LastDamage > 0 ? $"怪物攻击 -{LastDamage}" : "怪物攻击被格挡");

            elapsed = 0f;
            while (elapsed < 0.28f)
            {
                elapsed += Time.deltaTime;
                var strength = 1f - Mathf.Clamp01(elapsed / 0.28f);
                playerRoot.anchoredPosition = new Vector2(-Mathf.Sin(strength * Mathf.PI * 6f) * 10f * strength, 0f);
                yield return null;
            }
            playerRoot.anchoredPosition = Vector2.zero;
            IsAnimating = false;
        }

        private void BuildPlayerVisual()
        {
            CreateCircle("Shadow", playerRoot, new Color(0f, 0f, 0f, 0.35f), new Vector2(72f, 20f), new Vector2(0f, -38f));
            CreateCircle("Body", playerRoot, GamePalette.Hex("0E7490"), new Vector2(62f, 62f), new Vector2(0f, -2f));
            CreateCircle("Head", playerRoot, GamePalette.Hex("F8FAFC"), new Vector2(39f, 39f), new Vector2(0f, 18f));
            CreateCircle("EyeLeft", playerRoot, GamePalette.Hex("0F172A"), new Vector2(7f, 7f), new Vector2(-7f, 21f));
            CreateCircle("EyeRight", playerRoot, GamePalette.Hex("0F172A"), new Vector2(7f, 7f), new Vector2(7f, 21f));
            CreateCircle("Shield", playerRoot, GamePalette.Cyan, new Vector2(34f, 34f), new Vector2(-27f, -4f));
            CreateCircle("ShieldInset", playerRoot, GamePalette.Hex("0E7490"), new Vector2(22f, 22f), new Vector2(-27f, -4f));
            CreateFixedImage("Sword", playerRoot, GamePalette.Hex("F8FAFC"), new Vector2(7f, 48f),
                new Vector2(31f, 16f), -42f);
            CreateFixedImage("SwordGlow", playerRoot, GamePalette.Hex("67E8F9"), new Vector2(16f, 6f),
                new Vector2(48f, 35f), -42f);
        }

        private void BuildPlayerHealthBar()
        {
            var back = CreateFixedImage("PlayerHealthBar", playerRoot, GamePalette.Hex("020617"), new Vector2(92f, 11f),
                new Vector2(0f, -53f), 0f);
            playerHealthFill = CreateFixedImage("PlayerHealthFill", back.transform, GamePalette.Cyan, new Vector2(88f, 7f),
                Vector2.zero, 0f);
            playerHealthFill.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            playerHealthFill.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            playerHealthFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            var labelObject = new GameObject("PlayerHealthText", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(playerRoot, false);
            playerHealthLabel = labelObject.GetComponent<TextMeshProUGUI>();
            playerHealthLabel.font = view.PlayerNameText.font;
            playerHealthLabel.fontSize = 14f;
            playerHealthLabel.color = Color.white;
            playerHealthLabel.alignment = TextAlignmentOptions.Center;
            playerHealthLabel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            playerHealthLabel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playerHealthLabel.rectTransform.sizeDelta = new Vector2(100f, 20f);
            playerHealthLabel.rectTransform.anchoredPosition = new Vector2(0f, -70f);
            playerHealthLabel.raycastTarget = false;
        }

        private void RebuildEnemyVisual()
        {
            enemyTintParts.Clear();
            for (var index = enemyRoot.childCount - 1; index >= 0; index--)
            {
                var child = enemyRoot.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            CreateCircle("Shadow", enemyRoot, new Color(0f, 0f, 0f, 0.35f), new Vector2(88f, 22f), new Vector2(0f, -40f));
            if (levelType == MapNodeType.Boss)
            {
                CreateCircle("Aura", enemyRoot, new Color(enemyColor.r, enemyColor.g, enemyColor.b, 0.35f),
                    new Vector2(104f, 104f), Vector2.zero);
                AddEnemyTintPart(CreateCircle("Body", enemyRoot, enemyColor, new Vector2(80f, 80f), Vector2.zero));
                CreateFixedImage("CrownLeft", enemyRoot, GamePalette.Yellow, new Vector2(20f, 28f),
                    new Vector2(-23f, 43f), -18f);
                CreateFixedImage("CrownCenter", enemyRoot, GamePalette.Yellow, new Vector2(20f, 34f),
                    new Vector2(0f, 48f), 0f);
                CreateFixedImage("CrownRight", enemyRoot, GamePalette.Yellow, new Vector2(20f, 28f),
                    new Vector2(23f, 43f), 18f);
            }
            else if (levelType == MapNodeType.Elite)
            {
                CreateCircle("ArmorBorder", enemyRoot, GamePalette.Hex("FED7AA"), new Vector2(94f, 94f), Vector2.zero);
                AddEnemyTintPart(CreateCircle("Armor", enemyRoot, enemyColor, new Vector2(82f, 82f), Vector2.zero));
                CreateFixedImage("ArmorTop", enemyRoot, enemyColor, new Vector2(58f, 22f), new Vector2(0f, 39f), 0f);
            }
            else
            {
                AddEnemyTintPart(CreateCircle("EarLeft", enemyRoot, enemyColor, new Vector2(31f, 31f), new Vector2(-25f, 25f)));
                AddEnemyTintPart(CreateCircle("EarRight", enemyRoot, enemyColor, new Vector2(31f, 31f), new Vector2(25f, 25f)));
                AddEnemyTintPart(CreateCircle("Body", enemyRoot, enemyColor, new Vector2(82f, 82f), Vector2.zero));
            }

            CreateCircle("EyeLeft", enemyRoot, GamePalette.Hex(levelType == MapNodeType.Boss ? "FEF2F2" : "F0FDF4"),
                new Vector2(11f, 11f), new Vector2(-14f, 8f));
            CreateCircle("EyeRight", enemyRoot, GamePalette.Hex(levelType == MapNodeType.Boss ? "FEF2F2" : "F0FDF4"),
                new Vector2(11f, 11f), new Vector2(14f, 8f));
            CreateFixedImage("Mouth", enemyRoot, GamePalette.Hex(levelType == MapNodeType.Boss ? "7F1D1D" : "14532D"),
                new Vector2(32f, 6f), new Vector2(0f, -17f), 0f);
        }

        private void RebuildStepMarkers()
        {
            stepMarkers.Clear();
            for (var index = markerRoot.childCount - 1; index >= 0; index--)
            {
                var child = markerRoot.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            for (var index = 0; index <= totalShots; index++)
            {
                var ratio = (float)index / totalShots;
                var anchorX = Mathf.Lerp(EnemyStartAnchorX, EnemyEndAnchorX, ratio);
                var marker = CreateCircle($"Step{index}", markerRoot, GamePalette.Hex("64748B"),
                    new Vector2(13f, 13f), Vector2.zero);
                SetAnchor(marker.rectTransform, anchorX, 0.212f);
                stepMarkers.Add(marker);
            }
            UpdateStepMarkers();
        }

        private void SetEnemyProgress(float value)
        {
            EnemyProgress = Mathf.Clamp01(value);
            SetAnchor(enemyRoot, Mathf.Lerp(EnemyStartAnchorX, EnemyEndAnchorX, EnemyProgress), FighterAnchorY);
            UpdateStepMarkers();
        }

        private void UpdateStepMarkers()
        {
            for (var index = 0; index < stepMarkers.Count; index++)
            {
                var ratio = totalShots > 0 ? (float)index / totalShots : 0f;
                stepMarkers[index].color = ratio <= EnemyProgress + 0.001f
                    ? GamePalette.Hex("FB7185")
                    : GamePalette.Hex("64748B");
            }
        }

        private void UpdateHealthVisual()
        {
            healthLabel.text = $"{CurrentHealth} / {MaxHealth}";
            var ratio = MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
            healthFill.color = enemyColor;
            healthFill.rectTransform.anchorMax = new Vector2(0.015f + 0.97f * Mathf.Clamp01(ratio), 0.82f);
        }

        private void UpdatePlayerHealthVisual()
        {
            if (playerHealthFill == null || playerHealthLabel == null) return;
            var ratio = PlayerMaxHealth > 0 ? (float)PlayerCurrentHealth / PlayerMaxHealth : 0f;
            playerHealthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(ratio), 0.5f);
            playerHealthLabel.text = $"{PlayerCurrentHealth} / {PlayerMaxHealth}";
        }

        private void AddEnemyTintPart(Image image)
        {
            enemyTintParts.Add(image);
        }

        private void SetEnemyTint(Color color)
        {
            foreach (var image in enemyTintParts)
            {
                if (image != null) image.color = color;
            }
        }

        private static Image CreateCircle(string objectName, Transform parent, Color color, Vector2 size, Vector2 position)
        {
            var image = CreateFixedImage(objectName, parent, color, size, position, 0f);
            image.sprite = SpriteFactory.Circle;
            image.preserveAspect = false;
            return image;
        }

        private static Image CreateFixedImage(string objectName, Transform parent, Color color, Vector2 size,
            Vector2 position, float rotation)
        {
            var image = UiFactory.CreateImage(objectName, parent, color, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            image.rectTransform.sizeDelta = size;
            image.rectTransform.anchoredPosition = position;
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            image.raycastTarget = false;
            return image;
        }

        private static void SetAnchor(RectTransform rect, float x, float y)
        {
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x, y);
            rect.anchoredPosition = Vector2.zero;
        }

        private static float EaseOutBack(float value)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var shifted = value - 1f;
            return 1f + c3 * shifted * shifted * shifted + c1 * shifted * shifted;
        }
    }
}
