using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadtreeWorldGenerator : MonoBehaviour
{
    private QuadTree q_tree;
    private bool debugGrid = false;

    private void Awake()
    {
        // Quad Tree is generated from the bottom up by specifying the unit scale of the highest detail chunk.
        q_tree = new((WorldOptions.CHUNK_QUAD_AMOUNT * WorldOptions.CHUNK_QUAD_SCALAR) * Mathf.Pow(2, QuadTreeParameters.maxDepth), transform.position);
    }

    private void Update()
    {
        q_tree.UpdateGrid(transform.position);
        Debug.Log("Active quantity of grids: " + q_tree.GetActive().Count);
    }


    // Wireframe Mode for QuadMesh
    private void OnDrawGizmos()
    {
        if (q_tree == null) return;
        if (!debugGrid) return;

        foreach (var Grid in q_tree.GetActive())
        {
            Gizmos.DrawWireCube(Grid.g_Position, Grid.n_Bounds);
        }
    }
}


// Testing out Quadtree Chunks
public class QuadTreeChunk {
    private Quad parentGrid;

    public GameObject chunkObject;
    public Mesh chunkMesh;

    public ComputeShader _ComputeShader;
    public ComputeShader _NormalComputeShader;

    public ComputeBuffer b_HashTable;
    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Indices;
    public ComputeBuffer b_Normals;

    private float c_MeshScale;
    private int c_MeshQuantity = WorldOptions.CHUNK_QUAD_AMOUNT;
    private int m_IndexLimit;

    private Vector3[] m_Vertices;
    private Vector3[] m_Normals;
    private int[] m_Indices;

    public QuadTreeChunk(Quad parent)
    {
        // Dispose of any buffers if they happen to have present allocations.
        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Normals?.Dispose();
        b_Indices?.Dispose();

        parentGrid = parent;

        m_IndexLimit = (c_MeshQuantity + 2) * (c_MeshQuantity + 2);

        // Scalars for the Chunk LOD and Mesh Scale
        float ParentScale = parentGrid.TRCorner_Position().x - parentGrid.BLCorner_Position().x;
        c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR * (ParentScale / ((WorldOptions.CHUNK_QUAD_AMOUNT - 1) * WorldOptions.CHUNK_QUAD_SCALAR));

        // The chunk object initilisation
        chunkObject = new("Chunk");
        chunkMesh = new();

        // Compute Shader Initialisation
        _ComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_ChunkQuadtree"));
        _NormalComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_ChunkPostVertex"));
        
        // Initialises Mesh Data.
        m_Vertices = new Vector3[m_IndexLimit];
        m_Normals = new Vector3[m_IndexLimit];
        m_Indices = new int[m_IndexLimit * 6];

        // Chunk Object Mesh component initialisation.
        if (chunkObject.GetComponent<MeshFilter>() == null) chunkObject.AddComponent<MeshFilter>();
        if (chunkObject.GetComponent<MeshRenderer>() == null) {
            chunkObject.AddComponent<MeshRenderer>();
            chunkObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        }

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);

        b_HashTable = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));
        b_HashTable.SetData(PerlinNoise2D._Noise2D);
        _ComputeShader.SetBuffer(0, "hash", b_HashTable);

        _ComputeShader.SetInt("meshSize", c_MeshQuantity + 2);
        _ComputeShader.SetFloat("quadScale", c_MeshScale);

        _ComputeShader.SetFloat("PI", Mathf.PI);
        _ComputeShader.SetVector("globalPosition", parentGrid.g_Position);

        _ComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _ComputeShader.SetBuffer(0, "indices", b_Indices);

        _ComputeShader.Dispatch(0, m_IndexLimit / 32, m_IndexLimit / 32, 1);

        b_Vertices.GetData(m_Vertices);
        b_Indices.GetData(m_Indices);

        chunkMesh.vertices = m_Vertices;
        chunkMesh.triangles = m_Indices;


        // Compute the Normals
        b_Normals = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);

        _NormalComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _NormalComputeShader.SetBuffer(0, "normals", b_Normals);

        _NormalComputeShader.SetInt("meshSize", c_MeshQuantity + 2);
        _NormalComputeShader.SetFloat("quadScale", c_MeshScale);

        _NormalComputeShader.Dispatch(0, m_IndexLimit / 32, m_IndexLimit / 32, 1);

        b_Vertices.GetData(m_Vertices);
        b_Normals.GetData(m_Normals);

        chunkMesh.vertices = m_Vertices;
        chunkMesh.normals = m_Normals;
        //chunkMesh.vertices = m_Vertices;


        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Indices?.Dispose();
        b_Normals?.Dispose();

        chunkObject.GetComponent<MeshFilter>().sharedMesh = chunkMesh;
    }


    // Generates a One by One Quadmesh to scale of its parent Quadtree Node
    public void GenerateDebugQuadMesh()
    {
        m_Vertices = new Vector3[4];
        m_Indices = new int[6];

        Vector3 relativePosition = parentGrid.g_Position;

        m_Vertices[0] = relativePosition + new Vector3(-parentGrid.n_Bounds.x / 2, 0, -parentGrid.n_Bounds.z / 2);
        m_Vertices[1] = relativePosition + new Vector3(parentGrid.n_Bounds.x / 2, 0, -parentGrid.n_Bounds.z / 2);
        m_Vertices[2] = relativePosition + new Vector3(-parentGrid.n_Bounds.x / 2, 0, parentGrid.n_Bounds.z / 2);
        m_Vertices[3] = relativePosition + new Vector3(parentGrid.n_Bounds.x / 2, 0, parentGrid.n_Bounds.z / 2);

        m_Indices[0] = 1;
        m_Indices[1] = 0;
        m_Indices[2] = 2;

        m_Indices[3] = 2;
        m_Indices[4] = 3;
        m_Indices[5] = 1;

        chunkMesh.vertices = m_Vertices;
        chunkMesh.triangles = m_Indices;

        if (chunkObject.GetComponent<MeshFilter>() == null) chunkObject.AddComponent<MeshFilter>().sharedMesh = chunkMesh;
        if (chunkObject.GetComponent<MeshRenderer>() == null) chunkObject.AddComponent<MeshRenderer>().sharedMaterial = new(Shader.Find("Universal Render Pipeline/Simple Lit"));
    }
}
