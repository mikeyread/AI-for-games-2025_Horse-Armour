using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class NavMesh_Script : MonoBehaviour
{
    [SerializeField]
    public QuadtreeWorldGenerator quadtreeWorldGenerator;

    [SerializeField]
    float TerrainModifyer;

    NavMeshSettings NavMeshsettings = new NavMeshSettings();

    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    // Start is called before the first frame update

    public Vector3 TLCorner { get; set; }
    //public Vector3 TRCorner { get; set; }
    //public Vector3 BLCorner { get; set; }
    public Vector3 BRCorner { get; set; }

    public int Direction { get; set; }

    [SerializeField]
    public int MaxTravelHight;

    public  List<AStarNode> CurrentShortestPath = new List<AStarNode>();
    public List<AStarNode> NodesLookedAt = new List<AStarNode>();
    private float time = 0f;
    private bool done = false;
    private float time2 = 0f;
    private bool done2 = false;


    //Testing 
    Vector3 PathStart = Vector3.zero;
    Vector3 PathEnd = Vector3.zero;

    void Start()
    {
        NavMeshsettings.MaxTravelHight = MaxTravelHight;
        NavMeshsettings.TerrainModifyer = TerrainModifyer;


        //.Add(new NavMeshNode(transform.position, nodes.Count + 1));
        //nodes.Sort();
        //nodes = nodes.OrderBy(NavMeshNode => NavMeshNode.Pos).ToList();
        //TLCorner = nodes.LastOrDefault().Pos;
        //BRCorner = nodes.LastOrDefault().Pos;

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
        nodes.Add(new NavMeshNode((nodes.LastOrDefault().Pos) + new Vector3(x, y, z), nodes.Count));
        return true;
    }


    // Update is called once per frame
    void Update()
    {
        //SpawnNodesSpiral(10);
        //nodes
        //nodes.Sort();
        //nodes = nodes.OrderBy(NavMeshNode => NavMeshNode.Pos.magnitude).ToList();
        if (time <= 1)
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
                //SpawnNodesSetSqure(new Vector3(0, 0, 0), 100);


            }
        }

        if (time2 <= 6)
        {
            time2 += Time.deltaTime;
        }
        else
        {
            if (!done2)
            {
                done2 = true;

                Debug.Log("Navigate Tree Done");
                //FindPath(new Vector3(5, 0, 2), new Vector3(-12, 0, -2));

            }
        }

        //Debug.Log("test");
        //PlayerOderUnit();
    }

    private void FixedUpdate()
    {
        //Debug.Log("test");
        //PlayerOderUnit();
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

            Debug.Log("Start path at " + PathStart);
            Debug.Log("end path at " + PathEnd);
            CurrentShortestPath = new List<AStarNode>();
            FindPath(PathStart, PathEnd);
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

            //Debug.Log("for each thing");
            if (Grid.IsLeaf())
            {
                //Debug.Log("LEAF");


                //DEAL WITH THIS
                if (Grid.n_depth <= 5 /*quadtreeWorldGenerator.WorldSettings.Quadtree_maxDepth*/)
                {
                    
                    SpawnNodesSqure(Grid.g_Position, (int)(Grid.chunk.c_MeshScale * Grid.chunk.c_MeshQuantity), Grid.chunk);

                    Debug.Log("Grid: " + Grid.g_Position);
                    //Debug.Log("Chunk: " + Grid.chunk.c_MeshScale);
                    
                }
            }

        }
    }

    private void SpawnNodesSqure(Vector3 StartingTLVector,int edgeLength, QuadTreeChunk chunk)
    {
        for (float z = StartingTLVector.z - (edgeLength / 2); z < (StartingTLVector.z + edgeLength - (edgeLength / 2)); z++)  //(float z = StartingTLVector.z + (edgeLength / 2); z > (StartingTLVector.z - (edgeLength + (edgeLength / 2))); z--) 
        {
            for (float x = StartingTLVector.x - (edgeLength / 2); x < (StartingTLVector.x + edgeLength - (edgeLength / 2)); x++)
            {
                //Debug.Log("Grid: " + StartingTLVector);
                //Debug.Log(edgeLength);
                //Debug.Log(x);
                //Debug.Log(z);
                float temp1 = (StartingTLVector.x - (edgeLength / 2));
                float temp2 = (StartingTLVector.z - (edgeLength / 2));
                //Debug.Log(temp1);
                //Debug.Log(temp2);
                //Vector3 localoffset = new Vector3 ((chunk.Chunk.transform.position.x - (edgeLength / 2)) - x,0 ,(chunk.Chunk.transform.position.z - (edgeLength / 2)) - z);
                Vector3 localoffset = new Vector3(x - temp1, 0, z - temp2);
                //Debug.Log(localoffset.x);
                //Debug.Log(localoffset.z);
                //Debug.Log(chunk.c_MeshQuantity);
                int vertexIndex = ((int)(localoffset.x) + 1) + ((int)(localoffset.z) + 1) * (chunk.c_MeshQuantity + 2);
                //Debug.Log(vertexIndex);
                float vertexY = chunk.chunkMesh.vertices[vertexIndex].y;
                //Debug.DrawLine(new Vector3(x, vertexY + 500f, z), new Vector3(x, -1000, z), Color.red, float.MaxValue);
                if (Physics.Raycast(new Ray(new Vector3(x, vertexY + 500f, z), new Vector3(0, -1000, 0)), out RaycastHit Hit))
                {
                    //Debug.Log("Hit");
                    //Debug.Log("Hit" + Hit.collider.name);
                    if (Hit.collider.name == "TreePrefab(Clone)")
                    {
                        Debug.Log("Hit tree");
                    }
                    else
                    {
                        nodes.Add(new NavMeshNode(new Vector3(x, vertexY /*transform.position.y*/, z), nodes.Count));
                    }
                }
                else
                {
                    nodes.Add(new NavMeshNode(new Vector3(x, vertexY /*transform.position.y*/, z), nodes.Count));
                }


                nodes.Last().ConnectNode(nodes);
            }

        }
    }

    private void SpawnNodesSetSqure(Vector3 StartingTLVector, int edgeLength)
    {
        for (float z = StartingTLVector.z - (edgeLength / 2); z < (StartingTLVector.z + edgeLength - (edgeLength / 2)); z++)  //(float z = StartingTLVector.z + (edgeLength / 2); z > (StartingTLVector.z - (edgeLength + (edgeLength / 2))); z--) 
        {
            for (float x = StartingTLVector.x - (edgeLength / 2); x < (StartingTLVector.x + edgeLength - (edgeLength / 2)); x++)
            {
                //Debug.Log("Grid: " + StartingTLVector);
                //Debug.Log(edgeLength);
                //Debug.Log(x);
                //Debug.Log(z);
                float temp1 = (StartingTLVector.x - (edgeLength / 2));
                float temp2 = (StartingTLVector.z - (edgeLength / 2));
                //Debug.Log(temp1);
                //Debug.Log(temp2);
                //Vector3 localoffset = new Vector3 ((chunk.Chunk.transform.position.x - (edgeLength / 2)) - x,0 ,(chunk.Chunk.transform.position.z - (edgeLength / 2)) - z);
                Vector3 localoffset = new Vector3(x - temp1, 0, z - temp2);
                //Debug.Log(localoffset.x);
                //Debug.Log(localoffset.z);
                //Debug.Log(chunk.c_MeshQuantity);
                //int vertexIndex = ((int)(localoffset.x) + 1) + ((int)(localoffset.z) + 1) * (chunk.c_MeshQuantity + 2);
                //Debug.Log(vertexIndex);
                //float vertexY = chunk.chunkMesh.vertices[vertexIndex].y;

                if (Physics.Raycast(new Ray(new Vector3(x, transform.position.y + 1f, z), new Vector3(0, 1000, 0)), out RaycastHit Hit))
                {
                    //Debug.Log("Hit");
                    if (Hit.collider.name == "Wall")
                    {
                        //Debug.Log("Hit Wall");
                    }
                    else
                    {
                        nodes.Add(new NavMeshNode(new Vector3(x,  transform.position.y, z), nodes.Count));
                    }
                }
                else
                {
                    nodes.Add(new NavMeshNode(new Vector3(x, transform.position.y, z), nodes.Count));
                }

                //nodes.Add(new NavMeshNode(new Vector3(x, 0 /*transform.position.y*/, z), nodes.Count));
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
                //Gizmos.color = Color.blue;
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
        if (CurrentShortestPath == null) return;
        List<AStarNode> ShowenCurrentShortestPath = CurrentShortestPath;
        foreach (AStarNode aStarNode in ShowenCurrentShortestPath)
        {
            //Debug.Log(TCurrentShortestPathTest.Count());
            Gizmos.color = Color.blue;
            Gizmos.DrawCube( new Vector3 (Tnodes[aStarNode.NavMeshNodeIndex].Pos.x, Tnodes[aStarNode.NavMeshNodeIndex].Pos.y +0.2f, Tnodes[aStarNode.NavMeshNodeIndex].Pos.z), Vector3.one);
            //Debug.Log("drawing");
        }

        if (NodesLookedAt == null) return;
        List<AStarNode> TCurrentShortestPathVisetedTest = NodesLookedAt;
        foreach (AStarNode aStarNode in TCurrentShortestPathVisetedTest)
        {
            //Debug.Log(TCurrentShortestPathTest.Count());
            Gizmos.color = Color.red;
            Gizmos.DrawCube(Tnodes[aStarNode.NavMeshNodeIndex].Pos, Vector3.one);
            //Debug.Log("drawing");
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

    public List<Vector3> FindPath( Vector3 StartPoint, Vector3 EndPoint)
    {
        Debug.Log("starting to look for path");
        AStarNode NodeWithCurrentShortestPath;
        List<AStarNode> AStarNodes = new List<AStarNode>(); // all nodes peeked at
        List<AStarNode> VistedAStarNode = new List<AStarNode>(); //AllVisteted nodes
        List<AStarNode> ToVistAStarNode = new List<AStarNode>(); // openSet
        NavMeshNode StartingNode = null;
        NavMeshNode EndNode = null;
        DateTime time = DateTime.Now;

        AStarNode nextNode = null;
        //find starting node
        //Debug.Log("Now trying node at " + node.Pos + " With index " + node.NavMeshNodeIndex);
        foreach (NavMeshNode node in nodes)
        {
            //if ((node.Pos - StartPoint).magnitude < 1)
            //{
            //    StartingNode = node;
            //    Debug.Log("Start node " +StartingNode.Pos);
            //}
            //if ((node.Pos - EndPoint).magnitude < 1)
            //{
            //    EndNode = node;
            //    Debug.Log("End node " + EndNode.Pos);
            //}
            //if (EndNode != null && StartingNode != null)
            //{
            //    Debug.Log("escaped early");
            //    break;
            //}
            //Debug.Log("Now trying node at " + node.Pos + " With index " + node.NavMeshNodeIndex);
            if ((node.Pos - StartPoint).x < 1 && (node.Pos - StartPoint).x > -1 && (node.Pos - StartPoint).z < 1 && (node.Pos - StartPoint).z > -1)
            {
                StartingNode = node;
                Debug.Log("Start node " + StartingNode.Pos);

            }
            if ((node.Pos - EndPoint).x < 1 && (node.Pos - EndPoint).x > -1 && (node.Pos - EndPoint).z < 1 && (node.Pos - EndPoint).z > -1)
            {
                EndNode = node;
                Debug.Log("End node " + EndNode.Pos);
            }
            if (EndNode != null && StartingNode != null)
            {
                Debug.Log("escaped early");
                break;
            }


        }
        NodeWithCurrentShortestPath = new AStarNode(FindAbsoluteDistance(StartPoint, EndPoint), 0, null, StartingNode.NavMeshNodeIndex);
        Debug.Log("Cost of first node " + FindAbsoluteDistance(StartPoint, EndPoint).magnitude);

        Debug.Log("Starting node is at  " + nodes[StartingNode.NavMeshNodeIndex].Pos);

        VistedAStarNode.Add(NodeWithCurrentShortestPath);

        AStarNode temp = NodeWithCurrentShortestPath;

        while (NodeWithCurrentShortestPath.NavMeshNodeIndex != EndNode.NavMeshNodeIndex && DateTime.Now < time.AddSeconds(8))
        {
            NodesLookedAt = AStarNodes;

            


            //foreach (AStarNode aStarNode in VistedAStarNode)
            //{


            //Debug.Log("!!!!!!!!!!!! Current node cost " + NodeWithCurrentShortestPath.CoastofUsingThisNode + " for node at " + nodes[NodeWithCurrentShortestPath.NavMeshNodeIndex].Pos);

            foreach (NavMeshNodeConnections Connection in nodes[NodeWithCurrentShortestPath.NavMeshNodeIndex].ConnectedNodes)
                {
                    //Debug.Log(Connection);
                if (Connection != null && Connection.StartingNavMeshNode.Pos != Connection.EndingNavMeshNode.Pos)
                {
                    bool nodeAlreadyMapped = false;
                    foreach (AStarNode starNode in AStarNodes)
                    {
                        if (nodes[starNode.NavMeshNodeIndex].Pos == Connection.EndingNavMeshNode.Pos)
                        {
                            nodeAlreadyMapped = true;
                            if (starNode.CoastFromStartPoint >= NodeWithCurrentShortestPath.CoastFromStartPoint + Connection.TravelCost)
                            {
                                starNode.CoastFromStartPoint = NodeWithCurrentShortestPath.CoastFromStartPoint + Connection.TravelCost;
                                starNode.PreviousNode = NodeWithCurrentShortestPath;
                            }
                            if (!VistedAStarNode.Contains(starNode))
                            {
                                ToVistAStarNode.Add(starNode);
                            }
                        }
                    }
                    if (!nodeAlreadyMapped)
                    {
                        nextNode = new AStarNode(FindAbsoluteDistance(Connection.EndingNavMeshNode.Pos, EndNode.Pos), NodeWithCurrentShortestPath.CoastFromStartPoint + Connection.TravelCost, NodeWithCurrentShortestPath, Connection.EndingNavMeshNode.NavMeshNodeIndex);
                        AStarNodes.Add(nextNode);
                        Debug.Log("Cost of using node is " + nextNode.CoastofUsingThisNode + " which is " + FindAbsoluteDistance(Connection.EndingNavMeshNode.Pos, EndNode.Pos).magnitude + " + " + NodeWithCurrentShortestPath.CoastFromStartPoint + Connection.TravelCost + " for node at " + nodes[nextNode.NavMeshNodeIndex].Pos);

                        ToVistAStarNode.Add(nextNode);
                        Debug.Log("Node added to open set");
                    }

                    //if (nextNode.CoastofUsingThisNode < NodeWithCurrentShortestPath.CoastofUsingThisNode)
                    {
                        //ToVistAStarNode.Add(nextNode);
                        //Debug.Log("Node added to open set");
                    }
                }

            }
            //}
            temp = new AStarNode();

            foreach (AStarNode starNode in ToVistAStarNode)
            {
                if (starNode.CoastofUsingThisNode <= temp.CoastofUsingThisNode && !VistedAStarNode.Contains(starNode))
                {
                    temp = starNode;
                    //VistedAStarNode.Add(temp);
                    //Debug.Log("added node to Visted list " + nodes[temp.NavMeshNodeIndex].Pos + " index " + temp.NavMeshNodeIndex);
                }

            }
            if (temp.CoastofUsingThisNode >= float.MaxValue /*NodeWithCurrentShortestPath.CoastofUsingThisNode*/ && VistedAStarNode.Count > 1)
            {

                temp = VistedAStarNode[VistedAStarNode.IndexOf(NodeWithCurrentShortestPath) - 1];
                Debug.Log("retreating to prior node, moving to node at visted index  " + VistedAStarNode.IndexOf(temp));
                CurrentShortestPath.Remove(temp);
                //Debug.Log("moving bak to " + nodes[temp.NavMeshNodeIndex].Pos);
            }
            VistedAStarNode.Add(temp);
            Debug.Log("added node to Visted list " + nodes[temp.NavMeshNodeIndex].Pos + " index " + temp.NavMeshNodeIndex);
            NodeWithCurrentShortestPath = temp;
            ToVistAStarNode.Clear();

            Debug.Log("NodeWithCurrentShortestPath  " + NodeWithCurrentShortestPath.NavMeshNodeIndex);
            if (!CurrentShortestPath.Contains(NodeWithCurrentShortestPath)) { CurrentShortestPath.Add(NodeWithCurrentShortestPath); }
        }



        List<Vector3> Path = new List<Vector3>();
 
        Debug.Log("Start node " + StartingNode.Pos);
        Debug.Log("End node " + EndNode.Pos);
        Debug.Log("End node " + EndNode.Pos);
        foreach (AStarNode thing in CurrentShortestPath)
        {
            Debug.Log(nodes[thing.NavMeshNodeIndex].Pos + " " + thing.CoastofUsingThisNode);

        }
        Path.Add(nodes[NodeWithCurrentShortestPath.NavMeshNodeIndex].Pos);
        CurrentShortestPath.Clear();
        CurrentShortestPath.Add(NodeWithCurrentShortestPath);
        AStarNode NextNodeToCheck = NodeWithCurrentShortestPath.PreviousNode;
        while (Path.Last() != StartingNode.Pos)
        {
            Debug.Log(Path.Last());
            CurrentShortestPath.Add(NextNodeToCheck);
            Path.Add(nodes[NextNodeToCheck.NavMeshNodeIndex].Pos);
            NextNodeToCheck = NextNodeToCheck.PreviousNode;
        }
        return Path;

        return new List<Vector3>();
    }

    private Vector3 FindAbsoluteDistance(Vector3 Start, Vector3 End)
    {
        return (Start - End);
    }
}

public class AStarAlg
{

}

public class AStarNode
{
    public int NavMeshNodeIndex;
    public Vector3 DistanceToEndPoint;
    public float CoastFromStartPoint;
    public float CoastofUsingThisNode = float.MaxValue;
    public AStarNode PreviousNode;
    public AStarNode (Vector3 DistanceToEndPoint, float CoastFromStartPoint, AStarNode previousNode, int NavMeshNodeIndex)
    {

        this.DistanceToEndPoint = DistanceToEndPoint;
        this.CoastFromStartPoint = CoastFromStartPoint;
        if (previousNode == null)
        {
            CoastofUsingThisNode = float.MaxValue;
        }
        else
        {
            CoastofUsingThisNode = DistanceToEndPoint.magnitude + CoastFromStartPoint;
        }

        this.PreviousNode = previousNode;
        this.NavMeshNodeIndex = NavMeshNodeIndex;
    }
    public AStarNode()
    {

    }
    //public 
}
public class NavMeshNode
{
    public int NavMeshNodeIndex;
    public Vector3 Pos { get; set; }

    public float TerrainModifyer { get; set; }

    public NavMeshNodeConnections[] ConnectedNodes = new NavMeshNodeConnections[9];

    //public NavMeshNode[] ConnectedNodes = new NavMeshNode[9];
    public NavMeshNode()
    {
        TerrainModifyer = 0.2f;
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
    public NavMeshNode(Vector3 Pos, int NavMeshNodeIndex) : this()
    {
        this.NavMeshNodeIndex = NavMeshNodeIndex;
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
        this.ConnectedNodes[IdentifyNodeDirection(this, OtherNode)] = new NavMeshNodeConnections(this, OtherNode);
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
        if (OtherNode.Pos.z < Node.Pos.z && OtherNode.Pos.x > Node.Pos.x) //SE
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
public class NavMeshSettings
{
    public float TerrainModifyer { get; set; } = 0.2f;
    public int MaxTravelHight { get; set; }
}
public class NavMeshNodeConnections
{
    public NavMeshNode StartingNavMeshNode { get; set; }
    public NavMeshNode EndingNavMeshNode { get; set; }
    public Vector3 TravelVector { get; set; }
    public float TravelCost { get; set; }

    public NavMeshNodeConnections()
    {
        TravelVector = new Vector3();
        TravelCost = 0.2f;
    }
    public NavMeshNodeConnections(NavMeshNode StartingNavMeshNode, NavMeshNode EndingNavMeshNode) : this() 
    {
        this.StartingNavMeshNode = StartingNavMeshNode;
        this.EndingNavMeshNode = EndingNavMeshNode;
        TravelCost = CalculateTravelCost();
        TravelVector = CalculateTravelVector();
    }
    public Vector4 CalculateTravelVector()
    {
        TravelVector = new Vector3(StartingNavMeshNode.Pos.x - EndingNavMeshNode.Pos.x, StartingNavMeshNode.Pos.y - EndingNavMeshNode.Pos.y, StartingNavMeshNode.Pos.z - EndingNavMeshNode.Pos.z);
        return TravelVector;
        //return new Vector3();
    }
    public float CalculateTravelCost()   
    {
        TravelCost = Vector3.Distance(StartingNavMeshNode.Pos, EndingNavMeshNode.Pos) * StartingNavMeshNode.TerrainModifyer;

        return TravelCost;
        //return 1;
    }



}
