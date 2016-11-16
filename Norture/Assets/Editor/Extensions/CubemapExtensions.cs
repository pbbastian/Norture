using UnityEngine;

namespace Norture.Extensions
{
    public static class CubemapExtensions
    {
        public static void SetPixel(this Cubemap cubemap, Vector3 direction, Color color)
        {
            var coordinate = new CubemapCoordinate(direction);
            cubemap.SetPixel(coordinate.Face, (int)(coordinate.U * cubemap.width), (int)(coordinate.V * cubemap.width), color);
        }
    }
}