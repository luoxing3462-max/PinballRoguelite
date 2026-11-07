using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class GameplayUiView : MonoBehaviour
    {
        [Header("Combat bindings")]
        [SerializeField] private CombatDisplay _combatDisplay;
        [SerializeField] private BattleArtTheme _artTheme;
        [SerializeField] private RectTransform _battleInfo;
        [SerializeField] private RectTransform _playerAvatarPlaceholder;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _attackPreviewText;
        [SerializeField] private RectTransform _enemyAvatarPlaceholder;
        [SerializeField] private TMP_Text _enemyNameText;
        [SerializeField] private Image _enemyHealthFill;
        [SerializeField] private TMP_Text _enemyHealthText;
        [SerializeField] private RectTransform _progressLine;
        [SerializeField] private TMP_Text _encounterTitleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _remainingShotsText;

        [Header("Board and launch bindings")]
        [SerializeField] private TMP_Text _boardHintText;
        [SerializeField] private Button _normalMarbleButton;
        [SerializeField] private Button _splitMarbleButton;
        [SerializeField] private Button _launchButton;
        [SerializeField] private LaunchHoldButton _launchHoldButton;
        [SerializeField] private Image _chargeFill;
        [SerializeField] private TMP_Text _chargeValueText;
        [SerializeField] private TMP_Text _launchButtonText;

        [Header("Feedback bindings")]
        [SerializeField] private TMP_Text _hitFeedbackText;
        [SerializeField] private TMP_Text _damageFeedbackText;
        [SerializeField] private TMP_Text _settlementFeedbackText;
        private TMP_Text _effectStatusText;

        [Header("Result bindings")]
        [SerializeField] private GameObject _resultOverlay;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private TMP_Text _victoryTitleText;
        [SerializeField] private TMP_Text _victoryMessageText;
        [SerializeField] private TMP_Text _victoryScoreText;
        [SerializeField] private Button _victoryPrimaryButton;
        [SerializeField] private TMP_Text _victoryPrimaryText;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private TMP_Text _defeatTitleText;
        [SerializeField] private TMP_Text _defeatMessageText;
        [SerializeField] private TMP_Text _defeatScoreText;
        [SerializeField] private Button _defeatRetryButton;
        [SerializeField] private TMP_Text _defeatRetryText;
        [SerializeField] private Button _defeatRestartButton;
        [SerializeField] private TMP_Text _defeatRestartText;

        [Header("State colors")]
        [SerializeField] private Color _normalSelectedColor = new Color(0.05f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color _splitSelectedColor = new Color(0.93f, 0.33f, 0.61f, 1f);
        [SerializeField] private Color _unselectedColor = new Color(0.12f, 0.16f, 0.25f, 1f);

        private bool _initialized;
        private MarbleDefinition _firstMarble;
        private MarbleDefinition _secondMarble;

        public CombatDisplay CombatDisplay => _combatDisplay;
        public BattleArtCatalog ArtCatalog => _artTheme != null ? _artTheme.Catalog : null;
        public RectTransform BattleInfo => _battleInfo;
        public RectTransform PlayerAvatarPlaceholder => _playerAvatarPlaceholder;
        public TMP_Text PlayerNameText => _playerNameText;
        public TMP_Text AttackPreviewText => _attackPreviewText;
        public RectTransform EnemyAvatarPlaceholder => _enemyAvatarPlaceholder;
        public TMP_Text EnemyNameText => _enemyNameText;
        public Image EnemyHealthFill => _enemyHealthFill;
        public TMP_Text EnemyHealthText => _enemyHealthText;
        public RectTransform ProgressLine => _progressLine;
        public TMP_Text EncounterTitleText => _encounterTitleText;
        public TMP_Text ScoreText => _scoreText;
        public TMP_Text RemainingShotsText => _remainingShotsText;
        public TMP_Text DamageFeedbackText => _damageFeedbackText;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            if (_artTheme == null) _artTheme = GetComponent<BattleArtTheme>();
            _combatDisplay.Initialize(this);
            _resultOverlay.SetActive(false);
        }

        public void BindInput(Action normalSelected, Action splitSelected, Action chargeStarted, Action chargeReleased)
        {
            _normalMarbleButton.onClick.RemoveAllListeners();
            _splitMarbleButton.onClick.RemoveAllListeners();
            _normalMarbleButton.onClick.AddListener(() => normalSelected?.Invoke());
            _splitMarbleButton.onClick.AddListener(() => splitSelected?.Invoke());
            _launchHoldButton.Pressed = chargeStarted;
            _launchHoldButton.Released = chargeReleased;
        }

        public void ConfigureMarbleButtons(MarbleDefinition first, MarbleDefinition second)
        {
            _firstMarble = first;
            _secondMarble = second;
            ConfigureMarbleButton(_normalMarbleButton, first);
            ConfigureMarbleButton(_splitMarbleButton, second);
        }

        public void SetBoardHint(string value)
        {
            _boardHintText.text = value;
        }

        public void SetCharge(float value, bool isCharging)
        {
            var normalized = Mathf.Clamp01(value / 100f);
            _chargeFill.rectTransform.anchorMax = new Vector2(normalized, 1f);
            _chargeValueText.text = isCharging ? $"蓄力 {Mathf.RoundToInt(value)}%" : "蓄力";
        }

        public void SetLaunchLabel(string value)
        {
            _launchButtonText.text = value;
        }

        public void SetControls(string selectedKind, bool disabled, bool canLaunch)
        {
            _normalMarbleButton.interactable = !disabled;
            _splitMarbleButton.interactable = !disabled;
            _launchButton.interactable = !disabled && canLaunch;
            SetMarbleButtonVisual(_normalMarbleButton, selectedKind == _firstMarble.Id, _firstMarble.Color);
            SetMarbleButtonVisual(_splitMarbleButton, selectedKind == _secondMarble.Id, _secondMarble.Color);
        }

        public void ShowHitFeedback(string value)
        {
            _hitFeedbackText.text = value;
        }

        public void ShowDamageFeedback(string value)
        {
            _damageFeedbackText.text = value;
        }

        public void ShowSettlementFeedback(string value)
        {
            _settlementFeedbackText.text = value;
        }

        public void SetEffectStatus(string value)
        {
            if (_effectStatusText != null) _effectStatusText.text = value;
        }

        private static void ConfigureMarbleButton(Button button, MarbleDefinition definition)
        {
            if (button.image.sprite == null) button.image.color = definition.Color;
            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = definition.DisplayName + "\n" + definition.Description;
        }

        private void SetMarbleButtonVisual(Button button, bool selected, Color accent)
        {
            if (button == null || button.image == null) return;
            if (button.image.sprite == null)
            {
                button.image.color = selected ? accent : _unselectedColor;
                return;
            }

            button.image.color = selected ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
        }

        public void ShowResult(bool success, bool boss, int currentScore, int target, Action continuePressed,
            Action retryPressed, Action restartPressed)
        {
            _resultOverlay.SetActive(true);
            _victoryPanel.SetActive(success);
            _defeatPanel.SetActive(!success);
            if (success)
            {
                _victoryTitleText.text = boss ? "冒险完成！" : "关卡通过";
                _victoryTitleText.color = boss ? GamePalette.Yellow : GamePalette.Green;
                _victoryMessageText.text = boss
                    ? "最终 Boss 已被击败，所有路线汇聚成了胜利。"
                    : "目标积分已达成，可以返回地图选择下一条路线。";
                _victoryScoreText.text = $"关卡得分  {currentScore} / {target}";
                _victoryPrimaryText.text = boss ? "再来一局" : "返回地图";
                _victoryPrimaryButton.onClick.RemoveAllListeners();
                _victoryPrimaryButton.onClick.AddListener(() =>
                {
                    if (boss) restartPressed?.Invoke();
                    else continuePressed?.Invoke();
                });
                return;
            }

            _defeatTitleText.text = "挑战失败";
            _defeatMessageText.text = "玩家生命值已经归零，调整弹珠路线后再试一次。";
            _defeatScoreText.text = $"关卡得分  {currentScore} / {target}";
            _defeatRetryText.text = "重新挑战";
            _defeatRestartText.text = "重新开始冒险";
            _defeatRetryButton.onClick.RemoveAllListeners();
            _defeatRestartButton.onClick.RemoveAllListeners();
            _defeatRetryButton.onClick.AddListener(() => retryPressed?.Invoke());
            _defeatRestartButton.onClick.AddListener(() => restartPressed?.Invoke());
        }
    }
}
