using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class MarbleSelectionScreen : MonoBehaviour
    {
        public event Action<IReadOnlyList<string>> SelectionConfirmed;

        private readonly List<string> selectedIds = new List<string>();
        private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        private Button confirmButton;
        private Text selectionText;

        public void Setup()
        {
            UiFactory.CreateImage("Background", transform, GamePalette.Background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.CreateText("Title", transform, "选择本轮出战弹珠", 52, Color.white, TextAnchor.MiddleCenter, new Vector2(.06f, .87f), new Vector2(.94f, .96f), Vector2.zero, Vector2.zero);
            selectionText = UiFactory.CreateText("SelectionText", transform, "请选择 2 种弹珠（0 / 2）", 28, GamePalette.Cyan, TextAnchor.MiddleCenter, new Vector2(.08f, .81f), new Vector2(.92f, .87f), Vector2.zero, Vector2.zero);

            for (var index = 0; index < MarbleCatalog.All.Count; index++)
            {
                var definition = MarbleCatalog.All[index];
                var column = index % 2;
                var row = index / 2;
                var left = .07f + column * .45f;
                var top = .76f - row * .20f;
                var button = UiFactory.CreateButton("Marble_" + definition.Id, transform, "●  " + definition.DisplayName + "\n" + definition.Description,
                    definition.Color, 20, new Vector2(left, top - .16f), new Vector2(left + .40f, top), Vector2.zero, Vector2.zero,
                    () => Toggle(definition.Id));
                buttons.Add(definition.Id, button);
            }

            confirmButton = UiFactory.CreateButton("ConfirmButton", transform, "确认出战", GamePalette.Green, 38, new Vector2(.22f, .08f), new Vector2(.78f, .16f), Vector2.zero, Vector2.zero, Confirm);
            Refresh();
        }

        private void Toggle(string id)
        {
            if (selectedIds.Contains(id)) selectedIds.Remove(id);
            else if (selectedIds.Count < 2) selectedIds.Add(id);
            Refresh();
        }

        private void Refresh()
        {
            selectionText.text = $"请选择 2 种弹珠（{selectedIds.Count} / 2）";
            foreach (var pair in buttons)
            {
                var selected = selectedIds.Contains(pair.Key);
                pair.Value.image.color = selected ? Color.Lerp(MarbleCatalog.Get(pair.Key).Color, Color.white, .15f) : new Color(.10f, .14f, .22f, 1f);
            }
            confirmButton.interactable = selectedIds.Count == 2;
        }

        private void Confirm()
        {
            if (selectedIds.Count != 2) return;
            SelectionConfirmed?.Invoke(new List<string>(selectedIds));
        }
    }
}
