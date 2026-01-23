using UnityEngine;

namespace TimelineSystem
{
    public enum EasingType
    {
        Linear,
        Shake,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce
    }

    public static class Easing
    {
        public static float Ease(float t, EasingType type)
        {
            t = Mathf.Clamp01(t);

            switch (type)
            {
                case EasingType.Linear: return t;
                case EasingType.Shake: return t;
                case EasingType.EaseInQuad: return t * t;
                case EasingType.EaseOutQuad: return t * (2 - t);
                case EasingType.EaseInOutQuad: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

                case EasingType.EaseInCubic: return t * t * t;
                case EasingType.EaseOutCubic: return (--t) * t * t + 1;
                case EasingType.EaseInOutCubic: return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

                case EasingType.EaseInQuart: return t * t * t * t;
                case EasingType.EaseOutQuart: return 1 - (--t) * t * t * t;
                case EasingType.EaseInOutQuart: return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

                case EasingType.EaseInSine: return 1 - Mathf.Cos(t * Mathf.PI / 2);
                case EasingType.EaseOutSine: return Mathf.Sin(t * Mathf.PI / 2);
                case EasingType.EaseInOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

                case EasingType.EaseInExpo: return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
                case EasingType.EaseOutExpo: return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
                case EasingType.EaseInOutExpo:
                    return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                        ? Mathf.Pow(2, 20 * t - 10) / 2
                        : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;

                case EasingType.EaseInCirc: return 1 - Mathf.Sqrt(1 - t * t);
                case EasingType.EaseOutCirc: return Mathf.Sqrt(1 - (--t) * t);
                case EasingType.EaseInOutCirc:
                    return t < 0.5f
                        ? (1 - Mathf.Sqrt(1 - 4 * t * t)) / 2
                        : (Mathf.Sqrt(1 - (-2 * t + 2) * (-2 * t + 2)) + 1) / 2;

                case EasingType.EaseInBack: return 2.70158f * t * t * t - 1.70158f * t * t;
                case EasingType.EaseOutBack: return 1 + 2.70158f * (t - 1) * (t - 1) * (t - 1) + 1.70158f * (t - 1) * (t - 1);
                case EasingType.EaseInOutBack:
                    float c1 = 1.70158f;
                    float c2 = c1 * 1.525f;
                    return t < 0.5f
                        ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                        : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;

                case EasingType.EaseInElastic:
                    float c3 = (2 * Mathf.PI) / 3;
                    return t == 0 ? 0 : t == 1 ? 1 : -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c3);

                case EasingType.EaseOutElastic:
                    float c4 = (2 * Mathf.PI) / 3;
                    return t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;

                case EasingType.EaseInOutElastic:
                    float c5 = (2 * Mathf.PI) / 4.5f;
                    return t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                        ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2
                        : (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c5)) / 2 + 1;

                case EasingType.EaseInBounce: return 1 - EaseOutBounce(1 - t);
                case EasingType.EaseOutBounce: return EaseOutBounce(t);
                case EasingType.EaseInOutBounce:
                    return t < 0.5f
                        ? (1 - EaseOutBounce(1 - 2 * t)) / 2
                        : (1 + EaseOutBounce(2 * t - 1)) / 2;

                default: return t;
            }
        }

        private static float EaseOutBounce(float t)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (t < 1 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5 / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
    }
}
