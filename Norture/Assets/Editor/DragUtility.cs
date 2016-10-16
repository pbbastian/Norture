using UnityEditor;
using UnityEngine;

namespace Norture
{
    public static class DragUtility
    {
        public static DragResult Drag2D(Rect position, DragState previousState)
        {
            var result = new DragResult();
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        result.Delta = current.delta / Mathf.Min(position.width, position.height) * 140f;
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return result;
        }

        public struct DragResult
        {
            public Vector2 Delta;
            public DragState State;
        }

        public struct DragState
        {
            public EventModifiers Modifier;
            public bool IsDragging;
        }
    }
}
