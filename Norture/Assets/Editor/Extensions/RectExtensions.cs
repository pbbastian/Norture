using UnityEngine;

namespace Norture.Extensions
{
    public static class RectExtensions
    {
        public static Vector2 GlobalToLocalPoint(this Rect rect, Vector2 globalPosition)
        {
            return globalPosition - rect.min;
        }

        public static Vector2 GlobalToRelativeLocalPoint(this Rect rect, Vector2 globalPosition)
        {
            var localPosition = rect.GlobalToLocalPoint(globalPosition);
            localPosition.x /= rect.width;
            localPosition.y /= rect.height;
            return localPosition;
        }
    }
}