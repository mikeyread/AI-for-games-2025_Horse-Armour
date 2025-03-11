using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavMesh_Script : MonoBehaviour
{
    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    // Start is called before the first frame update

    public Vector3 TLCorner { get; set; }
    public Vector3 TRCorner { get; set; }
    public Vector3 BLCorner { get; set; }
    public Vector3 BRCorner { get; set; }

    public int Direction { get; set; }

    void Start()
    {
        nodes.Add(new NavMeshNode(transform.position));
        
     

        while (nodes.Count < 500)
        {
            //if (nodes.LastOrDefault().Pos > )
            //{

            //}
            switch (Direction)
            {
                case 0: // right
                    {
                        AddNode(1,0);
                        break;
                    }
                case 1: //down
                    {
                        AddNode(0,-1);
                        break;
                    }
                case 2: // left
                    {
                        AddNode(-1,0);
                        break;
                    }
                case 3: // up
                    {
                        AddNode(0,1);
                        break;
                    }
            }
        }



    }

    private bool AddNode(int x, int y)
    {
        nodes.Add(new NavMeshNode((nodes.LastOrDefault().Pos) + new Vector3(x, y)));
        return true;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
public class NavMeshNode
{
    public Vector3 Pos { get; set; }
    double PosX = 0;
    double PosY = 0; // might not be needed
    double PosZ = 0;
    public NavMeshNode()
    {

    }
    public NavMeshNode(Vector3 Pos)
    {
        this.Pos = Pos;
    }
}
