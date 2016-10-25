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
        Texture previewTexture;
        Rect previewRect;
        bool previewDirty = false;
        const int cubemapResolution = 256;

        [MenuItem("Window/Norture")]
        public static void ShowWindow()
        {
            GetWindow<NortureWindow>("Norture");
        }

        void Update()
        {
            if (previewDirty)
            {
                previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);
                previewRenderUtility.DrawMesh(sphereMesh, Matrix4x4.identity, material, 0);

                previewRenderUtility.m_Camera.transform.position = Vector2.zero;
                previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(dragController.Position.y, dragController.Position.x, 0);
                previewRenderUtility.m_Camera.transform.position = previewRenderUtility.m_Camera.transform.forward * -6f;
                previewRenderUtility.m_Camera.Render();

                previewTexture = previewRenderUtility.EndPreview();
                previewDirty = false;
            }
        }

        void OnGUI()
        {
            previewRect = GUILayoutUtility.GetRect(position.width, position.width);
            // var rect = new Rect(0, 0, position.width, position.height);

            dragController.Sample(previewRect);
            HandleClick(previewRect, previewRenderUtility.m_Camera);
            previewDirty = true;

            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
        }

        void HandleClick(Rect rect, Camera camera)
        {
            var current = Event.current;
            var controlId = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            var eventType = current.GetTypeForControl(controlId);

            var correctEvent = (eventType == EventType.MouseDown || eventType == EventType.MouseDrag) && current.modifiers == EventModifiers.None;

            if (correctEvent)
            {
                current.Use();
                lastPosition = current.mousePosition;
                lastPosition.x = camera.pixelWidth * lastPosition.x / rect.width;
                lastPosition.y = camera.pixelHeight * (rect.height - lastPosition.y) / rect.height;
            }

            var cameraPosition = camera.gameObject.transform.position;
            var wsPosition = camera.ScreenToWorldPoint(new Vector3(lastPosition.x, lastPosition.y, camera.nearClipPlane));
            var wsDirection = (wsPosition - cameraPosition).normalized;
            Vector3 intersection;
            LineSphereIntersection(cameraPosition, wsDirection, out intersection);
            
            if (correctEvent)
            {
                SetCubemapPixelByDirection(cubemap, intersection.normalized, Color.black);
                cubemap.Apply();
            }

            EditorGUILayout.LabelField("Camera Pixel Size", string.Format("({0}, {1})", camera.pixelWidth, camera.pixelHeight));
            EditorGUILayout.LabelField("Camera World Space", cameraPosition.ToString());
            EditorGUILayout.LabelField("Screen Space", lastPosition.ToString());
            EditorGUILayout.LabelField("World Space", wsPosition.ToString());
            EditorGUILayout.LabelField("Direction", wsDirection.ToString());
            EditorGUILayout.LabelField("Intersection", intersection.ToString());
        }

        void SetCubemapPixelByDirection(Cubemap cubemap, Vector3 direction, Color color)
        {
            // Direction -> Cubemap -> Direction -> Cubemap for demonstrative purposes.
            var coordinate = new CubemapCoordinate(new CubemapCoordinate(direction).ToDirection());
            cubemap.SetPixel(coordinate.Face, (int)(coordinate.U*cubemap.width), (int)(coordinate.V*cubemap.width), color);
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
            InitializeCubemap(cubemap);
            material.SetTexture("_Cube", cubemap);
            previewTexture = new Texture2D(0, 0);
            previewRect = new Rect(Vector2.zero, new Vector2(cubemapResolution, cubemapResolution));
            previewDirty = true;
        }

        void InitializeCubemap(Cubemap cubemap)
        {
            var faces = new[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            var colors = new[] { new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1), new Color(1, 0, 1) };
            int i = 0;

            foreach (var face in faces)
            {
                for (int x = 0; x < cubemapResolution; ++x)
                {
                    for (int y = 0; y < cubemapResolution; ++y)
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
            return new Cubemap(cubemapResolution, TextureFormat.ARGB32, false);
        }
    }
}