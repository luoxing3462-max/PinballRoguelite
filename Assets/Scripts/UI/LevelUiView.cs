using System;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class LevelUiView : MonoBehaviour
    {
        public event Action ContinuePressed;

        [SerializeField] private RectTransform _mapArea;
        [SerializeField] private Text _progressText;
        [SerializeField] private GameObject _placeholderModal;
        [SerializeField] private Text _placeholderTitleText;
        [SerializeField] private Text _placeholderBodyText;
        [SerializeField] private Button _placeholderContinueButton;

        public RectTransform MapArea => _mapArea;

        public void Initialize()
        {
            _placeholderContinueButton.onClick.RemoveAllListeners();
            _placeholderContinueButton.onClick.AddListener(OnContinuePressed);
            _placeholderModal.SetActive(false);
        }

        public void ClearMap()
        {
            foreach (Transform child in _mapArea)
                Destroy(child.gameObject);
        }

        public void SetProgress(string value)
        {
            _progressText.text = value;
        }

        public void CreateConnection(Vector2 from, Vector2 to, Color color)
        {
            UiFactory.CreateLine(_mapArea, from, to, color, 8f);
        }

        public Button CreateNodeButton(string nodeId, string symbol, string typeLabel, Color color, bool available,
            bool completed, bool boss, Vector2 position, Action onClick)
        {
            var size = boss ? 116f : 94f;
            var button = UiFactory.CreateButton(nodeId, _mapArea, completed ? "✓" : symbol, color, boss ? 44 : 38,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position - Vector2.one * size * 0.5f,
                position + Vector2.one * size * 0.5f, () => onClick?.Invoke());
            button.interactable = available;
            button.transform.SetAsLastSibling();
            var label = UiFactory.CreateText("TypeLabel", button.transform, typeLabel, 20,
                available ? Color.white : GamePalette.Muted, TextAnchor.MiddleCenter,
                new Vector2(-0.25f, -0.42f), new Vector2(1.25f, -0.02f), Vector2.zero, Vector2.zero);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            return button;
        }

        public void ShowPlaceholder(string title, Color titleColor, string body)
        {
            _placeholderTitleText.text = title;
            _placeholderTitleText.color = titleColor;
            _placeholderBodyText.text = body;
            _placeholderModal.SetActive(true);
        }

        private void OnContinuePressed()
        {
            _placeholderModal.SetActive(false);
            ContinuePressed?.Invoke();
        }
    }
}
