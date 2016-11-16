using UnityEngine;

namespace Norture.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 GlobalToLocalPoint(this Vector2 globalPosition, Rect rect)
        {
            return rect.GlobalToLocalPoint(globalPosition);
        }

        public static Vector2 GlobalToRelativeLocalPoint(this Vector2 globalPosition, Rect rect)
        {
            return rect.GlobalToRelativeLocalPoint(globalPosition);
        }

        public static Vector2 RelativeLocalToScreenPoint(this Vector2 relativeLocalPoint, Camera camera)
        {
            return camera.RelativeLocalToScreenPoint(relativeLocalPoint);
        }
    }
}