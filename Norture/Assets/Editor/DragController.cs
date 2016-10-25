using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class DragController
{
    public Vector2 Position;

    public bool IsDragging;

    private EventModifiers m_modifierMask;

    public DragController(EventModifiers modifierMask)
    {
        m_modifierMask = modifierMask;
    }

    public void Sample(Rect position)
    {
        int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        Event current = Event.current;

        switch (current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && position.width > 50f && current.modifiers == m_modifierMask)
                {
                    IsDragging = true;
                    GUIUtility.hotControl = controlID;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (IsDragging)
                {
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    IsDragging = false;
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID && IsDragging)
                {
                    var delta = current.delta;
                    delta.x /= position.width;
                    delta.y /= position.height;
                    Position += delta * 140f;
                    current.Use();
                    GUI.changed = true;
                }
                break;
        }
    }
}