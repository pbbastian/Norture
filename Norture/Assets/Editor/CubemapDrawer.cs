using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Norture
{
    public class CubemapDrawer : IDisposable
    {
        private PreviewRenderUtility _previewRenderUtility;
        private Material _material;

        public Cubemap Cubemap;
        private Mesh _mesh;

        private CubemapFace[] _faces =
        {
            CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };

        private Matrix4x4[] _faceTransformations;

        public CubemapDrawer(Cubemap cubemap)
        {
            _previewRenderUtility = new PreviewRenderUtility();
            _material = LoadMaterial();
            _mesh = GetQuadMesh();
            Cubemap = cubemap;

            var translation = Matrix4x4.TRS(Vector3.forward, Quaternion.identity, Vector3.one);
            _faceTransformations = new Matrix4x4[]
            {
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 90f, 0f), Vector3.one) * translation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, -90f, 0f), Vector3.one) * translation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90f, 0f, 0f), Vector3.one) * translation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one) * translation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one) * translation,
                Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 180f, 0f), Vector3.one) * translation
            };

            Assert.AreEqual(_faces.Length, _faceTransformations.Length);
        }

        public void Render(Rect rect)
        {
            _previewRenderUtility.BeginPreview(rect, GUIStyle.none);
            for (var i = 0; i < _faceTransformations.Length; i++)
            {
                _previewRenderUtility.DrawMesh(_mesh, _faceTransformations[i], _material, 0);
            }

        }

        public void Dispose()
        {
            _previewRenderUtility.Cleanup();
        }

        Material LoadMaterial()
        {
            return Resources.Load<Material>("NortureMaterial");
        }

        Mesh GetQuadMesh()
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var mesh = UnityEngine.Object.Instantiate(meshFilter.sharedMesh);
            UnityEngine.Object.DestroyImmediate(gameObject);
            return mesh;
        }
    }
}