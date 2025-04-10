using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;



// Settings for World Generation.
[System.Serializable]
public struct WorldGenerationSettings {
    // Perlin Noise Seed
    [Header("World Settings")]
    public uint World_Seed;

    // Quadtree Settings
    [Header("Quadtree Settings")]
    public uint Quadtree_maxDepth;
    public float Quadtree_GridSphericalCheck;

    // Chunk Settings
    [Header("Chunk Settings")]
    public int Chunk_QuadQuantity;
    public float Chunk_QuadScale;
    public float objDensity;
    public GameObject prefab;
}


public class QuadtreeWorldGenerator : MonoBehaviour {

    // Settings for the World
    public WorldGenerationSettings WorldSettings;

    private QuadTree q_tree;

    private void Awake() {
        if (WorldSettings.World_Seed != 0) Random.InitState((int)WorldSettings.World_Seed);

        Vector3 flattenedPosition = new(transform.position.x, 0, transform.position.z);

        // Quad Tree is generated from the bottom up by specifying the unit scale of the highest detail chunk.
        q_tree = new((WorldSettings.Chunk_QuadQuantity * WorldSettings.Chunk_QuadScale) * Mathf.Pow(2, WorldSettings.Quadtree_maxDepth), flattenedPosition, WorldSettings);
    }

    private void Update()
    {
        q_tree.UpdateGrid(transform.position);
    }


    // Wireframe Mode for QuadMesh
    private void OnDrawGizmos() {
        if (q_tree == null) return;

        foreach (var Grid in q_tree.GetActive()) {

        }
    }
}


// A chunk within the Quadtree
public class QuadTreeChunk {
    private Quad parentGrid;

    private WorldGenerationSettings settings;

    public GameObject Chunk;
    public Mesh chunkMesh;

    public ComputeShader _ComputeShader;
    public ComputeShader _PostProcessComputeShader;

    public ComputeBuffer b_HashTable;
    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Normals;
    public ComputeBuffer b_Indices;
    public ComputeBuffer b_UV;
    public ComputeBuffer b_Color;

    private float c_MeshScale;
    private int c_MeshQuantity;
    private int m_IndexLimit;

    private Texture2D m_Texture;
    private Vector3[] m_Vertices;
    private Vector2[] m_UV;
    private Color[] m_Color;
    private int[] m_Indices;

    public List<GameObject> chunkObjects;



    public QuadTreeChunk(Quad parent) {
        // Dispose of any buffers if they happen to have present allocations.
        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Indices?.Dispose();
        b_UV?.Dispose();
        b_Normals?.Dispose();
        b_Color?.Dispose();

        parentGrid = parent;
        settings = parent.settings;

        c_MeshQuantity = settings.Chunk_QuadQuantity;
        m_IndexLimit = (c_MeshQuantity + 2) * (c_MeshQuantity + 2);

        // Scalars for the Chunk LOD and Mesh Scale
        float ParentScale = parentGrid.TRCorner_Position().x - parentGrid.BLCorner_Position().x;
        c_MeshScale = settings.Chunk_QuadScale * (ParentScale / ((settings.Chunk_QuadQuantity - 1) * settings.Chunk_QuadScale));

        // The chunk object initilisation
        Chunk = new("Chunk");
        chunkMesh = new();
        chunkMesh.name = "Chunk_Mesh";
        Chunk.tag = "WorldChunk";

        // Compute Shader Initialisation
        _ComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_ChunkQuadtree"));
        _PostProcessComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_ChunkPostVertex"));
        
        // Initialises Mesh relevant Data
        m_Vertices = new Vector3[m_IndexLimit];
        m_Color = new Color[m_IndexLimit];
        m_UV = new Vector2[m_IndexLimit];
        m_Indices = new int[m_IndexLimit * 6];


        // Attaches Components necessary for Mesh construction.
        if (Chunk.GetComponent<MeshFilter>() == null) Chunk.AddComponent<MeshFilter>();

        if (Chunk.GetComponent<MeshRenderer>() == null) {
            Chunk.AddComponent<MeshRenderer>();
            Chunk.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        }

        m_Texture = new(c_MeshQuantity + 2, c_MeshQuantity + 2);

        GenerateMesh();
    }

    private void GenerateMesh()
    {
        // Create all the Buffers
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Normals = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);

        b_Color = new ComputeBuffer(m_IndexLimit, sizeof(float) * 4);
        b_UV = new ComputeBuffer(m_IndexLimit, sizeof(float) * 2);

