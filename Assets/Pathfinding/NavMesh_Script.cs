using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class NavMesh_Script : MonoBehaviour
{
    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    // Start is called before the first frame update

    public Vector3 TLCorner { get; set; }
    //public Vector3 TRCorner { get; set; }
    //public Vector3 BLCorner { get; set; }
    public Vector3 BRCorner { get; set; }

    public int Direction { get; set; }

    void Start()
    {
        nodes.Add(new NavMeshNode(transform.position));
        TLCorner = nodes.LastOrDefault().Pos;
        BRCorner = nodes.LastOrDefault().Pos;
        //TLCorner = transform.position;
        //BRCorner = transform.position;
        Direction = 0;
        //while (nodes.Count < 0)
        //{
        //    if (nodes.LastOrDefault().Pos.z >= TLCorner.z + 1)
        //    {
        //        Direction = 0;
        //        TLCorner = new Vector3(TLCorner.x,nodes.LastOrDefault().Pos.z + 0, TLCorner.y);
        //        //TLCorner = nodes.LastOrDefault().Pos;
        //    }
        //    else if (nodes.LastOrDefault().Pos.x >= BRCorner.x + 1)
        //    {
        //        Direction = 1;
        //        BRCorner = new Vector3(nodes.LastOrDefault().Pos.x + 0, BRCorner.z, BRCorner.y);
        //    }
        //    else if (nodes.LastOrDefault().Pos.z <= BRCorner.z - 1)
        //    {
        //        Direction = 2;
        //        BRCorner = new Vector3(BRCorner.x, nodes.LastOrDefault().Pos.z - 0, BRCorner.y);
        //        //BRCorner = nodes.LastOrDefault().Pos;
        //    }
        //    else if (nodes.LastOrDefault().Pos.x <= TLCorner.x - 1)
        //    {
        //        Direction = 3;
        //        TLCorner = new Vector3(nodes.LastOrDefault().Pos.z - 0, TLCorner.z, TLCorner.y);
        //    }
        //    switch (Direction)
        //    {
        //        case 0: // right
        //            {
        //                AddNode(1,0,0);
        //                break;
        //            }
        //        case 1: //down
        //            {
        //                AddNode(0,0, -1);
        //                break;
        //            }
        //        case 2: // left
        //            {
        //                AddNode(-1,0,0);
        //                break;
        //            }
        //        case 3: // up
        //            {
        //                AddNode(0,0,1);
        //                break;
        //            }
        //    }
        //}



    }

    private bool AddNode(int x, int y, int z)
    {
        nodes.Add(new NavMeshNode((nodes.LastOrDefault().Pos) + new Vector3(x, y, z)));
        return true;
    }


    // Update is called once per frame
    void Update()
    {


    }
    private void SpawnNodesSpiral(int total)
    {
        for (int i = 0; i < total; i++)
        {
            if (nodes.LastOrDefault().Pos.z >= TLCorner.z + 1) //turn right
            {
                Direction = 0;
                TLCorner = new Vector3(TLCorner.x, TLCorner.y, nodes.LastOrDefault().Pos.z + 0);
                //TLCorner = nodes.LastOrDefault().Pos;
            }
            if (nodes.LastOrDefault().Pos.x >= BRCorner.x + 1)
            {
                Direction = 1;
                BRCorner = new Vector3(nodes.LastOrDefault().Pos.x + 0, BRCorner.y, BRCorner.z);
            }
            if (nodes.LastOrDefault().Pos.z <= BRCorner.z - 1)
            {
                Direction = 2;
                BRCorner = new Vector3(BRCorner.x, BRCorner.y, nodes.LastOrDefault().Pos.z - 0);
                //BRCorner = nodes.LastOrDefault().Pos;
            }
            if (nodes.LastOrDefault().Pos.x <= TLCorner.x - 1)
            {
                Direction = 3;
                TLCorner = new Vector3(nodes.LastOrDefault().Pos.x - 0, TLCorner.y, TLCorner.z);
            }
            switch (Direction)
            {
                case 0: // right
                    {
                        AddNode(1, 0, 0);
                        break;
                    }
                case 1: //down
                    {
                        AddNode(0, 0, -1);
                        break;
                    }
                case 2: // left
                    {
                        AddNode(-1, 0, 0);
                        break;
                    }
                case 3: // up
                    {
                        AddNode(0, 0, 1);
                        break;
                    }
            }
        }

    }

    private void OnDrawGizmos()
    {
        if (nodes == null) return;
        foreach(var Node in nodes)
        {
            Gizmos.DrawWireCube(Node.Pos, Vector3.one);
        }
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
