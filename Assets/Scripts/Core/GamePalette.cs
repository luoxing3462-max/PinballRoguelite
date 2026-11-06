using UnityEngine;

namespace PinballRoguelite
{
    public static class GamePalette
    {
        public static readonly Color Background = Hex("07111F");
        public static readonly Color Panel = Hex("111827");
        public static readonly Color PanelLight = Hex("1E293B");
        public static readonly Color Muted = Hex("94A3B8");
        public static readonly Color Cyan = Hex("22D3EE");
        public static readonly Color Blue = Hex("38BDF8");
        public static readonly Color Pink = Hex("F472B6");
        public static readonly Color Green = Hex("4ADE80");
        public static readonly Color Orange = Hex("F97316");
        public static readonly Color Purple = Hex("A78BFA");
        public static readonly Color Yellow = Hex("FACC15");
        public static readonly Color Red = Hex("EF4444");

        public static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value.TrimStart('#'), out var color);
            return color;
        }
    }
}
