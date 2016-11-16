using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Norture.Extensions
{
    public static class CameraExtensions
    {
        private static MethodInfo setCameraOnlyDrawMesh = typeof(Handles).GetMethod("SetCameraOnlyDrawMesh", BindingFlags.NonPublic | BindingFlags.Static);

        public static void SetOnlyDrawMesh(this Camera camera)
        {
            setCameraOnlyDrawMesh.Invoke(null, new[] { camera });
        }

        public static bool RaycastUnitSphere(this Camera camera, Vector2 positionScreenSpace, out Vector3 intersectionWorldSpace)
        {
            var cameraWorldSpace = camera.gameObject.transform.position;
            var positionWorldSpace = camera.ScreenToWorldPoint(new Vector3(positionScreenSpace.x, positionScreenSpace.y, camera.nearClipPlane));
            var directionWorldSpace = positionWorldSpace - cameraWorldSpace;
            directionWorldSpace.Normalize();

            var ray = new Ray(cameraWorldSpace, directionWorldSpace);
            return ray.IntersectsWithSphere(out intersectionWorldSpace);
        }

        public static Vector2 RelativeLocalToScreenPoint(this Camera camera, Vector2 relativeLocalPoint)
        {
            relativeLocalPoint.y = 1f - relativeLocalPoint.y;
            relativeLocalPoint.Scale(camera.pixelRect.size);
            return relativeLocalPoint;
        }
    }
}