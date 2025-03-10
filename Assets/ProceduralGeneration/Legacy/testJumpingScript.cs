using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LogSpam : MonoBehaviour
{
    Vector3 position;
    Vector3 velocity;
    float jumpHeight = 0;
    float gravity = 0.000033f;

    void Start()
    {
        this.position = transform.position;
        Debug.Log("Log Spam Started");
    }

    void Update()
    {
        if (jumpHeight < this.position.y) {
            jumpHeight = this.position.y;
        }

        if (transform.position.y - gravity <= 0)
        {
            float buffer = gravity - (transform.position.y - 0);
            this.position.y = buffer;
            Debug.Log(buffer);
            jumpHeight = 0;
            velocity *= -1;
        }

        velocity.y -= gravity;
        this.position += velocity;
        transform.position = position;

        Debug.Log(jumpHeight);
    }
}
