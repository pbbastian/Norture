using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;

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
        Vector2 downPosition;
        bool downValid = false;
        Vector2 upPosition;
        Texture previewTexture;
        Rect previewRect;
        bool previewDirty = false;
        const int cubemapResolution = 512;
        DateTime lastTime;
        TimeSpan currentTimeSpan;
        int callCount = 0;
        int correctCallCount = 0;

        [MenuItem("Window/Norture")]
        public static void ShowWindow()
        {
            GetWindow<NortureWindow>("Norture");
        }

        void Update()
        {
            if (previewDirty)
            {
                cubemap.Apply();
                previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);
                previewRenderUtility.DrawMesh(sphereMesh, Matrix4x4.identity, material, 0);

                previewRenderUtility.m_Camera.transform.position = Vector2.zero;
                previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(dragController.Position.y, dragController.Position.x, 0);
                previewRenderUtility.m_Camera.transform.position = previewRenderUtility.m_Camera.transform.forward * -6f;
                previewRenderUtility.m_Camera.Render();

                previewTexture = previewRenderUtility.EndPreview();
                previewDirty = false;
            }
            
            Repaint();
        }

        void OnGUI()
        {
            previewRect = GUILayoutUtility.GetRect(position.width, position.width);
            // var rect = new Rect(0, 0, position.width, position.height);

            dragController.Sample(previewRect);
            HandleClick(previewRect, previewRenderUtility.m_Camera);
            previewDirty = true;

            var timeSpan = DateTime.Now - lastTime;
            currentTimeSpan += timeSpan;
            callCount++;
            lastTime = DateTime.Now;

            if (currentTimeSpan > TimeSpan.FromSeconds(1))
            {
                Debug.LogFormat("Calls per second: {0}", callCount / currentTimeSpan.TotalSeconds);
                Debug.LogFormat("Correct calls per second: {0}", correctCallCount / currentTimeSpan.TotalSeconds);
                currentTimeSpan = TimeSpan.Zero;
                callCount = 0;
                correctCallCount = 0;
            }

            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
        }

        void HandleClick(Rect rect, Camera camera)
        {
            var current = Event.current;
            var eventType = current.GetTypeForControl(GUIUtility.GetControlID(FocusType.Passive));

            var correctEvent = (eventType == EventType.MouseDown || eventType == EventType.MouseDrag) && current.modifiers == EventModifiers.None;

            if (eventType == EventType.MouseMove)
            {
                current.Use();
                Debug.Log(current.mousePosition);
            }

            else if (eventType == EventType.MouseDown && current.modifiers == EventModifiers.None)
            {
                current.Use();
                downPosition = current.mousePosition;
                downPosition.x = camera.pixelWidth * downPosition.x / rect.width;
                downPosition.y = camera.pixelHeight * (rect.height - downPosition.y) / rect.height;

                Vector3 downPositionWorldSpace;
                downValid = RaycastScreenWithSphere(camera, downPosition, out downPositionWorldSpace);
                if (downValid)
                {
                    SetCubemapPixelByDirection(cubemap, downPositionWorldSpace.normalized, Color.black);
                    cubemap.Apply();
                    Debug.LogFormat("Down: {0}", new CubemapCoordinate(downPositionWorldSpace).ToString());
                }
                else
                {
                    Debug.Log("Invalid down position");
                }
            }
            else if (downValid && eventType == EventType.MouseUp && current.modifiers == EventModifiers.None)
            {
                // current.Use();
                // upPosition = current.mousePosition;
                // upPosition.x = camera.pixelWidth * upPosition.x / rect.width;
                // upPosition.y = camera.pixelHeight * (rect.height - upPosition.y) / rect.height;

                // // Will need downPositionWorldSpace later
                // Vector3 downPositionWorldSpace, upPositionWorldSpace;
                // RaycastScreenWithSphere(camera, downPosition, out downPositionWorldSpace);
                // if (RaycastScreenWithSphere(camera, upPosition, out upPositionWorldSpace))
                // {
                //     SetCubemapPixelByDirection(cubemap, upPositionWorldSpace.normalized, Color.black);
                //     var coordinates = GetCubemapArcCoordinates(cubemap, downPositionWorldSpace, upPositionWorldSpace);
                //     foreach (var coordinate in coordinates)
                //     {
                //         cubemap.SetPixel(coordinate.Face, (int)(coordinate.U*(float)cubemap.width), (int)(coordinate.V*(float)cubemap.width), Color.white);
                //     }
                //     cubemap.Apply();
                //     Debug.LogFormat("Up: {0}", new CubemapCoordinate(upPositionWorldSpace).ToString());
                // }
                // else
                // {
                //     Debug.Log("Invalid up position");
                // }
                downValid = false;
            }
            else if (eventType == EventType.MouseDrag && downValid && current.modifiers == EventModifiers.None)
            {
                // current.Use();
                // upPosition = current.mousePosition;
                // upPosition.x = camera.pixelWidth * upPosition.x / rect.width;
                // upPosition.y = camera.pixelHeight * (rect.height - upPosition.y) / rect.height;
                // Vector3 upPositionWorldSpace;
                // RaycastScreenWithSphere(camera, upPosition, out upPositionWorldSpace);
                // SetCubemapPixelByDirection(cubemap, upPositionWorldSpace.normalized, Color.black);
                // cubemap.Apply();

                current.Use();
                upPosition = current.mousePosition;
                upPosition.x = camera.pixelWidth * Mathf.Clamp01(upPosition.x / rect.width);
                upPosition.y = camera.pixelHeight * Mathf.Clamp01((rect.height - upPosition.y) / rect.height);

                // Will need downPositionWorldSpace later
                Vector3 downPositionWorldSpace, upPositionWorldSpace;
                RaycastScreenWithSphere(camera, downPosition, out downPositionWorldSpace);
                if (RaycastScreenWithSphere(camera, upPosition, out upPositionWorldSpace))
                {
                    SetCubemapPixelByDirection(cubemap, upPositionWorldSpace.normalized, Color.black);
                    var coordinates = GetCubemapArcCoordinates(cubemap, downPositionWorldSpace, upPositionWorldSpace);
                    foreach (var coordinate in coordinates)
                    {
                        cubemap.SetPixel(coordinate.Face, (int)(coordinate.U*(float)cubemap.width), (int)(coordinate.V*(float)cubemap.width), Color.black);
                    }
                    // Debug.LogFormat("Up: {0}", new CubemapCoordinate(upPositionWorldSpace).ToString());
                }
                else
                {
                    // Debug.Log("Invalid up position");
                }

                downPosition = upPosition;
                correctCallCount++;
            }

        }

        bool RaycastScreenWithSphere(Camera camera, Vector2 positionScreenSpace, out Vector3 intersectionWorldSpace)
        {
            var cameraWorldSpace = camera.gameObject.transform.position;
            var positionWorldSpace = camera.ScreenToWorldPoint(new Vector3(positionScreenSpace.x, positionScreenSpace.y, camera.nearClipPlane));
            var directionWorldSpace = (positionWorldSpace - cameraWorldSpace).normalized;

            var ray = new Ray(cameraWorldSpace, directionWorldSpace);
            return ray.IntersectsWithSphere(out intersectionWorldSpace);
        }

        void SetCubemapPixelByDirection(Cubemap cubemap, Vector3 direction, Color color)
        {
            // Direction -> Cubemap -> Direction -> Cubemap for demonstrative purposes.
            var coordinate = new CubemapCoordinate(new CubemapCoordinate(direction).ToDirection());
            cubemap.SetPixel(coordinate.Face, (int)(coordinate.U * cubemap.width), (int)(coordinate.V * cubemap.width), color);
        }

        IEnumerable<CubemapCoordinate> GetCubemapArcCoordinates(Cubemap cubemap, Vector3 startPosition, Vector3 endPosition)
        {
            var startCoordinate = new CubemapCoordinate(startPosition).PixelCenter(cubemap.width);
            var endCoordinate = new CubemapCoordinate(endPosition).PixelCenter(cubemap.width);

            startPosition = startCoordinate.ToDirection();
            endPosition = endCoordinate.ToDirection();
            var lineDirection = (endPosition - startPosition).normalized;

            if (startCoordinate.Face != endCoordinate.Face)
            {
                Debug.LogWarningFormat("Drawing across faces not supported yet. {0} -> {1}", startCoordinate, endCoordinate);
                return new CubemapCoordinate[0];
            }

            var coordinates = new List<CubemapCoordinate> { startCoordinate };

            int i = 0;
            bool complete = startCoordinate.Equals(endCoordinate, 1f / (float)cubemap.width);
            while (!complete && i < 1000)
            {
                var lastPosition = coordinates.Last().ToDirection().normalized;
                var currentCoordinate = coordinates
                    .Last()
                    .GetNeighbors(cubemap.width)
                    // .MinBy((candidateCoordinate) => UnitSphereUtil.GreatCircleDistance(candidateCoordinate.ToDirection(), endPosition));
                    .MinBy((candidateCoordinate) => (endPosition - candidateCoordinate.ToDirection()).magnitude);
                coordinates.Add(currentCoordinate);
                complete = currentCoordinate.Equals(endCoordinate, 1f / (float)cubemap.width);
                i++;
            }

            if (i == 1000)
            {
                Debug.LogWarning("Max iterations reached");
            }

            // Debug.LogFormat("Start: {0}\nEnd: {1}", startCoordinate, endCoordinate);
            // Debug.Log(string.Join("\n", coordinates.Select(c => c.ToString()).ToArray()));

            return coordinates;
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
            lastTime = DateTime.Now;
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