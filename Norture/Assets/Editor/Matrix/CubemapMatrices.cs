using System;
using UnityEngine;
namespace Norture.Matrix
{
    public static class CubemapMatrices
    {
        /// <summary>
        /// Transform in positive U direction.
        /// </summary>
        public static readonly Matrix3x3 PositiveU = Matrix3x3.Translation(1, 0);
        /// <summary>
        /// Transform in negative U direction.
        /// </summary>
        public static readonly Matrix3x3 NegativeU = Matrix3x3.Translation(-1, 0);

        // Notation: 

        /// <summary>
        /// Transform from NegativeZ to PositiveY.
        /// </summary>
        public static readonly Matrix3x3 PositiveY_NegativeZ = Matrix3x3.Translation(1, 0) * Matrix3x3.Rotation(Mathf.PI);
        /// <summary>
        /// Transform from PositiveX to PositiveY.
        /// </summary>
        public static readonly Matrix3x3 PositiveY_PositiveX = Matrix3x3.Translation(1, 1) * Matrix3x3.Rotation(-Mathf.PI * 0.5f);
        /// <summary>
        /// Transform from NegativeX to PositiveY.
        /// </summary>
        public static readonly Matrix3x3 PositiveY_NegativeX = Matrix3x3.Rotation(Mathf.PI * 0.5f);
        /// <summary>
        /// Transform from PositiveZ to PositiveY.
        /// </summary>
        public static readonly Matrix3x3 PositiveY_PositiveZ = Matrix3x3.Translation(0, 1);

        /// <summary>
        /// Transform from PositiveZ to NegativeY.
        /// </summary>
        public static readonly Matrix3x3 NegativeY_PositiveZ = Matrix3x3.Translation(0, -1);
        /// <summary>
        /// Transform from PositiveX to NegativeY.
        /// </summary>
        public static readonly Matrix3x3 NegativeY_PositiveX = Matrix3x3.Translation(2, 0) * Matrix3x3.Rotation(Mathf.PI * 0.5f);
        /// <summary>
        /// Transform from NegativeX to NegativeY.
        /// </summary>
        public static readonly Matrix3x3 NegativeY_NegativeX = Matrix3x3.Translation(-1, 1) * Matrix3x3.Rotation(-Mathf.PI * 0.5f);
        /// <summary>
        /// Transform from NegativeZ to NegativeY.
        /// </summary>
        public static readonly Matrix3x3 NegativeY_NegativeZ = Matrix3x3.Translation(1, 2) * Matrix3x3.Rotation(Mathf.PI);
    }
}
