using System.IO;
using UnityEngine;

namespace Norture.Extensions
{
    public static class CubemapExtensions
    {
        public static void SetPixel(this Cubemap cubemap, Vector3 direction, Color color)
        {
            var coordinate = new CubemapCoordinate(direction);
            cubemap.SetPixel(coordinate, color);
        }

        public static void SetPixel(this Cubemap cubemap, CubemapCoordinate coordinate, Color color)
        {
            cubemap.SetPixel(coordinate.Face, (int) (coordinate.U * cubemap.width), (int) (coordinate.V * cubemap.width),
                color);
        }

        public static Color GetPixel(this Cubemap cubemap, CubemapCoordinate coordinate)
        {
            return cubemap.GetPixel(coordinate.Face, (int) (coordinate.U * cubemap.width),
                (int) (coordinate.V * cubemap.width));
        }

        public static void EncodeToPNGs(this Cubemap cubemap, string path)
        {
            if (!path.StartsWith("/"))
                path = "/" + path;
            if (!path.EndsWith("/"))
                path = path + "/";

            var texture = new Texture2D(cubemap.width, cubemap.height, TextureFormat.RGB24, false);
            foreach (var face in CubemapUtility.Faces)
            {
                texture.SetPixels(cubemap.GetPixels(face));
                File.WriteAllBytes(Application.dataPath + path + cubemap.name + "_" + face.ToString() + ".png",
                    texture.EncodeToPNG());
            }
        }
    }
}