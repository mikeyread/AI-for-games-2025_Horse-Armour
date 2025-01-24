using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Literally the most basic movement script I can make so I can move the camera.
public class BasicMovement : MonoBehaviour
{
    private Vector3 velocity = new Vector3(0,0,0);

    private static float moveSpeed = 0.002f;
    private static float sensitivity = 1.2f;

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

        float horInput = Input.GetAxisRaw("Horizontal") * moveSpeed;
        float verInput = Input.GetAxisRaw("Vertical") * moveSpeed;

        transform.localRotation = Quaternion.Euler(horInput, 0, verInput);

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        Vector3 forwardRelative = verInput * camForward;
        Vector3 rightRelative = verInput * camRight;

        Vector3 moveDir = forwardRelative + rightRelative;

        velocity = new Vector3(horInput, velocity.y, verInput);

        this.transform.Translate(velocity);
    }
}
