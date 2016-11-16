using UnityEngine;

namespace Norture
{
    public static class UnitSphereUtil
    {
        public static float GreatCircleDistance(Vector3 position1, Vector3 position2)
        {
            return Mathf.Acos(Vector3.Dot(position1, position2));
        }


    }
}