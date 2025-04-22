using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class NavMesh_Script : MonoBehaviour
{
    [SerializeField]
    public QuadtreeWorldGenerator quadtreeWorldGenerator;

    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    // Start is called before the first frame update

    public Vector3 TLCorner { get; set; }
    //public Vector3 TRCorner { get; set; }
    //public Vector3 BLCorner { get; set; }
    public Vector3 BRCorner { get; set; }

    public int Direction { get; set; }

    public int MaxTravelHight { get; set; }


    private float time = 0f;
    private bool done = false;

    void Start()
    {
        MaxTravelHight = 10;

        nodes.Add(new NavMeshNode(transform.position));
        //nodes.Sort();
        //nodes = nodes.OrderBy(NavMeshNode => NavMeshNode.Pos).ToList();
        TLCorner = nodes.LastOrDefault().Pos;
        BRCorner = nodes.LastOrDefault().Pos;

        //NavagateTree();

        //SpawnNodesSqure(transform.position, 25);


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

    private bool AddNodeFromLastNoded(int x, int y, int z)
    {
        nodes.Add(new NavMeshNode((nodes.LastOrDefault().Pos) + new Vector3(x, y, z)));
        return true;
    }


    // Update is called once per frame
    void Update()
    {
        //SpawnNodesSpiral(10);
        //nodes
        //nodes.Sort();
        //nodes = nodes.OrderBy(NavMeshNode => NavMeshNode.Pos.magnitude).ToList();
        if (time <= 5)
        {
            time += Time.deltaTime;
        }
        else
        {
            if (!done)
            {
                Debug.Log("Navigate Tree Triggered");
                done = true;
                NavagateTree();

            }
        }
    }

    //to get active leafs
    private void NavagateTree()
    {
        if (quadtreeWorldGenerator == null)
        {
            Debug.Log("Null Tree");
            return;

        }
        foreach (var Grid in quadtreeWorldGenerator.q_tree.GetActive())
        {

            Debug.Log("for each thing");
            if (Grid.IsLeaf())
            {
                Debug.Log("LEAF");


                //DEAL WITH THIS
                //if (Grid.n_depth >= quadtreeWorldGenerator.WorldSettings.Quadtree_maxDepth)
                //{
                    
                    SpawnNodesSqure(Grid.g_Position, (int)(Grid.chunk.c_MeshScale * Grid.chunk.c_MeshQuantity), Grid.chunk);

                    Debug.Log("Grid: " + Grid.g_Position);
                    Debug.Log("Chunk: " + Grid.chunk.c_MeshScale);
                    
                //}
            }

        }
    }

    private void SpawnNodesSqure(Vector3 StartingTLVector,int edgeLength, QuadTreeChunk chunk)
    {
        for /*(float z = StartingTLVector.z - edgeLength; z < (StartingTLVector.z); z++)*/  (float z = StartingTLVector.z /*- (edgeLength / 2)*/; z > (StartingTLVector.z - edgeLength); z--) 
        {
            for (float x = StartingTLVector.x /*- (edgeLength/2)*/; x < (StartingTLVector.x + edgeLength); x++)
            {
                Vector3 localoffset = new Vector3 (chunk.Chunk.transform.position.x - x,0 , chunk.Chunk.transform.position.z - z);
                int vertexIndex = ((int)(transform.position.x) + 1) + ((int)(transform.position.z) + 1) * (chunk.c_MeshQuantity + 2);
                Debug.Log(vertexIndex);
                float vertexY = chunk.chunkMesh.vertices[vertexIndex].y;
                nodes.Add(new NavMeshNode(new Vector3(x, vertexY /*transform.position.y*/, z)));
                nodes.LastOrDefault().ConnectNode(nodes);
            }

        }
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
                        AddNodeFromLastNoded(1, 0, 0);
                        break;
                    }
                case 1: //down
                    {
                        AddNodeFromLastNoded(0, 0, -1);
                        break;
                    }
                case 2: // left
                    {
                        AddNodeFromLastNoded(-1, 0, 0);
                        break;
                    }
                case 3: // up
                    {
                        AddNodeFromLastNoded(0, 0, 1);
                        break;
                    }
            }
        }

    }

    private void OnDrawGizmos()
    {
        if (nodes == null) return;
        List<NavMeshNode> Tnodes = nodes; // nodes.OrderBy(NavMeshNode => NavMeshNode.Pos.magnitude).ToList();
        for (int i = 0; i < nodes.Count; i++)
        {
            //if ( i < Tnodes.Count/4)
            //{
            //    Gizmos.color = Color.green;
            //}
            //else if (i < Tnodes.Count * 2/ 4)
            //{
            //    Gizmos.color = Color.yellow;
            //}
            //else if (i < Tnodes.Count * 3 / 4)
            //{
            //    Gizmos.color = Color.blue;
            //}
            //else if (i < Tnodes.Count * 4 / 4)
            //{
            //    Gizmos.color = Color.red;
            //}
            //else
            //{
            //    Gizmos.color = Color.black;
            //}

            if (nodes[i].ConnectedNodes.Count(x => x != null) >=  8)
            {
                Gizmos.color = Color.green;
            }
            else if (nodes[i].ConnectedNodes.Count(x => x != null) >= 7)
            {
                Gizmos.color = new Color(0.3f, 0.7f, 0f, 1f);
            }
            else if (nodes[i].ConnectedNodes.Count(x => x != null) >= 6)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0f, 1f);
            }
            else if (nodes[i].ConnectedNodes.Count(x => x != null) >= 5)
            {
                Gizmos.color = new Color(0.7f, 0.3f, 0f, 1f);
            }
            else if (nodes[i].ConnectedNodes.Count(x => x != null) >= 4)
            {
                Gizmos.color = new Color(0.9f, 0.1f, 0f, 1f);
            }
            else if (nodes[i].ConnectedNodes.Count(x => x != null) <= 3 && nodes[i].ConnectedNodes.Count(x => x != null) >= 1)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.black;
            }
            Gizmos.DrawWireCube(Tnodes[i].Pos, Vector3.one);
                //Gizmos.color = Color.yellow;
        }
        //foreach(var Node in nodes)
        //{

        //    Gizmos.DrawWireCube(Node.Pos, Vector3.one);
        //    Gizmos.color = Color.yellow;
        //}
    }

    private void JoinNode(NavMeshNode node)
    {

        for (int i = nodes.Count; i > -1; i--)
        {
            if (nodes[i].Pos.y > node.Pos.y - MaxTravelHight && nodes[i].Pos.y < node.Pos.y + MaxTravelHight)
            {
                if (nodes[i].Pos.x == node.Pos.x - 1 && nodes[i].Pos.y == node.Pos.x + 1)
                {

                }
            }

        }
    }

}
public class NavMeshNode
{
    public Vector3 Pos { get; set; }

