using System;
using Norture.Extensions;
using UnityEngine;
using UnityEditor;

namespace Norture
{
    public class RenderPreviewGUI : IDisposable
    {
        private Camera camera;
        private RenderTexture renderTexture;

        public RenderPreviewGUI(int width, int height, int depth, RenderTextureFormat format)
        {
            renderTexture = new RenderTexture(width, height, depth, format);

            var gameObject = EditorUtility.CreateGameObjectWithHideFlags("RenderPreviewCamera", HideFlags.HideAndDontSave, new Type[]
            {
                typeof(Camera)
            });
            camera = gameObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.farClipPlane = 10f;
            camera.nearClipPlane = 0.01f;
            camera.backgroundColor = new Color(0.192156866f, 0.192156866f, 0.192156866f, 1f);
            camera.renderingPath = RenderingPath.Forward;
            camera.useOcclusionCulling = false;
            camera.targetTexture = renderTexture;
            camera.SetOnlyDrawMesh();
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }
}