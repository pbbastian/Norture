using UnityEngine;
using UnityEditor;
using System;

namespace Norture
{
    public class ObjectSetter : EditorWindow
    {

        public static ObjectSetter window;
        private static GameObject obj;
        private static DataHolder data;
        private static PreviewRenderUtility previewRenderUtility;
        private Vector2 drag;

        [MenuItem("Tools/ObjectSetter")]
        public static void OpenWindow()
        {
            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup();
            }
            previewRenderUtility = new PreviewRenderUtility();

            window = EditorWindow.GetWindow<ObjectSetter>();
            window.titleContent.text = "Object Setter";

            var gameObject = EditorUtility.CreateGameObjectWithHideFlags("ObjectSetterCamera", HideFlags.HideAndDontSave, new Type[]
            {
            typeof(Camera)
            });
            var camera = gameObject.GetComponent<Camera>();
            CameraExtensions.SetOnlyDrawMesh(camera);

            DestroyImmediate(gameObject, true);
        }

        void OnGUI()
        {
            if (window == null)
            {
                OpenWindow();
            }

            if (Selection.activeGameObject != null)
            {
                obj = Selection.activeGameObject;
                GUILayout.Label("Currently selected object: " + obj.name);

                data = obj.GetComponent<DataHolder>();
                if (data != null)
                {
                    var tempData = data.next;
                    EditorGUI.BeginChangeCheck();
                    data.next = (DataHolder)EditorGUILayout.ObjectField(
                        "Next Object",
                        data.next,
                        typeof(DataHolder),
                        true
                    );
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (data.next != null)
                        {
                            data.next.previous = obj.GetComponent<DataHolder>();
                        }
                        else
                        {
                            tempData.previous = null;
                        }
                    }

                    data.enableOnLoad = EditorGUILayout.Toggle(
                        "Enable On Load",
                        data.enableOnLoad
                    );
                }
                else
                {
                    if (GUILayout.Button("Add DataHolder"))
                    {
                        obj.AddComponent<DataHolder>();
                    }
                }

                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer.sharedMaterial != null)
                {
                    var rect = GUILayoutUtility.GetRect(100, 100);
                    //drag = DragUtility.Drag2D(rect);
                    var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

                    
                    previewRenderUtility.BeginPreview(rect, GUIStyle.none);
                    previewRenderUtility.DrawMesh(mesh, Matrix4x4.identity, renderer.sharedMaterial, 0);
                    // Graphics.DrawMesh()

                    previewRenderUtility.m_Camera.transform.position = Vector2.zero;
                    previewRenderUtility.m_Camera.transform.rotation =
                        Quaternion.Euler(new Vector3(-drag.y, -drag.x, 0));
                    previewRenderUtility.m_Camera.transform.position =
                        previewRenderUtility.m_Camera.transform.forward * -6f;
                    previewRenderUtility.m_Camera.Render();

                    Texture resultRender = previewRenderUtility.EndPreview();

                    GUI.DrawTexture(rect, resultRender, ScaleMode.StretchToFill, false);
                }

                int i = 0;
                foreach (DataHolder dataObj in FindObjectsOfType(typeof(DataHolder)))
                {
                    if (data != null)
                    {
                        GUI.backgroundColor =
                            data.previous == dataObj ? new Color(1f, 0f, 0f) :
                            data.next == dataObj ? new Color(0f, 1f, 0f) :
                            data == dataObj ? new Color(0.5f, 0.5f, 0.5f) :
                            GUI.backgroundColor;
                    }

                    if (GUILayout.Button(dataObj.gameObject.name))
                    {
                        Selection.activeGameObject = dataObj.gameObject;
                    }

                    GUI.backgroundColor = new Color(1f, 1f, 1f);
                    ++i;
                }
            }
        }

        void OnDestroy()
        {
            previewRenderUtility.Cleanup();
            //renderPreview.Dispose();
        }

        void Update()
        {
            Repaint();
        }

        

    }
}