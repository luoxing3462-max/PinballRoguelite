using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public static class UiFactory
    {
        private static Font runtimeFont;

        public static Font RuntimeFont
        {
            get
            {
                if (runtimeFont != null) return runtimeFont;
                runtimeFont = Resources.Load<Font>("Fonts/NotoSansSC-VF");
                if (runtimeFont == null)
                    runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return runtimeFont;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string value, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = RuntimeFont;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, UnityAction onClick = null)
        {
            var image = CreateImage(name, parent, color, anchorMin, anchorMax, offsetMin, offsetMax);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(0.20f, 0.24f, 0.31f, 0.8f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);
            CreateText("Label", image.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(8, 5), new Vector2(-8, -5));
            return button;
        }

        public static Image CreateLine(Transform parent, Vector2 from, Vector2 to, Color color, float thickness)
        {
            var image = CreateImage("Connection", parent, color, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var rect = image.rectTransform;
            var delta = to - from;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = from;
            rect.sizeDelta = new Vector2(delta.magnitude, thickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            image.raycastTarget = false;
            return image;
        }
    }
}
