using System;
using UnityEngine;

namespace PinballRoguelite
{
    [Serializable]
    public sealed class LaunchPhysicsSettings
    {
        [SerializeField] private float minimumChargeTime = 0.08f;
        [SerializeField] private float maximumChargeTime = 1.35f;
        [SerializeField] private float minimumImpulse = 15f;
        [SerializeField] private float maximumImpulse = 25f;
        [SerializeField] private AnimationCurve chargeToImpulse = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0.30f),
            new Keyframe(0.32f, 0.12f, 0.55f, 0.90f),
            new Keyframe(0.70f, 0.60f, 1.10f, 1.45f),
            new Keyframe(1f, 1f, 1.05f, 0f));

        public float MaximumChargeTime => maximumChargeTime;

        public float GetChargePercent(float heldSeconds)
        {
            return GetChargeFraction(heldSeconds) * 100f;
        }

        public float EvaluateImpulse(float chargePercent)
        {
            var normalizedCharge = Mathf.Clamp01(chargePercent / 100f);
            var curvedCharge = Mathf.Clamp01(chargeToImpulse.Evaluate(normalizedCharge));
            return Mathf.Lerp(minimumImpulse, maximumImpulse, curvedCharge);
        }

        public float GetAutomaticChargePercent(float normalizedCharge)
        {
            var heldSeconds = Mathf.Lerp(minimumChargeTime, maximumChargeTime, Mathf.Clamp01(normalizedCharge));
            return GetChargePercent(heldSeconds);
        }

        private float GetChargeFraction(float heldSeconds)
        {
            var duration = Mathf.Max(0.01f, maximumChargeTime - minimumChargeTime);
            return Mathf.Clamp01((Mathf.Max(0f, heldSeconds) - minimumChargeTime) / duration);
        }
    }
}
