﻿using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using Norture.Extensions;

namespace Norture
{
    public class NortureWindow : EditorWindow
    {
        private PreviewRenderUtility _previewRenderUtility;
        private Mesh _sphereMesh;
        private Material _material;
        private Cubemap _cubemap;
        [SerializeField] private DragController _dragController;
        [SerializeField] private Vector2 _cameraDrag;
        private Vector2 _downPosition;
        private bool _downValid = false;
        private Vector2 _upPosition;
        private Texture _previewTexture;
        private Rect _previewRect;
        private bool _previewDirty = false;
        private const int CubemapResolution = 512;
        private DateTime _lastTime;
        private TimeSpan _currentTimeSpan;
        private int _callCount = 0;
        private int _correctCallCount = 0;

        [MenuItem("Window/Norture")]
        public static void ShowWindow()
        {
            GetWindow<NortureWindow>("Norture");
        }

        void Update()
        {
            if (_previewDirty)
            {
                _cubemap.Apply();
                _previewRenderUtility.BeginPreview(_previewRect, GUIStyle.none);
                _previewRenderUtility.DrawMesh(_sphereMesh, Matrix4x4.identity, _material, 0);

                _previewRenderUtility.m_Camera.transform.position = Vector2.zero;
                _previewRenderUtility.m_Camera.transform.rotation = Quaternion.Euler(_dragController.Position.y,
                    _dragController.Position.x, 0);
                _previewRenderUtility.m_Camera.transform.position =
                    _previewRenderUtility.m_Camera.transform.forward * -6f;
                _previewRenderUtility.m_Camera.Render();

                _previewTexture = _previewRenderUtility.EndPreview();
                _previewDirty = false;
            }

            Repaint();
        }

        void OnGUI()
        {
            _previewRect = GUILayoutUtility.GetRect(position.width, position.width);
            // var rect = new Rect(0, 0, position.width, position.height);

            _dragController.Sample(_previewRect);
            if (_dragController.IsDragging)
            {
                _previewDirty = true;
            }

            HandleClick(_previewRect, _previewRenderUtility.m_Camera);

            var timeSpan = DateTime.Now - _lastTime;
            _currentTimeSpan += timeSpan;
            _callCount++;
            _lastTime = DateTime.Now;

            /* Input rate measurement
            if (_currentTimeSpan > TimeSpan.FromSeconds(1))
            {
                Debug.LogFormat("Calls per second: {0}", _callCount / _currentTimeSpan.TotalSeconds);
                Debug.LogFormat("Correct calls per second: {0}", _correctCallCount / _currentTimeSpan.TotalSeconds);
                _currentTimeSpan = TimeSpan.Zero;
                _callCount = 0;
                _correctCallCount = 0;
            }
            */

            GUI.DrawTexture(_previewRect, _previewTexture, ScaleMode.StretchToFill, false);
        }

        void HandleClick(Rect rect, Camera camera)
        {
            var current = Event.current;
            var eventType = current.GetTypeForControl(GUIUtility.GetControlID(FocusType.Passive));

            if (eventType == EventType.MouseMove)
            {
                current.Use();
                Debug.Log(current.mousePosition);
            }
            else if (eventType == EventType.MouseDown && current.modifiers == EventModifiers.None &&
                     rect.Contains(current.mousePosition))
            {
                current.Use();

                _downPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
                    .RelativeLocalToScreenPoint(camera);

                Vector3 downPositionWorldSpace;
                _downValid = true;
            }
            else if (_downValid && eventType == EventType.MouseUp && current.modifiers == EventModifiers.None)
            {
                _downValid = false;
            }
            else if (eventType == EventType.MouseDrag && _downValid && current.modifiers == EventModifiers.None)
            {
                current.Use();

                _upPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
                    .RelativeLocalToScreenPoint(camera);

                PaintScreenSpaceBrushOnSphere((int) _upPosition.x, (int) _upPosition.y);
//                line((int) _downPosition.x, (int) _downPosition.y, (int) _upPosition.x, (int) _upPosition.y,
//                    PaintScreenSpaceBrushOnSphere);

                _previewDirty = true;
                _downPosition = _upPosition;
                _correctCallCount++;
            }
        }

