using UnityEngine;
using System.Collections;

public class rotateOnY : MonoBehaviour
{

    public bool rotateObject = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (rotateObject)
        {
            transform.Rotate(0, 20 * Time.deltaTime, 0);
        }

    }
}