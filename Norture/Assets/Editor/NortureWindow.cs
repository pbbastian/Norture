using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace Norture
{
    public class NortureWindow : EditorWindow
    {
        PreviewRenderUtility previewRenderUtility;

        Mesh sphereMesh;

        Material material;

        Cubemap cubemap;

        [SerializeField]
        DragController dragController;

        [SerializeField]
        Vector2 cameraDrag;

        Vector2 lastPosition;

        [MenuItem("Window/Norture")]
        public static void ShowWindow()
        {
            GetWindow<NortureWindow>("Norture");
        }

        void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(position.width, position.width);
            // var rect = new Rect(0, 0, position.width, position.height);

            dragController.Sample(rect);
            HandleClick(rect, previewRenderUtility.m_Camera);


            previewRenderUtility.BeginPreview(rect, GUIStyle.none);
            previewRenderUtility.DrawMesh(sphereMesh, Matrix4x4.identity, material, 0);

            previewRenderUtility.m_Camera.transform.position = Vector2.zero;
            previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(dragController.Position.y, dragController.Position.x, 0);
            previewRenderUtility.m_Camera.transform.position = previewRenderUtility.m_Camera.transform.forward * -6f;
            previewRenderUtility.m_Camera.Render();



            Texture resultRender = previewRenderUtility.EndPreview();

            GUI.DrawTexture(rect, resultRender, ScaleMode.StretchToFill, false);
        }

        void HandleClick(Rect rect, Camera camera)
        {
            var current = Event.current;
            var controlId = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            var eventType = current.GetTypeForControl(controlId);

            if (eventType == EventType.MouseDown && current.modifiers == EventModifiers.None)
            {
                var style = new GUIStyle();
                current.Use();
                lastPosition = current.mousePosition;
                lastPosition.x = 2f * lastPosition.x / rect.width;
                lastPosition.y = 2f * (rect.height - lastPosition.y) / rect.height;
            }


            var cameraPosition = camera.gameObject.transform.position;
            var wsPosition = camera.ScreenToWorldPoint(new Vector3(lastPosition.x, lastPosition.y, camera.nearClipPlane));
            var wsDirection = (wsPosition - cameraPosition).normalized;
            Vector3 intersection;
            LineSphereIntersection(cameraPosition, wsDirection, out intersection);

            EditorGUILayout.LabelField("Camera Pixel Size", string.Format("({0}, {1})", camera.pixelWidth, camera.pixelHeight));
            EditorGUILayout.LabelField("Camera World Space", cameraPosition.ToString());
            EditorGUILayout.LabelField("Screen Space", lastPosition.ToString());
            EditorGUILayout.LabelField("World Space", wsPosition.ToString());
            EditorGUILayout.LabelField("Direction", wsDirection.ToString());
            EditorGUILayout.LabelField("Intersection", intersection.ToString());


        }

        bool LineSphereIntersection(Vector3 origin, Vector3 direction, out Vector3 intersection)
        {
            var directionDotOrigin = Vector3.Dot(direction, origin);
            var x = directionDotOrigin * directionDotOrigin - origin.sqrMagnitude + 0.25f;
            if (x < 0f)
            {
                intersection = Vector3.zero;
                return false;
            }
            else
            {
                var distance = -directionDotOrigin - Mathf.Sqrt(x);
                intersection = origin + distance * direction;
                return true;
            }
        }

        void OnEnable()
        {
            dragController = dragController ?? new DragController(EventModifiers.Alt);
            previewRenderUtility = new PreviewRenderUtility();
            sphereMesh = GetSphereMesh();
            material = LoadMaterial();
            cubemap = CreateCubemap();
            material.SetTexture("_Cube", cubemap);

            var faces = new[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            var colors = new[] { new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1), new Color(1, 0, 1) };
            int i = 0;

            foreach (var face in faces)
            {
                for (int x = 0; x < 128; ++x)
                {
                    for (int y = 0; y < 128; ++y)
                    {
                        cubemap.SetPixel(face, x, y, colors[i]);
                    }
                }
                ++i;
            }

            cubemap.Apply();
        }

        void OnDestroy()
        {
            previewRenderUtility.Cleanup();
        }

        Mesh GetSphereMesh()
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var mesh = Instantiate(meshFilter.sharedMesh);
            DestroyImmediate(gameObject);
            return mesh;
        }

        Material LoadMaterial()
        {
            return Resources.Load<Material>("NortureMaterial");
        }

        Cubemap CreateCubemap()
        {
            return new Cubemap(128, TextureFormat.ARGB32, false);
        }
    }
}