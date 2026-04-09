// PROTOTYPE - NOT FOR PRODUCTION
// Camera shake effect for synergy attacks
// Date: 2026-04-07

using UnityEngine;

namespace TowerDefenseRush.Prototype
{
    /// <summary>
    /// 相机震动效果 - 协同攻击时触发
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance;

        [Header("Settings")]
        public float defaultDuration = 0.3f;
        public float defaultMagnitude = 0.2f;

        private Vector3 originalPosition;
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0f;
        private bool isShaking = false;

        void Awake()
        {
            Instance = this;
            originalPosition = transform.localPosition;
        }

        void Update()
        {
            if (isShaking && shakeDuration > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.unscaledDeltaTime;
            }
            else if (isShaking)
            {
                isShaking = false;
                transform.localPosition = originalPosition;
            }
        }

        /// <summary>
        /// 触发相机震动
        /// </summary>
        public void Shake(float duration = -1, float magnitude = -1)
        {
            shakeDuration = duration > 0 ? duration : defaultDuration;
            shakeMagnitude = magnitude > 0 ? magnitude : defaultMagnitude;
            originalPosition = transform.localPosition;
            isShaking = true;
        }
    }
}
