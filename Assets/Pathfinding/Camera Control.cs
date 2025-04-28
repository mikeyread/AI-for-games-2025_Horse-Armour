using Flocking;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{


    [SerializeField]
    public NavMesh_Script NavMesh_Script = null;
    [SerializeField]
    public FlockManager flockManager = null;
    public float sensX;
    public float sensY;
    public float sensZ;

    public float moveSpeed = 10;
    public Transform orientation;
    public Rigidbody Player;
    float horizontalInput, verticalInput;
    Vector3 moveDirection;


    float XRotation;
    float YRotation;
    float ZRotation;

    Vector3 PathStart = Vector3.zero;
    Vector3 PathEnd = Vector3.zero;

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

        PlayerOderUnit();

        MovePLayer();

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
        //Debug.Log("test");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePLayer()
    {

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        orientation.transform.position = orientation.transform.position + moveDirection.normalized * moveSpeed * 10f;
        //Player.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

    }

    private void PlayerOderUnit()
    {

        //Debug.Log("FrameUpdate");
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("mouse");
            Vector3 target = new Vector3();
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                target = hit.point;
            }

            //target.y += 1;

            PathStart = target;

            //Unit.transform.position = target;

            //Debug.Log("Start path at " + PathStart);

        }

        if (Input.GetMouseButtonUp(1))
        {
            Vector3 target = new Vector3();
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                target = hit.point;
            }

            //target.y = Unit.transform.position.y;
            //target.y += 2;

            PathEnd = target;

            //Debug.Log("end path at " + PathEnd);

            //Unit.transform.position = Vector3.Lerp(Unit.unitStart, Unit.unitEnd, 0.5f);
            //Unit.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Unit.unitStart, Unit.unitEnd));

            //Unit.transform.position = target;
            //if (PathStart != Vector3.zero && PathEnd != Vector3.zero)

            PathStart = flockManager.transform.position;

            Debug.Log("Start path at " + PathStart);
            Debug.Log("end path at " + PathEnd);

            NavMesh_Script.CurrentShortestPath = new List<AStarNode>();
            //NavMesh_Script.FindPath(PathStart, PathEnd);
            //flockManager.pathNodes = NavMesh_Script.FindPath(PathStart, PathEnd);
            flockManager.UpdateDestination(NavMesh_Script.FindPath(PathStart, PathEnd));
                
        }

    }

}