        void PlotScreenSpacePointOnSphere(int x, int y)
        {
            Vector3 positionWorldSpace;
            if (_previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x, y), out positionWorldSpace))
                _cubemap.SetPixel(positionWorldSpace.normalized, Color.black);
        }

        void PaintScreenSpaceBrushOnSphere(int x0, int y0)
        {
            const int radius = 20;
            var radiusSquared = radius * radius;
            for (var x = x0 - radius; x <= x0 + radius; x++)
            {
                for (var y = y0 - radius; y <= y0 + radius; y++)
                {
                    var dx = x - x0;
                    var dy = y - y0;
                    double distanceSquared = dx * dx + dy * dy;

                    Vector3 positionWorldSpace;
                    if (distanceSquared <= radiusSquared &&
                        _previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x, y), out positionWorldSpace))
                    {
                        _cubemap.SetPixel(positionWorldSpace.normalized, Color.black);
                    }
                }
            }
        }

        // SEE http://stackoverflow.com/a/11683720
        public void line(int x, int y, int x2, int y2, Action<int, int> plot)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1;
            else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1;
            else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1;
            else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1;
                else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                plot(x, y);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        /* OLD STUFF
        void BresenhamLine(int x0, int y0, int x1, int y1, Action<int, int> plot)
        {
            var deltaX = x1 - x0;
            var deltaY = y1 - y0;
            var error = -1.0f;

            if (deltaX == 0)
            {
                for (int y = y0; y <= y1; y++)
                {
                    plot(x0, y);
                }
            }
            else
            {
                var deltaError = Mathf.Abs((float)deltaY / (float)deltaX);
                var signY = Math.Sign(deltaY);
                int y = y0;
                for (int x = x0; x < x1; x++)
                {
                    plot(x, y);
                    error += deltaError;
                    if (error >= 0.0f)
                    {
                        y += signY;
                        error -= 1.0f;
                    }
                }
            }

        }

        void BresenhamLineInt(int x0, int y0, int x1, int y1, Action<int, int> plot)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var D = 2 * dy - dx;

            if (dx == 0)
            {
                for (int y = y0; y <= y1; y++)
                {
                    plot(x0, y);
                }
            }
            else
            {
                var y = y0;

                for (int x = x0; x < x1; x++)
                {
                    plot(x, y);
                    if (D >= 0)
                    {
                        y++;
                        D -= dx;
                    }
                    D += dy;
                }

                plot(x1, y);
            }
        }
        */

        void SetCubemapPixelByDirection(Cubemap cubemap, Vector3 direction, Color color)
        {
            // Direction -> Cubemap -> Direction -> Cubemap for demonstrative purposes.
            var coordinate = new CubemapCoordinate(new CubemapCoordinate(direction).ToDirection());
            cubemap.SetPixel(coordinate.Face, (int) (coordinate.U * cubemap.width), (int) (coordinate.V * cubemap.width),
                color);
        }

        /** Cubemap arc coordinates
        IEnumerable<CubemapCoordinate> GetCubemapArcCoordinates(Cubemap cubemap, Vector3 startPosition,
            Vector3 endPosition)
        {
            var startCoordinate = new CubemapCoordinate(startPosition).PixelCenter(cubemap.width);
            var endCoordinate = new CubemapCoordinate(endPosition).PixelCenter(cubemap.width);

            startPosition = startCoordinate.ToDirection();
            endPosition = endCoordinate.ToDirection();
            var lineDirection = (endPosition - startPosition).normalized;

            if (startCoordinate.Face != endCoordinate.Face)
            {
                Debug.LogWarningFormat("Drawing across faces not supported yet. {0} -> {1}", startCoordinate,
                    endCoordinate);
                return new CubemapCoordinate[0];
            }

            var coordinates = new List<CubemapCoordinate> {startCoordinate};

            int i = 0;
            bool complete = startCoordinate.Equals(endCoordinate, 1f / (float) cubemap.width);
            while (!complete && i < 1000)
            {
                var lastPosition = coordinates.Last().ToDirection().normalized;
                var currentCoordinate = coordinates
                    .Last()
                    .GetNeighbors(cubemap.width)
                    // .MinBy((candidateCoordinate) => UnitSphereUtil.GreatCircleDistance(candidateCoordinate.ToDirection(), endPosition));
                    .MinBy((candidateCoordinate) => (endPosition - candidateCoordinate.ToDirection()).magnitude);
                coordinates.Add(currentCoordinate);
                complete = currentCoordinate.Equals(endCoordinate, 1f / (float) cubemap.width);
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
        */

        void OnEnable()
        {
            _dragController = _dragController ?? new DragController(EventModifiers.Alt);
            _previewRenderUtility = new PreviewRenderUtility();
            _sphereMesh = GetSphereMesh();
            _material = LoadMaterial();
            _cubemap = CreateCubemap();
            InitializeCubemap(_cubemap);
            _material.SetTexture("_Cube", _cubemap);
            _previewTexture = new Texture2D(0, 0);
            _previewRect = new Rect(Vector2.zero, new Vector2(CubemapResolution, CubemapResolution));
            _previewDirty = true;
            _lastTime = DateTime.Now;
        }

        void InitializeCubemap(Cubemap cubemap)
        {
            var faces = new[]
            {
                CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY,
                CubemapFace.PositiveZ, CubemapFace.NegativeZ
            };
            var colors = new[]
            {
                new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1),
                new Color(1, 0, 1)
            };
            int i = 0;

            foreach (var face in faces)
            {
                for (int x = 0; x < CubemapResolution; ++x)
                {
                    for (int y = 0; y < CubemapResolution; ++y)
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
            _previewRenderUtility.Cleanup();
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
            return new Cubemap(CubemapResolution, TextureFormat.ARGB32, false);
        }
    }
}