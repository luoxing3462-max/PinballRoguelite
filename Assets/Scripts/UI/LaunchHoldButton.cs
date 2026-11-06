using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PinballRoguelite
{
    public sealed class LaunchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action Pressed;
        public Action Released;
        private bool held;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            held = true;
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (held && !Input.GetMouseButton(0)) Release();
        }

        private void OnDisable()
        {
            held = false;
        }

        private void Release()
        {
            if (!held) return;
            held = false;
            Released?.Invoke();
        }
    }
}
