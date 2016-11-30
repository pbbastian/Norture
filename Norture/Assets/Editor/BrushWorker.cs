﻿using System;
using System.Security.Principal;
using System.Threading;
using DisruptorUnity3d;
using UnityEngine;
using UnityEngine.Assertions;
using Norture.Extensions;

namespace Norture
{
    public class BrushWorker : IDisposable
    {
        private readonly Thread _thread;
        private readonly RingBuffer<BrushWorkerRequest> _requestQueue;
        private readonly float[][] _mask;
        private readonly float[] _emptyMask;
        private readonly Color[][][] _colors;
        private readonly int _cubemapSize;

        public bool UseSoftBrush;
        public float BrushRadius;

        private Color _brushColor;
        private float _brushOpacity;

        public Color BrushColor
        {
            get
            {
                var color = _brushColor;
                color.a = _brushOpacity;
                return color;
            }
            set
            {
                _brushColor = value;
                _brushOpacity = _brushColor.a;
                _brushColor.a = 1f;
            }
        }

        public Color[][] Colors
        {
            get { return _colors[DestinationIndex]; }
        }

        public int RequestCount { get; private set; }
        bool _disposed;
        const int SourceIndex = 0;
        const int DestinationIndex = 1;


        public BrushWorker(Cubemap cubemap, Color brushColor, float brushRadius, bool useSoftBrush)
        {
            _cubemapSize = cubemap.width;
            BrushColor = brushColor;
            BrushRadius = brushRadius;
            UseSoftBrush = useSoftBrush;

            _colors = new Color[2][][];
            for (var i = 0; i < 2; i++)
            {
                _colors[i] = new Color[6][];
                for (var face = 0; face < 6; face++)
                {
                    _colors[i][face] = new Color[_cubemapSize * _cubemapSize];
                    cubemap.GetPixels((CubemapFace)face).CopyTo(_colors[i][face], 0);
                }
            }

            _mask = new float[6][];
            for (var i = 0; i < 6; i++)
                _mask[i] = new float[_cubemapSize * _cubemapSize];
            _emptyMask = new float[_cubemapSize * _cubemapSize];

            _requestQueue = new RingBuffer<BrushWorkerRequest>(100000);

            _thread = new Thread(ThreadStart);
        }

        public void Start()
        {
            Debug.Log("starting thread");
            _thread.Start();
        }

        public void Paint(CubemapFace face, int x, int y)
        {
            RequestCount++;
            _requestQueue.Enqueue(new BrushWorkerRequest
            {
                Type = BrushWorkerRequestType.Paint,
                Face = face,
                X = x,
                Y = y
            });
        }

        public void Fill()
        {
            RequestCount++;
            _requestQueue.Enqueue(new BrushWorkerRequest
            {
                Type = BrushWorkerRequestType.Fill
            });
        }

        public void PutBrushDown(CubemapFace face, int x, int y)
        {
            RequestCount++;
            _requestQueue.Enqueue(new BrushWorkerRequest
            {
                Type = BrushWorkerRequestType.PutBrushDown,
                Face = face,
                X = x,
                Y = y
            });
        }

        public void PutBrushUp(CubemapFace face, int x, int y)
        {
            RequestCount++;
            _requestQueue.Enqueue(new BrushWorkerRequest
            {
                Type = BrushWorkerRequestType.PutBrushUp,
                Face = face,
                X = x,
                Y = y
            });
        }

