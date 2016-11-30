using System;
using UnityEngine;
using NUnit.Framework;
namespace Norture.Matrix
{
    public struct Matrix3x3
    {
        // Row vectors
        public Vector3 R1 { get; set; }
        public Vector3 R2 { get; set; }
        public Vector3 R3 { get; set; }

        public Matrix3x3(Vector3 a, Vector3 b, Vector3 c)
        {
            R1 = a;
            R2 = b;
            R3 = c;
        }

        public void MultiplyBy(Matrix3x3 other)
        {
            R1 = new Vector3(
                // i = 1, j = 1
                R1.x * other.R1.x + R1.y * other.R2.x + R1.z * other.R3.x,
                // i = 1, j = 2
                R1.x * other.R1.y + R1.y * other.R2.y + R1.z * other.R3.y,
                // i = 1, j = 3
                R1.x * other.R1.z + R1.y * other.R2.z + R1.z * other.R3.z
            );
            R2 = new Vector3(
                // i = 2, j = 1
                R2.x * other.R1.x + R2.y * other.R2.x + R2.z * other.R3.x,
                // i = 2, j = 2
                R2.x * other.R1.y + R2.y * other.R2.y + R2.z * other.R3.y,
                // i = 2, j = 3
                R2.x * other.R1.z + R2.y * other.R2.z + R2.z * other.R3.z
            );
            R3 = new Vector3(
                // i = 3, j = 1
                R3.x * other.R1.x + R3.y * other.R2.x + R3.z * other.R3.x,
                // i = 3, j = 2
                R3.x * other.R1.y + R3.y * other.R2.y + R3.z * other.R3.y,
                // i = 3, j = 3
                R3.x * other.R1.z + R3.y * other.R2.z + R3.z * other.R3.z
            );
        }

        public static Matrix3x3 operator *(Matrix3x3 m1, Matrix3x3 m2)
        {
            m1.MultiplyBy(m2);
            return m1;
        }

        public void Invert()
        {
            var x0 = new Vector3(R1.x, R2.x, R3.x);
            var x1 = new Vector3(R1.y, R2.y, R3.y);
            var x2 = new Vector3(R1.z, R2.z, R3.z);

            //var A = R2.y * R3.z - R2.z * R3.y;
            //var B = -(R2.x * R3.z - R2.z * R3.x);
            //var C = R2.x * R3.y - R2.y * R3.x;

            var x1x2 = Vector3.Cross(x1, x2);
            var det = Vector3.Dot(x0, x1x2);
            Assert.False(Math.Abs(det) < 1e-6f);
            var detInv = 1f / det;

            R1 = x1x2 * detInv;
            R2 = Vector3.Cross(x2, x0) * detInv;
            R3 = Vector3.Cross(x0, x1) * detInv;
        }

        public Matrix3x3 Inverted
        {
            get
            {
                var m = this;
                m.Invert();
                return m;
            }
        }

        public Vector2 TransformPointFast(Vector2 v)
        {
            return new Vector2(
                R1.x * v.x + R1.y * v.y + R1.z,
                R2.x * v.x + R2.y * v.y + R2.z
            );
        }

        public Vector3 TransformVector(Vector3 v)
        {
            return new Vector3(
                R1.x * v.x + R1.y * v.y + R1.z * v.z,
                R2.x * v.x + R2.y * v.y + R2.z * v.z,
                R3.x * v.x + R3.y * v.y + R3.z * v.z
            );
        }

        public static Matrix3x3 Identity
        {
            get
            {
                return new Matrix3x3(
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1)
                );
            }
        }

        public static Matrix3x3 Translation(float x, float y)
        {
            return new Matrix3x3(
                new Vector3(1, 0, x),
                new Vector3(0, 1, y),
                new Vector3(0, 0, 1)
            );
        }

        public static Matrix3x3 Translation(Vector2 v)
        {
            return Translation(v.x, v.y);
        }

        public static Matrix3x3 Rotation(float theta)
        {
            return new Matrix3x3(
                new Vector3(Mathf.Cos(theta), -Mathf.Sin(theta), 0),
                new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0),
                new Vector3(0, 0, 1)
            );
        }

        public override string ToString()
        {
            return R1 + "\n" + R2 + "\n" + R3;
        }
    }
}
