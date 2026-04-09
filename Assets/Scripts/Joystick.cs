// PROTOTYPE - NOT FOR PRODUCTION
// Simple virtual joystick for mobile
// Date: 2026-04-07

using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefenseRush.Prototype
{
    /// <summary>
    /// 虚拟摇杆 - 用于移动设备输入
    /// </summary>
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("UI References")]
        public RectTransform background;
        public RectTransform handle;

        [Header("Settings")]
        public float handleRange = 1f;

        private Vector2 input = Vector2.zero;
        private Vector2 origin;
        private float radius;
        private bool isDragging = false;

        public float Horizontal => input.x;
        public float Vertical => input.y;

        void Start()
        {
            radius = background.sizeDelta.x / 2f;
            origin = background.position;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            background.position = eventData.position;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 direction = eventData.position - (Vector2)background.position;
            input = direction.magnitude > radius ? direction.normalized : direction / radius;
            input = Vector2.ClampMagnitude(input, 1f);

            handle.anchoredPosition = input * radius * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            background.position = origin;
        }
    }
}