        // Links the noise Hash Table to the buffer.
        b_HashTable = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));
        b_HashTable.SetData(PerlinNoise2D._Noise2D);
        _ComputeShader.SetBuffer(0, "hash", b_HashTable);

        // Compute Shader Constants
        _ComputeShader.SetInt("meshSize", c_MeshQuantity + 2);
        _ComputeShader.SetFloat("quadScale", c_MeshScale);
        _ComputeShader.SetFloat("PI", Mathf.PI);
        _ComputeShader.SetVector("globalPosition", parentGrid.g_Position);

        // parsing in the Compute Shader Buffers.
        _ComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _ComputeShader.SetBuffer(0, "indices", b_Indices);
        _ComputeShader.SetBuffer(0, "uvs", b_UV);

        _ComputeShader.Dispatch(0, m_IndexLimit / 32, m_IndexLimit / 32, 1);

        // Grab computed Buffer Data
        b_Vertices.GetData(m_Vertices);
        b_Indices.GetData(m_Indices);
        b_UV.GetData(m_UV);

        // Calculates Normals and assigns it to the buffer
        chunkMesh.vertices = m_Vertices;
        chunkMesh.triangles = m_Indices;

        chunkMesh.RecalculateNormals();
        b_Normals.SetData(chunkMesh.normals);

        // Creates Vertex Skirts and Colour data using the vertex height offset, with an otherwise similar setup to the above Compute Shader.
        _PostProcessComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _PostProcessComputeShader.SetBuffer(0, "normals", b_Normals);
        _PostProcessComputeShader.SetBuffer(0, "color", b_Color);

        // Hash Table for randomized color noise
        _PostProcessComputeShader.SetBuffer(0, "hash", b_HashTable);
        _ComputeShader.SetFloat("PI", Mathf.PI);

        _PostProcessComputeShader.SetInt("meshSize", c_MeshQuantity + 2);
        _PostProcessComputeShader.SetFloat("quadScale", c_MeshScale);

        _PostProcessComputeShader.Dispatch(0, m_IndexLimit / 32, m_IndexLimit / 32, 1);

        b_Vertices.GetData(m_Vertices);
        b_Color.GetData(m_Color);

        // Creates the 2D texture using the aquired Colour data.
        for (int y = 0; y < c_MeshQuantity + 2; y++)
        {
            for (int x = 0; x < c_MeshQuantity + 2; x++)
            {
                // Prevents colouration seams from the vertex skirt by assigning the true vertex grid coordinates the proper colour values.
                if (x <= 0 || x >= c_MeshQuantity + 2 || y <= 0)
                {
                    m_Texture.SetPixel(x, y, m_Color[x + y * c_MeshQuantity]);
                } else
                {
                    m_Texture.SetPixel(x, y, m_Color[x + y * (c_MeshQuantity + 2)]);
                }
            }
        }

        // Finalisation of mesh data
        chunkMesh.vertices = m_Vertices;
        chunkMesh.triangles = m_Indices;
        chunkMesh.uv = m_UV;
        chunkMesh.colors = m_Color;

        m_Texture.Apply();

        // Dispose of all the Buffers to prevent Memory Leaks.
        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Indices?.Dispose();
        b_UV?.Dispose();
        b_Normals?.Dispose();
        b_Color?.Dispose();

        Chunk.GetComponent<MeshFilter>().sharedMesh = chunkMesh;
        Chunk.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = null;
        Chunk.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = m_Texture;

        // Collider Attatchment
        Chunk.AddComponent<BoxCollider>();

        // Object Placement based on the chunks quadtree depth
        if (parentGrid.n_depth >= settings.Quadtree_maxDepth - 3) PopulateObjects();
    }


    // Populates a Chunk with scattered objects, namely Trees.
    private void PopulateObjects() {
        
        // Instantiate List of Chunk Objects
        chunkObjects = new List<GameObject>();

        float objDensity = settings.objDensity * c_MeshScale;
        float scale = c_MeshQuantity * c_MeshScale;

        for (int y = 0; y < c_MeshQuantity * objDensity; y++)
        {
            for (int x = 0; x < c_MeshQuantity * objDensity; x++)
            {
                float xPos = (x * c_MeshScale / objDensity) - scale / 2;
                float zPos = (y * c_MeshScale / objDensity) - scale / 2;

                // Find the Vertex Y
                int vertexIndex = ((int)(x / objDensity) + 1) + ((int)(y / objDensity) + 1) * (c_MeshQuantity + 2);
                float vertexY = m_Vertices[vertexIndex].y;

                float heightFilter = Mathf.Max(1, vertexY) / 750;

                // Find the vertex Y 
                int NormalIndex = (int)(x / objDensity) + (int)(y / objDensity) * c_MeshQuantity;

                // Find the normal
                float normalY = 1 - Vector3.Dot(chunkMesh.normals[NormalIndex], new Vector3(0, 1, 0));


                // Determine if the spot is valid.
                if (normalY < 0.07f && PerlinNoise2D.PerlinNoise(
                    parentGrid.g_Position.x + xPos,
                    parentGrid.g_Position.z + zPos,
                    231873,
                    8,
                    0.000015f,
                    1,
                    0.5f,
                    2,
                    false,
                    true
                ) >= heightFilter) {

                    // Shifts the object by a slight amount using it's position to find some noise offset.
                    float noiseOffset = (PerlinNoise2D.Noise2D(
                        Mathf.Abs(parentGrid.g_Position.x + xPos * 125),
                        Mathf.Abs(parentGrid.g_Position.z + zPos * 125)
                    ) * 2 - 1) * 15;

                    Vector3 objectPosition = new Vector3(
                        xPos + noiseOffset,
                        vertexY,
                        zPos + noiseOffset
                    );

                    var obj = GameObject.Instantiate(settings.prefab, parentGrid.g_Position + objectPosition, Quaternion.identity);
                    obj.transform.parent = Chunk.transform;
                    //obj.GetComponent<MeshRenderer>().material.color = new(1, normalY, 1);

                    chunkObjects.Add(obj);
                }
            }
        }
    }

    public void UpdateChunk()
    {
        // Will hide any objects that are too far away from the player.
        Vector3 PlayerPos = Camera.main.transform.position;

        Vector3 ObjectBoundScale = new Vector3(1, 0, 1) * ((settings.Chunk_QuadQuantity * settings.Chunk_QuadScale) * 8);
        float SphereOfInfluence = (ObjectBoundScale * settings.Quadtree_GridSphericalCheck).sqrMagnitude;

        if (chunkObjects != null)
        {
            foreach(var obj in chunkObjects)
            {
                if ((obj.transform.position - PlayerPos).sqrMagnitude <= SphereOfInfluence)
                {
                    obj.SetActive(true);
                } else
                {
                    obj.SetActive(false);
                }
            }
        }

    }
}
