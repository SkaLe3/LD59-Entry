using UnityEngine;

namespace Math
{
    public static class MathUtils
    {
        public static float FInterpTo(float current, float target, float deltaTime, float interpSpeed)
        {
            if (interpSpeed <= 0f) return current;
            float dist = target - current;
            if (Mathf.Abs(dist) < 0.0001f) return target;
    
            return current + dist * Mathf.Clamp01(deltaTime * interpSpeed);
        }
    }
}