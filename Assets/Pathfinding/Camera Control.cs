using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public float sensX;
    public float sensY;
    public float sensZ;

    public float moveSpeed;
    public Transform orientation;
    public Rigidbody Player;
    float horizontalInput, verticalInput;
    Vector3 moveDirection;


    float XRotation;
    float YRotation;
    float ZRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        CameraMovement();

        PlayerMovementInput();



    }

    private void CameraMovement()
    {
        if (Input.GetMouseButton(2))
        {
            //float mouseX = Input.mousePosition.x;

            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
            // float mouseZ = Inpu

            XRotation -= mouseY;
            YRotation += mouseX;

            //transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
            orientation.rotation = Quaternion.Euler(XRotation, YRotation, 0);

        }
    }
    private void PlayerMovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePLayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        Player.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

}