    public int TerrainModifyer { get; set; }

    public NavMeshNodeConnections[] ConnectedNodes = new NavMeshNodeConnections[9];

    //public NavMeshNode[] ConnectedNodes = new NavMeshNode[9];
    public NavMeshNode()
    {
        TerrainModifyer = 1;
        //ConnectedNodes[4] = this;
    }
    public NavMeshNode(Vector3 Pos) : this()
    {

        //if (Physics.Linecast(new Vector3 (Pos.x,Pos.y + 1000, Pos.z), new Vector3(Pos.x, Pos.y - 1000, Pos.z), out RaycastHit hitinfo))
        //{
        //    Debug.Log("Hit at " + hitinfo.collider.gameObject.transform.position.y + " was tagged as " + hitinfo.collider.tag);
        //    if (hitinfo.collider.tag == "WorldChunk")
        //    {
        //        Pos.y = hitinfo.collider.gameObject.transform.position.y;
        //    }

        //}
        this.Pos = Pos;
    }

    public bool ConnectNode (List<NavMeshNode> nodes)
    {
        foreach (NavMeshNode node in nodes)
        {
            if (node.Pos.x <= Pos.x + 1 && node.Pos.x >= Pos.x -1 && node.Pos.z <= Pos.z + 1 && node.Pos.z >= Pos.z - 1)
            {
                //from a 2D sky POV the could be a connection

                if (node.Pos.y <= Pos.y + 25 && node.Pos.y >= Pos.y - 25)
                {
                    //with in max hight limit
                    AddNodeConnection(node);
                }
            }
        }
        return true;
    }
    private void AddNodeConnection(NavMeshNode OtherNode)
    {
        ConnectedNodes[IdentifyNodeDirection(this, OtherNode)] = new NavMeshNodeConnections(this, OtherNode);
        OtherNode.ConnectedNodes[IdentifyNodeDirection(OtherNode, this)] = new NavMeshNodeConnections(OtherNode , this);
    }
    static private int IdentifyNodeDirection(NavMeshNode Node, NavMeshNode OtherNode)
    {
        if (OtherNode.Pos.z > Node.Pos.z && OtherNode.Pos.x == Node.Pos.x) //north
        {
            return 2;
        }
        if (OtherNode.Pos.x > Node.Pos.x && OtherNode.Pos.z == Node.Pos.z) //east
        {
            return 4;
        }
        if (OtherNode.Pos.z < Node.Pos.z && OtherNode.Pos.x == Node.Pos.x) //soth
        {
            return 6;
        }
        if (OtherNode.Pos.x < Node.Pos.x && OtherNode.Pos.z == Node.Pos.z) //west
        {
            return 8;
        }
        if (OtherNode.Pos.z > Node.Pos.z && OtherNode.Pos.x < Node.Pos.x) //NW
        {
            return 1;
        }
        if (OtherNode.Pos.z > Node.Pos.z && OtherNode.Pos.x > Node.Pos.x) //NE
        {
            return 3;
        }
        if (OtherNode.Pos.z < Node.Pos.z && OtherNode.Pos.x == Node.Pos.x) //SE
        {
            return 5;
        }
        if (OtherNode.Pos.z < Node.Pos.z && OtherNode.Pos.x < Node.Pos.x) //SW
        {
            return 7;
        }
        return 0;
    }

}
public class NavMeshNodeConnections
{
    public NavMeshNode StartingNavMeshNode { get; set; }
    public NavMeshNode EndingNavMeshNode { get; set; }
    public Vector3 TravelVector { get; set; }
    public int TravelCost { get; set; }

    public NavMeshNodeConnections()
    {
        TravelVector = new Vector3();
        TravelCost = 1;
    }
    public NavMeshNodeConnections(NavMeshNode StartingNavMeshNode, NavMeshNode EndingNavMeshNode) : this() 
    {
        this.StartingNavMeshNode = StartingNavMeshNode;
        this.EndingNavMeshNode = EndingNavMeshNode;

    }
    public Vector4 CalculateTravelVector()
    {
        TravelVector = new Vector3(StartingNavMeshNode.Pos.x - EndingNavMeshNode.Pos.x, StartingNavMeshNode.Pos.y - EndingNavMeshNode.Pos.y, StartingNavMeshNode.Pos.z - EndingNavMeshNode.Pos.z);
        return TravelVector;
        //return new Vector3();
    }
    public int CalculateTravelCost()   
    {
        TravelCost = (int)Vector3.Distance(StartingNavMeshNode.Pos, EndingNavMeshNode.Pos) * StartingNavMeshNode.TerrainModifyer;

        return TravelCost;
        //return 1;
    }



}
