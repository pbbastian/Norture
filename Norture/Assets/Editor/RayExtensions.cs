using UnityEngine;

namespace Norture
{
    public static class RayExtensions
    {
        public static bool IntersectsWithSphere(this Ray ray, out Vector3 intersection)
        {
            var directionDotOrigin = Vector3.Dot(ray.direction, ray.origin);
            var x = directionDotOrigin * directionDotOrigin - ray.origin.sqrMagnitude + 0.25f;
            if (x < 0f)
            {
                intersection = Vector3.zero;
                return false;
            }
            else
            {
                var distance = -directionDotOrigin - Mathf.Sqrt(x);
                intersection = ray.origin + distance * ray.direction;
                return true;
            }
        }
    }
}