using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    public sealed class EffectChoiceScreen : MonoBehaviour
    {
        public event Action<RunEffectId> ChoiceSelected;
        public void Setup(string title, string subtitle, IReadOnlyList<EffectOffer> offers)
        {
            UiFactory.CreateImage("Background", transform, GamePalette.Background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UiFactory.CreateText("Title", transform, title, 54, Color.white, TextAnchor.MiddleCenter, new Vector2(.06f, .83f), new Vector2(.94f, .95f), Vector2.zero, Vector2.zero);
            UiFactory.CreateText("Subtitle", transform, subtitle, 27, GamePalette.Cyan, TextAnchor.MiddleCenter, new Vector2(.08f, .75f), new Vector2(.92f, .83f), Vector2.zero, Vector2.zero);
            for (var index = 0; index < offers.Count; index++)
            {
                var offer = offers[index]; var top = .69f - index * .19f;
                UiFactory.CreateButton("Effect_" + offer.Id, transform, offer.Name + "\n" + offer.Description, index == 0 ? GamePalette.Blue : GamePalette.PanelLight, 29,
                    new Vector2(.10f, top - .15f), new Vector2(.90f, top), Vector2.zero, Vector2.zero, () => ChoiceSelected?.Invoke(offer.Id));
            }
        }
    }
}
