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
        //Debug.Log("Active quantity of grids: " + q_tree.GetActive().Count);
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
    public ComputeShader _PostProcessComputeShader;

    public ComputeBuffer b_HashTable;
    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Normals;
    public ComputeBuffer b_Indices;
    public ComputeBuffer b_Color;

    private float c_MeshScale;
    private int c_MeshQuantity = WorldOptions.CHUNK_QUAD_AMOUNT;
    private int m_IndexLimit;

    private Texture2D m_Texture;
    private Vector3[] m_Vertices;
    private Color[] m_Color;
    private int[] m_Indices;

    public QuadTreeChunk(Quad parent)
    {
        // Dispose of any buffers if they happen to have present allocations.
        b_HashTable?.Dispose();

        b_Vertices?.Dispose();
        b_Normals?.Dispose();
        b_Indices?.Dispose();
        b_Color?.Dispose();

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
        _PostProcessComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_ChunkPostVertex"));
        
        // Initialises Mesh Data.
        m_Vertices = new Vector3[m_IndexLimit];
        m_Color = new Color[m_IndexLimit];
        m_Indices = new int[m_IndexLimit * 6];

        // Chunk Object Mesh component initialisation.
        if (chunkObject.GetComponent<MeshFilter>() == null) chunkObject.AddComponent<MeshFilter>();

        if (chunkObject.GetComponent<MeshRenderer>() == null) {
            chunkObject.AddComponent<MeshRenderer>();
            chunkObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
            //chunkObject.GetComponent<MeshRenderer>().sharedMaterial = new(Shader.Find("Particles/Standard Unlit"));
        }

        m_Texture = new(c_MeshQuantity + 2, c_MeshQuantity + 2);

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Normals = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);
        b_Color = new ComputeBuffer(m_IndexLimit, sizeof(float) * 4);

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

        // Calculates Normals and assigns it to the buffer
        chunkMesh.RecalculateNormals();
        b_Normals.GetData(chunkMesh.normals);

        // Create the Skirts and apply Colour to the mesh via 
        _PostProcessComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _PostProcessComputeShader.SetBuffer(0, "normals", b_Normals);
        _PostProcessComputeShader.SetBuffer(0, "color", b_Color);

        _PostProcessComputeShader.SetInt("meshSize", c_MeshQuantity + 2);
        _PostProcessComputeShader.SetFloat("quadScale", c_MeshScale);

        _PostProcessComputeShader.Dispatch(0, m_IndexLimit / 32, m_IndexLimit / 32, 1);

        b_Vertices.GetData(m_Vertices);
        b_Color.GetData(m_Color);

        for (int y = 0; y < c_MeshQuantity + 2; y++)
        {
            for (int x = 0; x < c_MeshQuantity + 2; x++)
            {
                m_Texture.SetPixel(x, y, m_Color[x + y * (c_MeshQuantity + 2)]);
            }
        }

        chunkMesh.vertices = m_Vertices;
        chunkMesh.colors = m_Color;

        m_Texture.Apply();

        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Normals?.Dispose();
        b_Indices?.Dispose();
        b_Color?.Dispose();

        chunkObject.GetComponent<MeshFilter>().sharedMesh = chunkMesh;
        chunkObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = null;
        chunkObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = m_Texture;
    }
}
