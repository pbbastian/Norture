using UnityEngine;
using UnityEditor;
using System;
using Norture.Extensions;
using UnityEngine.Rendering;
using Norture.Matrix;

namespace Norture
{
    public class NortureWindow : EditorWindow
    {
        PreviewRenderUtility _previewRenderUtility;
        PreviewRenderUtility _viewCubePRU;
        Mesh _sphereMesh;
        Rect _viewCubeRect;
        Mesh _viewCubeMesh;
        Material _viewCubeMaterial;
        Texture _viewCubeRenderTexture;
        Material _material;
        Cubemap _cubemap;
        [SerializeField] DragController _dragController;
        [SerializeField] Vector2 _cameraDrag;
        Vector2 _downPosition;
        bool _downValid = false;
        Vector2 _upPosition;
        Texture _previewTexture;
        Rect _previewRect;
        bool _previewDirty = false;
        const int CubemapResolution = 256;
        DateTime _lastTime;
        TimeSpan _currentTimeSpan;
        int _callCount = 0;
        int _correctCallCount = 0;
        float[] _mask;
        BrushWorker _brushWorker;
        DateTime _lastPaint;
        int cubeUpdate = 0;

        public float BrushRadius = 5;
        public Color BrushColor = Color.white;
        public bool UseSoftBrush = false;

        [MenuItem("Window/Norture")]
        public static void ShowWindow()
        {
            GetWindow<NortureWindow>(true, "Norture");
        }

        void Update()
        {
            if (_previewDirty || Application.isPlaying)
            {
                // Debug.LogFormat("Request count: {0}", _brushWorker.RequestCount);
                _previewDirty = false;
                if (cubeUpdate % 2 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        _cubemap.SetPixels(_brushWorker.Colors[i], (CubemapFace)i);
                    }
                    _cubemap.Apply();
                }
                cubeUpdate++;

                _viewCubePRU.BeginPreview(_viewCubeRect, GUIStyle.none);
                SetCameraTransform(_viewCubePRU.m_Camera);
                _viewCubePRU.DrawMesh(_viewCubeMesh, Matrix4x4.TRS(new Vector3(0f, -0.25f, 0f), Quaternion.identity, Vector3.one * 0.5f), _viewCubeMaterial, 0);
                _viewCubePRU.m_Camera.Render();
                _viewCubeRenderTexture = _viewCubePRU.EndPreview();

                _previewRenderUtility.BeginPreview(_previewRect, GUIStyle.none);
                SetCameraTransform(_previewRenderUtility.m_Camera);
                _previewRenderUtility.DrawMesh(_sphereMesh, Matrix4x4.identity, _material, 0);

                _previewRenderUtility.m_Camera.Render();

                _previewTexture = _previewRenderUtility.EndPreview();
            }

            Repaint();
        }

        void SetCameraTransform(Camera camera)
        {
            camera.transform.position = Vector2.zero;
            camera.transform.rotation = Quaternion.Euler(_dragController.Position.y,
                _dragController.Position.x, 0);
            camera.transform.position = camera.transform.forward * -6f;
        }

        void OnGUI()
        {
            _previewRect = GUILayoutUtility.GetRect(position.width, position.width);
            _viewCubeRect = _previewRect;
            _viewCubeRect.width *= 0.2f;
            _viewCubeRect.position = new Vector2(_viewCubeRect.position.x + _previewRect.width - _viewCubeRect.width, _viewCubeRect.position.y);
            _viewCubeRect.height *= 0.2f;
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

            GUI.DrawTexture(_previewRect, _previewTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(_viewCubeRect, _viewCubeRenderTexture, ScaleMode.StretchToFill, true);
            BrushRadius = EditorGUILayout.Slider("Brush radius", BrushRadius, 1f, 20f);
            BrushColor = EditorGUILayout.ColorField("Brush color", BrushColor);
            UseSoftBrush = EditorGUILayout.Toggle("Soft brush", UseSoftBrush);
            EditorGUILayout.ObjectField("Material", null, typeof(Material), false);
            EditorGUILayout.HelpBox("Please select a material that uses the Norture shader", MessageType.Info);

            if (GUILayout.Button("Fill"))
            {
                _brushWorker.Fill();
                _previewDirty = true;

                Debug.Log(CubemapMatrices.PositiveY_NegativeZ);
                Debug.Log(CubemapMatrices.PositiveY_NegativeZ.Inverted);
                Debug.Log(CubemapMatrices.PositiveY_NegativeX.Inverted);
            }

            _brushWorker.BrushColor = BrushColor;
            _brushWorker.BrushRadius = BrushRadius;
            _brushWorker.UseSoftBrush = UseSoftBrush;
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
                Debug.Log(current.mousePosition);
                current.Use();

                _downPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
                    .RelativeLocalToScreenPoint(camera);

                Vector3 positionWorldSpace;
                if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(_downPosition, out positionWorldSpace)) return;
                var coordinate = new CubemapCoordinate(positionWorldSpace.normalized);
                _brushWorker.PutBrushDown(coordinate.Face, (int)(coordinate.U * CubemapResolution), (int)(coordinate.V * CubemapResolution));

                _downValid = true;
            }
            else if (_downValid && eventType == EventType.MouseUp && current.modifiers == EventModifiers.None)
            {
                current.Use();

                var currentPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
                    .RelativeLocalToScreenPoint(camera);
                Vector3 positionWorldSpace;
                if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(currentPosition, out positionWorldSpace)) return;
                var coordinate = new CubemapCoordinate(positionWorldSpace.normalized);
                _brushWorker.PutBrushUp(coordinate.Face, (int)(coordinate.U * CubemapResolution), (int)(coordinate.V * CubemapResolution));

