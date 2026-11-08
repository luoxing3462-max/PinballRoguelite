using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinballRoguelite
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MarbleSelectionUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private VisualTreeAsset _cardTemplate;
        [SerializeField] private MarbleSelectionArtCatalog _artCatalog;

        private readonly Dictionary<string, Button> _cardButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, VisualElement> _checks = new Dictionary<string, VisualElement>();
        private VisualElement _root;
        private VisualElement _backgroundArt;
        private VisualElement _titleArt;
        private VisualElement _instructionPanelArt;
        private VisualElement _footerArt;
        private VisualElement _cardContainer;
        private Label _selectionCount;
        private Button _confirmButton;
        private Action<string> _cardClicked;
        private Action _confirmClicked;
        private bool _confirmAttached;

        private void Awake()
        {
            if (_document == null) _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindVisualElements();
        }

        private void OnDisable()
        {
            if (_confirmAttached && _confirmButton != null) _confirmButton.clicked -= OnConfirmClicked;
            _confirmAttached = false;
            _cardClicked = null;
            _confirmClicked = null;
        }

        public void Initialize(IReadOnlyList<MarbleDefinition> definitions, Action<string> cardClicked, Action confirmClicked)
        {
            BindVisualElements();
            if (_root == null || _cardContainer == null || _selectionCount == null || _confirmButton == null || _cardTemplate == null || _artCatalog == null)
                throw new MissingReferenceException("MarbleSelectionUI requires its UXML, card template and art catalog bindings.");

            _cardClicked = cardClicked;
            _confirmClicked = confirmClicked;
            BuildCards(definitions);
            Show();
            Refresh(Array.Empty<string>());
        }

        public void Refresh(IReadOnlyCollection<string> selectedIds)
        {
            if (_selectionCount == null || _confirmButton == null) return;
            var selected = selectedIds ?? Array.Empty<string>();
            _selectionCount.text = $"请选择 2 种弹珠（{selected.Count} / 2）";
            _confirmButton.SetEnabled(selected.Count == 2);
            foreach (KeyValuePair<string, Button> item in _cardButtons)
            {
                bool isSelected = Contains(selected, item.Key);
                item.Value.EnableInClassList("is-selected", isSelected);
                if (_checks.TryGetValue(item.Key, out VisualElement check))
                    check.EnableInClassList("is-hidden", !isSelected);
            }
        }

        public void Show()
        {
            BindVisualElements();
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        private void BindVisualElements()
        {
            if (_root != null || _document == null) return;
            _root = _document.rootVisualElement;
            _backgroundArt = _root.Q<VisualElement>("background-art");
            _titleArt = _root.Q<VisualElement>("title-art");
            _instructionPanelArt = _root.Q<VisualElement>("instruction-panel-art");
            _footerArt = _root.Q<VisualElement>("footer-art");
            _cardContainer = _root.Q<VisualElement>("marble-card-container");
            _selectionCount = _root.Q<Label>("selection-count");
            _confirmButton = _root.Q<Button>("confirm-button");
            if (_confirmButton != null && !_confirmAttached)
            {
                _confirmButton.clicked += OnConfirmClicked;
                _confirmAttached = true;
            }
            ApplyStaticArtwork();
        }

        private void ApplyStaticArtwork()
        {
            if (_artCatalog == null) return;
            if (_backgroundArt != null) _backgroundArt.style.backgroundImage = new StyleBackground(_artCatalog.Background);
            if (_titleArt != null) _titleArt.style.backgroundImage = new StyleBackground(_artCatalog.Title);
            if (_instructionPanelArt != null) _instructionPanelArt.style.backgroundImage = new StyleBackground(_artCatalog.InstructionPanel);
            if (_footerArt != null) _footerArt.style.backgroundImage = new StyleBackground(_artCatalog.Footer);
            if (_confirmButton != null) _confirmButton.style.backgroundImage = new StyleBackground(_artCatalog.ConfirmButton);
        }

        private void BuildCards(IReadOnlyList<MarbleDefinition> definitions)
        {
            _cardContainer.Clear();
            _cardButtons.Clear();
            _checks.Clear();
            foreach (MarbleDefinition definition in definitions)
            {
                TemplateContainer instance = _cardTemplate.Instantiate();
                Button button = instance.Q<Button>("marble-card-button");
                VisualElement cardArt = instance.Q<VisualElement>("card-art");
                VisualElement icon = instance.Q<VisualElement>("marble-icon");
                VisualElement check = instance.Q<VisualElement>("selection-check");
                Label name = instance.Q<Label>("marble-name");
                Label description = instance.Q<Label>("marble-description");
                if (button == null || cardArt == null || icon == null || check == null || name == null || description == null)
                    throw new MissingReferenceException("MarbleCard.uxml is missing a required named element.");
                if (!_artCatalog.TryGetCard(definition.Id, out Sprite cardSprite, out Sprite iconSprite))
                    throw new MissingReferenceException($"Missing PSD art for marble card '{definition.Id}'.");

                button.AddToClassList(definition.Id);
                cardArt.style.backgroundImage = new StyleBackground(cardSprite);
                icon.style.backgroundImage = new StyleBackground(iconSprite);
                check.style.backgroundImage = new StyleBackground(_artCatalog.SelectionCheck);
                name.text = definition.DisplayName;
                description.text = definition.Description;
                check.AddToClassList("is-hidden");
                string marbleId = definition.Id;
                button.clicked += () => _cardClicked?.Invoke(marbleId);
                _cardButtons.Add(marbleId, button);
                _checks.Add(marbleId, check);
                _cardContainer.Add(instance);
            }
        }

        private void OnConfirmClicked()
        {
            _confirmClicked?.Invoke();
        }

        private static bool Contains(IReadOnlyCollection<string> collection, string value)
        {
            foreach (string item in collection)
                if (string.Equals(item, value, StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
