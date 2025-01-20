using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LogSpam : MonoBehaviour
{
    Vector3 position;
    Vector3 velocity;

    void Start()
    {
        this.position = transform.position;
        Debug.Log("Log Spam Started");
    }

    void Update()
    {
        velocity.y -= 0.0001f;
        this.position += velocity;
        transform.position = position;

        if (transform.position.y <= -2.0f)
        {
            velocity *= -1;
        }

        Debug.Log("Log Spam Updated");
    }
}
