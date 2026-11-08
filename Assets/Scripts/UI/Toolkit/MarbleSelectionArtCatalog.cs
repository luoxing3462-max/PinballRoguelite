using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    [CreateAssetMenu(menuName = "Pinball Roguelite/UI/Marble Selection Art Catalog")]
    public sealed class MarbleSelectionArtCatalog : ScriptableObject
    {
        [Serializable]
        private sealed class CardArt
        {
            [SerializeField] private string _marbleId;
            [SerializeField] private Sprite _card;
            [SerializeField] private Sprite _icon;

            public string MarbleId => _marbleId;
            public Sprite Card => _card;
            public Sprite Icon => _icon;
        }

        [SerializeField] private Sprite _background;
        [SerializeField] private Sprite _title;
        [SerializeField] private Sprite _instructionPanel;
        [SerializeField] private Sprite _footer;
        [SerializeField] private Sprite _selectionCheck;
        [SerializeField] private Sprite _confirmButton;
        [SerializeField] private List<CardArt> _cards = new List<CardArt>();

        public Sprite Background => _background;
        public Sprite Title => _title;
        public Sprite InstructionPanel => _instructionPanel;
        public Sprite Footer => _footer;
        public Sprite SelectionCheck => _selectionCheck;
        public Sprite ConfirmButton => _confirmButton;

        public bool TryGetCard(string marbleId, out Sprite card, out Sprite icon)
        {
            foreach (CardArt entry in _cards)
            {
                if (!string.Equals(entry.MarbleId, marbleId, StringComparison.Ordinal)) continue;
                card = entry.Card;
                icon = entry.Icon;
                return card != null && icon != null;
            }

            card = null;
            icon = null;
            return false;
        }
    }
}
