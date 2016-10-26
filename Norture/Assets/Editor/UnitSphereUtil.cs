using UnityEngine;

namespace Norture
{
    public static class UnitSphereUtil
    {
        public static float ArcLength(Vector3 position1, Vector3 position2)
        {
            var chord = (position1 - position2).magnitude;
            var arcLength = 2f * Mathf.Asin(chord * 0.5f);
            return arcLength;
        }

        // public static float DistanceToHyperplane(Vector3 )
    }
}