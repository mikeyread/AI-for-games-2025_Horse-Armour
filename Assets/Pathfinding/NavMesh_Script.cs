using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh_Script : MonoBehaviour
{
    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    // Start is called before the first frame update
    void Start()
    {
        nodes.Add(new NavMeshNode(transform.position));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class NavMeshNode
{
    double PosX = 0;
    double PosY = 0; // might not be needed
    double PosZ = 0;
    public NavMeshNode()
    {

    }
    public NavMeshNode(Vector3 Pos)
    {

    }
}
