using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    Grid grid;

    public void OnEnable()
    {
        grid = (Grid)target;
        SceneView.onSceneGUIDelegate = GridUpdate;
    }

    void GridUpdate(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.isKey && e.character == 'a')
        {
            GameObject obj;
            if (Selection.activeObject)
            {
                obj = (GameObject)Instantiate(Selection.activeObject);
                obj.transform.position = new Vector3(0f, 0f, 0f);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Grid width");
        grid.width = EditorGUILayout.FloatField(grid.width, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Grid height");
        grid.height = EditorGUILayout.FloatField(grid.height, GUILayout.Width(50));
        GUILayout.EndHorizontal();

        SceneView.RepaintAll();
    }
}
