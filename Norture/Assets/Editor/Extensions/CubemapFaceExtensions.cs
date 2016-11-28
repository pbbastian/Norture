using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
namespace Norture.Extensions
{
    public static class CubemapFaceExtensions
    {
        static CubemapFace[] noNeighbors = { };
        static CubemapFace[][] neighbors =
        {
            // PositiveU NegativeU PositiveV NegativeV
            new CubemapFace[] { CubemapFace.PositiveZ, CubemapFace.NegativeZ, CubemapFace.PositiveY, CubemapFace.NegativeY },
            new CubemapFace[] { CubemapFace.NegativeZ, CubemapFace.PositiveZ, CubemapFace.PositiveY, CubemapFace.NegativeY },
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.NegativeZ, CubemapFace.PositiveZ },
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.PositiveZ, CubemapFace.NegativeZ },
            new CubemapFace[] { CubemapFace.NegativeX, CubemapFace.PositiveX, CubemapFace.PositiveY, CubemapFace.NegativeY },
            new CubemapFace[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY }
        };

        public static CubemapFace[] GetNeighbors(this CubemapFace face)
        {
            if (face == CubemapFace.Unknown)
                return noNeighbors;
            return neighbors[(int)face];
        }
    }
}
