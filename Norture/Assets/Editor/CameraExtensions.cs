using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Norture
{
    public static class CameraExtensions
    {
        private static MethodInfo setCameraOnlyDrawMesh = typeof(Handles).GetMethod("SetCameraOnlyDrawMesh", BindingFlags.NonPublic | BindingFlags.Static);

        public static void SetOnlyDrawMesh(this Camera camera)
        {
            setCameraOnlyDrawMesh.Invoke(null, new[] { camera });
        }
    }
}