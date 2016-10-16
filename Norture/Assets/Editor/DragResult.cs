using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class DragResult
{
    public Vector2 Delta;
    public EventModifiers Modifiers;
    public bool IsDragging;
}