using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Literally the most basic movement script I can make so I can move the camera.
public class BasicMovement : MonoBehaviour
{
    private Vector3 velocity = new Vector3(0,0,0);

    private float moveSpeed = 10f;
    private CursorLockMode isLocked = CursorLockMode.Locked;


    private void Start()
    {
        Cursor.lockState = isLocked;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            isLocked = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0)) {
            isLocked = CursorLockMode.Locked;
        }

        if (isLocked != CursorLockMode.Locked) return;

        staticMovement();
    }


    // Very basic locked Axis movement for testing, no camera rotation for free-look.
    private void staticMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            velocity += new Vector3(0, 0, moveSpeed) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += new Vector3(-moveSpeed, 0, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity += new Vector3(0, 0, -moveSpeed) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += new Vector3(moveSpeed, 0, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            velocity += new Vector3(0, moveSpeed, 0) * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity += new Vector3(0, -moveSpeed, 0) * Time.deltaTime;
        }

        this.transform.position += velocity * Time.deltaTime;
    }
}
