using System;
using UnityEngine;
using UnityEngine.UI;

namespace PinballRoguelite
{
    public sealed class ResultOverlay : MonoBehaviour
    {
        public event Action ContinuePressed;
        public event Action RetryPressed;
        public event Action RestartPressed;
        private Text title;
        private Text message;
        private Text score;
        private Button primary;
        private Button secondary;
        private Text primaryLabel;
        private Text secondaryLabel;

        private void Awake()
        {
            UiFactory.CreateImage("Shade", transform, new Color(0.01f, 0.02f, 0.05f, 0.92f), Vector2.zero,
                Vector2.one, Vector2.zero, Vector2.zero);
            var panel = UiFactory.CreateImage("Panel", transform, GamePalette.PanelLight,
                new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.77f), Vector2.zero, Vector2.zero);
            title = UiFactory.CreateText("Title", panel.transform, "", 62, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.06f, 0.75f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);
            message = UiFactory.CreateText("Message", panel.transform, "", 32, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.51f), new Vector2(0.92f, 0.75f), Vector2.zero, Vector2.zero);
            score = UiFactory.CreateText("Score", panel.transform, "", 38, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.39f), new Vector2(0.92f, 0.53f), Vector2.zero, Vector2.zero);
            primary = UiFactory.CreateButton("Primary", panel.transform, "继续", GamePalette.Blue, 36,
                new Vector2(0.17f, 0.21f), new Vector2(0.83f, 0.35f), Vector2.zero, Vector2.zero);
            secondary = UiFactory.CreateButton("Secondary", panel.transform, "重新开始冒险", GamePalette.Hex("475569"),
                34, new Vector2(0.17f, 0.05f), new Vector2(0.83f, 0.18f), Vector2.zero, Vector2.zero);
            primaryLabel = primary.GetComponentInChildren<Text>();
            secondaryLabel = secondary.GetComponentInChildren<Text>();
        }

        public void Setup(bool success, bool boss, int currentScore, int target)
        {
            primary.onClick.RemoveAllListeners();
            secondary.onClick.RemoveAllListeners();
            if (success && boss)
            {
                title.text = "冒险完成！";
                title.color = GamePalette.Yellow;
                message.text = "最终 Boss 已被击败，所有路线汇聚成了胜利。";
                primaryLabel.text = "再来一局";
                primary.onClick.AddListener(() => RestartPressed?.Invoke());
                secondary.gameObject.SetActive(false);
            }
            else if (success)
            {
                title.text = "关卡通过";
                title.color = GamePalette.Green;
                message.text = "目标积分已达成，可以返回地图选择下一条路线。";
                primaryLabel.text = "返回地图";
                primary.onClick.AddListener(() => ContinuePressed?.Invoke());
                secondary.gameObject.SetActive(false);
            }
            else
            {
                title.text = "挑战失败";
                title.color = new Color(0.98f, 0.44f, 0.53f);
                message.text = "发射次数已经耗尽，再调整一下蓄力试试。";
                primaryLabel.text = "重新挑战";
                secondaryLabel.text = "重新开始冒险";
                primary.onClick.AddListener(() => RetryPressed?.Invoke());
                secondary.onClick.AddListener(() => RestartPressed?.Invoke());
                secondary.gameObject.SetActive(true);
            }
            score.text = $"关卡得分  {currentScore} / {target}";
        }
    }
}