                _downValid = false;
            }
            else if (eventType == EventType.MouseDrag && _downValid && current.modifiers == EventModifiers.None)
            {
                current.Use();


                _upPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
                    .RelativeLocalToScreenPoint(camera);

//                PaintScreenSpaceBrushOnSphere((int) _upPosition.x, (int) _upPosition.y);
                line((int) _downPosition.x, (int) _downPosition.y, (int) _upPosition.x, (int) _upPosition.y,
                    QueueBrushPaint);
//                var currentPosition = current.mousePosition.GlobalToRelativeLocalPoint(rect)
//                    .RelativeLocalToScreenPoint(camera);
//                Vector3 positionWorldSpace;
//                if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(currentPosition, out positionWorldSpace)) return;
//                var coordinate = new CubemapCoordinate(positionWorldSpace.normalized);
//                _brushWorker.Paint(coordinate.Face, (int)(coordinate.U * CubemapResolution), (int)(coordinate.V * CubemapResolution));

                _previewDirty = true;
                _downPosition = _upPosition;
                _correctCallCount++;
            }
        }

        void QueueBrushPaint(int x, int y)
        {
            var now = DateTime.Now;
            if (now - _lastPaint > TimeSpan.FromMilliseconds(Mathf.Lerp(0.05f, 0.5f, BrushRadius / 20f)))
            {
                //Debug.Log("more than 16 ms since last time");
                _lastPaint = now;
                Vector3 positionWorldSpace;
                if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x, y), out positionWorldSpace))
                    return;
                var coordinate = new CubemapCoordinate(positionWorldSpace.normalized);
                _brushWorker.Paint(coordinate.Face, (int) (coordinate.U * CubemapResolution),
                    (int) (coordinate.V * CubemapResolution));
            }
            else
            {
                //Debug.Log("less than 16 ms since last time");
            }
        }

