using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    public float width = 32f;
    public float height = 32f;

    void Start()
    {
    }

    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Vector3 position = Camera.current.transform.position;

        for (float y = position.y - 800f; y < position.y + 800f; y += height)
        {
            Gizmos.DrawLine(
                new Vector3(-1e6f, Mathf.Floor(y / height) * height, 0f),
                new Vector3(1e6f, Mathf.Floor(y / height) * height, 0f)
            );
        }

        for (float x = position.x - 1200f; x < position.x + 1200f; x += width)
        {
            Gizmos.DrawLine(
                new Vector3(Mathf.Floor(x / width) * width, -1e6f, 0f),
                new Vector3(Mathf.Floor(x / width) * width, 1e6f, 0f)
            );
        }
    }
}
