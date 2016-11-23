using System.IO;
using System.Linq;
using UnityEngine;

namespace Norture
{
    public class CubemapUtility
    {
        public static readonly CubemapFace[] Faces = new CubemapFace[]
        {
            CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };

        public static Cubemap LoadFromFiles(string path, string name)
        {
            if (!path.StartsWith("/"))
                path = "/" + path;
            if (!path.EndsWith("/"))
                path = path + "/";

            var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            texture.LoadImage(File.ReadAllBytes(Application.dataPath + path + name + Faces.First().ToString() + ".png"));

            var cubemap = new Cubemap(texture.width, texture.format, false);
            cubemap.SetPixels(texture.GetPixels(), Faces.First());

            foreach (var face in Faces.Skip(1))
            {
                texture.LoadImage(File.ReadAllBytes(Application.dataPath + path + name + face.ToString() + ".png"));
                cubemap.SetPixels(texture.GetPixels(), face);
            }

            return cubemap;
        }
    }
}