//        void PlotScreenSpacePointOnSphere(int x, int y, Color color)
//        {
//            Vector3 positionWorldSpace;
//            if (_previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x, y), out positionWorldSpace))
//            {
//                var coordinate = new CubemapCoordinate(positionWorldSpace.normalized);
//                var currentColor = _cubemap.GetPixel(coordinate);
//                _cubemap.SetPixel(coordinate, Color.Lerp(currentColor, color, color.a));
//            }
//        }
//
//        void PlotScreenSpacePointOnSphere(int x, int y)
//        {
//            PlotScreenSpacePointOnSphere(x, y, BrushColor);
//        }
//
//        void PaintScreenSpaceBrushOnSphere(int x0, int y0)
//        {
//            var radiusSquared = BrushRadius * BrushRadius;
//            for (var x = x0 - BrushRadius; x <= x0 + BrushRadius; x++)
//            {
//                for (var y = y0 - BrushRadius; y <= y0 + BrushRadius; y++)
//                {
//                    var dx = x - x0;
//                    var dy = y - y0;
//                    double distanceSquared = dx * dx + dy * dy;
//                    double brushFactor = radiusSquared / distanceSquared;
//
//                    Vector3 positionWorldSpace;
//                    if (distanceSquared <= radiusSquared)
//                        PlotScreenSpacePointOnSphere((int) x, (int) y);
//                }
//            }
//        }
//
//        void PaintMaskBrush(int x0, int y0)
//        {
//            Vector3 positionWorldSpace;
//            if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x0, y0), out positionWorldSpace)) return;
//            var delta = 1f / (float) _cubemap.width;
//            var textureSpaceRadius = BrushRadius * 10e-2f * (float) _cubemap.width;
//            var radiusSquared = textureSpaceRadius * textureSpaceRadius;
//            var center = new CubemapCoordinate(positionWorldSpace.normalized);
//
//            for (var u = Mathf.Max(0f, center.U - textureSpaceRadius);
//                u <= Mathf.Min(1f, center.U + textureSpaceRadius);
//                u += delta)
//            {
//                for (var v = Mathf.Max(0f, center.V - textureSpaceRadius);
//                    v <= Mathf.Min(1f, center.V + textureSpaceRadius);
//                    v += delta)
//                {
//                    var du = u - center.U;
//                    var dv = v - center.V;
//                    double distanceSquared = du * du + dv * dv;
//
//                    if (distanceSquared > radiusSquared) continue;
//                }
//            }
//        }
//
//        void PaintCubemapBrush(int x0, int y0)
//        {
//            Vector3 positionWorldSpace;
//            if (!_previewRenderUtility.m_Camera.RaycastUnitSphere(new Vector2(x0, y0), out positionWorldSpace)) return;
//            var delta = 1f / (float) _cubemap.width;
//            var textureSpaceRadius = BrushRadius * 1e-2f;
//            var radiusSquared = textureSpaceRadius * textureSpaceRadius;
//            var center = new CubemapCoordinate(positionWorldSpace.normalized);
//            for (var u = Mathf.Max(0f, center.U - textureSpaceRadius);
//                u <= Mathf.Min(1f, center.U + textureSpaceRadius);
//                u += delta)
//            {
//                for (var v = Mathf.Max(0f, center.V - textureSpaceRadius);
//                    v <= Mathf.Min(1f, center.V + textureSpaceRadius);
//                    v += delta)
//                {
//                    var du = u - center.U;
//                    var dv = v - center.V;
//                    double distanceSquared = du * du + dv * dv;
//
//                    if (distanceSquared > radiusSquared) continue;
//
//                    var point = new CubemapCoordinate(center.Face, u, v);
//                    var currentColor = _cubemap.GetPixel(point);
//                    _cubemap.SetPixel(point, Color.Lerp(currentColor, BrushColor, BrushColor.a));
//                }
//            }
//        }

        public delegate void LineCallback(int x, int y);

        // SEE http://stackoverflow.com/a/11683720
        public void line(int x, int y, int x2, int y2, LineCallback plot)
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
            Debug.Log("OnEnable");
            _dragController = _dragController ?? new DragController(EventModifiers.Alt);
            _previewRenderUtility = new PreviewRenderUtility();
            _viewCubePRU = new PreviewRenderUtility();
            _sphereMesh = GetSphereMesh();
            _viewCubeMesh = GetViewCubeMesh();
            _viewCubeMaterial = GetViewCubeMaterial();
            _material = LoadMaterial();
            _cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/Norture.cubemap");
            if (_cubemap == null)
            {
                _cubemap = CreateCubemap();
                AssetDatabase.CreateAsset(_cubemap, "Assets/Norture.cubemap");
            }
            _material.SetTexture("_Cube", _cubemap);
            _previewTexture = new Texture2D(0, 0);
            _previewRect = new Rect(Vector2.zero, new Vector2(_cubemap.width, _cubemap.width));
            _previewDirty = true;
            _lastTime = DateTime.Now;
            _mask = new float[6*_cubemap.width*_cubemap.width];
            if (_brushWorker != null)
                _brushWorker.Dispose();
            _brushWorker = new BrushWorker(_cubemap, BrushColor, BrushRadius, UseSoftBrush);
            _brushWorker.Start();
            _lastPaint = DateTime.Now;
        }

//        void InitializeCubemap(Cubemap cubemap)
//        {
//            var faces = new[]
//            {
//                CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveY, CubemapFace.NegativeY,
//                CubemapFace.PositiveZ, CubemapFace.NegativeZ
//            };
//            var colors = new[]
//            {
//                new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1),
//                new Color(1, 0, 1)
//            };
//            int i = 0;
//
//            foreach (var face in faces)
//            {
//                for (int x = 0; x < CubemapResolution; ++x)
//                {
//                    for (int y = 0; y < CubemapResolution; ++y)
//                    {
//                        cubemap.SetPixel(face, x, y, colors[i]);
//                    }
//                }
//                ++i;
//            }
//
//            cubemap.Apply();
//        }

        void OnDisable()
        {
            Debug.Log("OnDisable");
            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
            _brushWorker.Dispose();
            _brushWorker = null;
        }

        Mesh GetSphereMesh()
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var mesh = Instantiate(meshFilter.sharedMesh);
            DestroyImmediate(gameObject);
            return mesh;
        }

        Mesh GetViewCubeMesh()
        {
            return AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/ViewCube/cube.obj");
        }

        Material GetViewCubeMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/ViewCube/Materials/01___Default.mat");
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