        void ThreadStart()
        {
            while (!_disposed)
            {
                BrushWorkerRequest operation;
                if (!_requestQueue.TryDequeue(out operation))
                {
                    // Debug.Log("sleeping thread");
                    Thread.Sleep(16);
                    continue;
                }
                //                Debug.LogFormat("{0} - {1} estimated remaining",
                //                    Enum.GetName(typeof(BrushWorkerRequestType), operation.Type),
                //                    _requestQueue.Count);
                try
                {
                    switch (operation.Type)
                    {
                        case BrushWorkerRequestType.Paint:
                            PerformPaint(operation.Face, operation.X, operation.Y);
                            break;
                        case BrushWorkerRequestType.Fill:
                            PerformFill();
                            break;
                        case BrushWorkerRequestType.PutBrushDown:
                            PerformPutBrushDown(operation.Face, operation.X, operation.Y);
                            break;
                        case BrushWorkerRequestType.PutBrushUp:
                            PerformPutBrushUp(operation.Face, operation.X, operation.Y);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        void PerformPaint(CubemapFace originalFace, int x0, int y0)
        {
            var neighbors = originalFace.GetNeighbors();
            var neighborMatrices = originalFace.GetNeighborMatrices();

            PaintFaceBrush(originalFace, x0, y0);
            var uv = new Vector2(x0 / (float)_cubemapSize, y0 / (float)_cubemapSize);

            for (var i = 0; i < neighbors.Length; i++)
            {
                var face = neighbors[i];

                var origin = neighborMatrices[i].TransformPointFast(uv);

                // Transformed x0 and y0
                var x0t = (int)(origin.x * _cubemapSize);
                var y0t = (int)(origin.y * _cubemapSize);

                PaintFaceBrush(face, x0t, y0t);
            }
        }

        void PaintFaceBrush(CubemapFace face, int x0, int y0)
        {
            var radius = BrushRadius * 1e-2f * _cubemapSize;
            var upperRadius = (int)Mathf.Ceil(radius);
            var radiusSquared = radius * radius;

            var sourceColors = _colors[SourceIndex][(int)face];
            var destinationColors = _colors[DestinationIndex][(int)face];

            var lowerX = Math.Max(0, x0 - upperRadius);
            var lowerY = Math.Max(0, y0 - upperRadius);

            var upperX = Math.Min(_cubemapSize, x0 + upperRadius);
            var upperY = Math.Min(_cubemapSize, y0 + upperRadius);

            for (var x = lowerX; x < upperX; x++)
            {
                for (var y = lowerY; y < upperY; y++)
                {
                    var dx = x - x0;
                    var dy = y - y0;
                    var distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared > radiusSquared) continue;

                    var mask = _mask[(int)face];
                    var index = x + y * _cubemapSize;
                    Assert.IsTrue(index >= 0 || index < _cubemapSize * _cubemapSize);

                    float factor;
                    if (UseSoftBrush)
                    {
                        factor = (1f - (distanceSquared / radiusSquared));
                    }
                    else
                    {
                        factor = 1f; // (1f - Mathf.Pow((distanceSquared / radiusSquared), BrushRadius));
                    }

                    mask[index] = Mathf.Max(mask[index], _brushOpacity * factor);

                    var sourceColor = sourceColors[index];
                    var opacity = mask[index];
                    // destinationColors[index] = new Color((sourceColor.r * (1f - opacity) + _brushColor.r * opacity), (sourceColor.b * (1f - opacity) + _brushColor.g * opacity), (sourceColor.g * (1f - opacity) + _brushColor.b * opacity), 1f);
                    destinationColors[index] = Color.Lerp(sourceColor, _brushColor, opacity);
                }
            }
        }

        void PerformFill()
        {
            for (var face = 0; face < 6; face++)
            {
                var sourceColors = _colors[SourceIndex][face];
                var destinationColors = _colors[DestinationIndex][face];

                for (var x = 0; x < _cubemapSize; x++)
                {
                    for (var y = 0; y < _cubemapSize; y++)
                    {
                        var index = x + y * _cubemapSize;

                        // new Color(x / (float)_cubemapSize, y / (float)_cubemapSize, 0f, 1f)
                        destinationColors[index] = _brushColor;
                        sourceColors[index] = destinationColors[index];
                    }
                }
            }
        }

        void PerformPutBrushDown(CubemapFace face, int x, int y)
        {
            PerformPaint(face, x, y);
        }

        void PerformPutBrushUp(CubemapFace face, int x, int y)
        {
            for (var i = 0; i < 6; i++)
            {
                _colors[DestinationIndex][i].CopyTo(_colors[SourceIndex][i], 0);
                _emptyMask.CopyTo(_mask[i], 0);
            }
        }

        // https://msdn.microsoft.com/en-us/library/fs2xkftw(v=vs.110).aspx
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BrushWorker()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            Debug.Log("stopping thread");
            _thread.Abort();
            _disposed = true;
        }
    }

    public struct BrushWorkerRequest
    {
        public BrushWorkerRequestType Type;
        public CubemapFace Face;
        public int X;
        public int Y;
    }

    public enum BrushWorkerRequestType
    {
        Paint,
        Fill,
        PutBrushDown,
        PutBrushUp
    }
}