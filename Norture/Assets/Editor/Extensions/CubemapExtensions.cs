using System.IO;
using UnityEngine;
using Norture.Matrix;
using UnityEngine.Assertions;

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
            cubemap.SetPixel(coordinate.Face, (int)(coordinate.U * cubemap.width), (int)(coordinate.V * cubemap.width),
                color);
        }

        public static Color GetPixel(this Cubemap cubemap, CubemapCoordinate coordinate)
        {
            return cubemap.GetPixel(coordinate.Face, (int)(coordinate.U * cubemap.width),
                (int)(coordinate.V * cubemap.width));
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

        // PositiveU NegativeU PositiveV NegativeV
        static readonly CubemapFace[][] neighbors =
        {
            // PositiveX
            new CubemapFace[] { CubemapFace.PositiveZ, CubemapFace.NegativeZ, CubemapFace.PositiveY, CubemapFace.NegativeY },
            // NegativeX
            new CubemapFace[] { CubemapFace.NegativeZ, CubemapFace.PositiveZ, CubemapFace.PositiveY, CubemapFace.NegativeY },
            // PositiveY
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.NegativeZ, CubemapFace.PositiveZ },
            // NegativeY
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.PositiveZ, CubemapFace.NegativeZ },
            // PositiveZ
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.PositiveY, CubemapFace.NegativeY },
            // NegativeZ
            new CubemapFace[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY }
        };

        // PositiveU NegativeU PositiveV NegativeV
        static readonly Matrix3x3[][] neighborMatrices =
        {
            // PositiveX
            new Matrix3x3[] { CubemapMatrices.PositiveU, CubemapMatrices.NegativeU, CubemapMatrices.PositiveY_PositiveX, CubemapMatrices.NegativeY_PositiveX },
            // NegativeX
            new Matrix3x3[] { CubemapMatrices.PositiveU, CubemapMatrices.NegativeU, CubemapMatrices.PositiveY_NegativeX, CubemapMatrices.NegativeY_NegativeX },
            // PositiveY
            new Matrix3x3[] { CubemapMatrices.PositiveY_NegativeX.Inverted, CubemapMatrices.PositiveY_PositiveX.Inverted, CubemapMatrices.PositiveY_NegativeZ.Inverted, CubemapMatrices.PositiveY_PositiveZ.Inverted },
            // NegativeY
            new Matrix3x3[] { CubemapMatrices.NegativeY_NegativeX.Inverted, CubemapMatrices.NegativeY_PositiveX.Inverted, CubemapMatrices.NegativeY_PositiveZ.Inverted, CubemapMatrices.NegativeY_NegativeZ.Inverted },
            // PositiveZ
            new Matrix3x3[] { CubemapMatrices.PositiveU, CubemapMatrices.NegativeU, CubemapMatrices.PositiveY_PositiveZ, CubemapMatrices.NegativeY_PositiveZ },
            // NegativeZ
            new Matrix3x3[] { CubemapMatrices.PositiveU, CubemapMatrices.NegativeU, CubemapMatrices.PositiveY_NegativeZ, CubemapMatrices.NegativeY_NegativeZ }
        };

        /// <summary>
        /// Returns the neighboring cubemap faces in the order "PositiveU NegativeU PositiveV NegativeV".
        /// </summary>
        /// <returns>The neighbors.</returns>
        /// <param name="face">The face to find neighbors for.</param>
        public static CubemapFace[] GetNeighbors(this CubemapFace face)
        {
            Assert.AreNotEqual(face, CubemapFace.Unknown);
            return neighbors[(int)face];
        }

        public static Matrix3x3[] GetNeighborMatrices(this CubemapFace face)
        {
            Assert.AreNotEqual(face, CubemapFace.Unknown);
            return neighborMatrices[(int)face];
        }
    }
}