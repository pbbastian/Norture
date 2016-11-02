using System;
using UnityEngine;

namespace Norture
{
    public struct CubemapCoordinate
    {
        public CubemapFace Face;
        public float U;
        public float V;

        static Vector3[] FaceOrigins = {
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, -1f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 0f, -1f)
        };

        static float[] FaceToWorld = {
            0f, 0f, 0f, -1f, -1f, 0f,
            0f, 0f, 0f, -1f, 1f, 0f,
            1f, 0f, 0f, 0f, 0f, 1f,
            1f, 0f, 0f, 0f, 0f, -1f,
            1f, 0f, 0f, -1f, 0f, 0f,
            -1f, 0f, 0f, -1f, 0f, 0f
        };

        public CubemapCoordinate(CubemapFace face, float u, float v)
        {
            Face = face;
            U = u;
            V = v;
        }

        public CubemapCoordinate(CubemapCoordinate other, float u, float v)
        {
            Face = other.Face;
            U = u;
            V = v;
        }

        public CubemapCoordinate(Vector3 direction)
        {
            CalculateCoordinatesFromDirection(direction, out Face, out U, out V);
        }

        public CubemapCoordinate[] GetNeighbors(int size)
        {
            var delta = 1f / (float)size;
            return new CubemapCoordinate[] {
                // Direct neighbors
                new CubemapCoordinate(Face, U + delta, V),
                new CubemapCoordinate(Face, U - delta, V),
                new CubemapCoordinate(Face, U, V + delta),
                new CubemapCoordinate(Face, U, V - delta),
                // Corner neighbors
                // new CubemapCoordinate(Face, U + delta, V + delta),
                // new CubemapCoordinate(Face, U + delta, V - delta),
                // new CubemapCoordinate(Face, U - delta, V + delta),
                // new CubemapCoordinate(Face, U - delta, V - delta)
            };
        }

        public CubemapCoordinate PixelCenter(int size)
        {
            return new CubemapCoordinate(Face,
                (Mathf.Floor(U * size) + 0.5f) / size,
                (Mathf.Floor(V * size) + 0.5f) / size);
        }

        public bool Equals(CubemapCoordinate other, float eps)
        {
            return Face == other.Face
                && Mathf.Abs(U - other.U) < eps
                && Mathf.Abs(V - other.V) < eps;
        }

        static void CalculateCoordinatesFromDirection(Vector3 direction, out CubemapFace face, out float u, out float v)
        {
            var absX = Mathf.Abs(direction.x);
            var absY = Mathf.Abs(direction.y);
            var absZ = Mathf.Abs(direction.z);

            if (absX > absY && absX > absZ)
            {
                if (direction.x > 0)
                {
                    face = CubemapFace.PositiveX;
                    CalculateUV(-direction.z, -direction.y, absX, out u, out v);
                }
                else
                {
                    face = CubemapFace.NegativeX;
                    CalculateUV(direction.z, -direction.y, absX, out u, out v);
                }
            }
            else if (absY > absX && absY > absZ)
            {
                if (direction.y > 0)
                {
                    face = CubemapFace.PositiveY;
                    CalculateUV(direction.x, direction.z, absY, out u, out v);
                }
                else
                {
                    face = CubemapFace.NegativeY;
                    CalculateUV(direction.x, -direction.z, absY, out u, out v);
                }
            }
            else
            {
                if (direction.z > 0)
                {
                    face = CubemapFace.PositiveZ;
                    CalculateUV(direction.x, -direction.y, absZ, out u, out v);
                }
                else
                {
                    face = CubemapFace.NegativeZ;
                    CalculateUV(-direction.x, -direction.y, absZ, out u, out v);
                }
            }
        }

        static void CalculateUV(float sc, float tc, float absMa, out float u, out float v)
        {
            u = ((sc / absMa) + 1f) * 0.5f;
            v = ((tc / absMa) + 1f) * 0.5f;
        }

        public Vector3 ToDirection()
        {
            var faceIndex = (int)Face;

            var offset = 6 * faceIndex;
            var u = U * 2f - 1f;
            var v = V * 2f - 1f;
            var worldUV = new Vector3(
                FaceToWorld[offset+0]*u + FaceToWorld[offset+1]*v,
                FaceToWorld[offset+2]*u + FaceToWorld[offset+3]*v,
                FaceToWorld[offset+4]*u + FaceToWorld[offset+5]*v
            );

            var faceOrigin = FaceOrigins[faceIndex];
            var worldCoordinate = faceOrigin + worldUV;
            
            return worldCoordinate.normalized;
        }

        public override string ToString()
        {
            return "(" + Enum.GetName(typeof(CubemapFace), Face) + ", " + U + ", " + V + ")";
        }
    }
}