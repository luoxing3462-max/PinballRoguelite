using System;
using UnityEngine;

namespace PinballRoguelite
{
    public sealed class TitleScreen : MonoBehaviour
    {
        public event Action StartPressed;

        private void Awake()
        {
            UiFactory.CreateImage("Background", transform, GamePalette.Panel, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);

            UiFactory.CreateText("Marble", transform, "●", 170, GamePalette.Cyan, TextAnchor.MiddleCenter,
                new Vector2(0.15f, 0.70f), new Vector2(0.85f, 0.84f), Vector2.zero, Vector2.zero);
            UiFactory.CreateText("Title", transform, "弹珠爬塔", 92, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero);
            UiFactory.CreateText("Subtitle", transform, "竖屏物理弹珠 Roguelite 原型", 38, GamePalette.Muted,
                TextAnchor.MiddleCenter, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.57f), Vector2.zero, Vector2.zero);

            UiFactory.CreateButton("StartButton", transform, "开始冒险", GamePalette.Blue, 44,
                new Vector2(0.20f, 0.30f), new Vector2(0.80f, 0.39f), Vector2.zero, Vector2.zero,
                () => StartPressed?.Invoke());

            UiFactory.CreateText("Tip", transform, "选择弹珠 · 按住蓄力 · 碰柱得分 · 挑战 Boss", 28,
                GamePalette.Muted, TextAnchor.MiddleCenter, new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.28f),
                Vector2.zero, Vector2.zero);
        }
    }
}
