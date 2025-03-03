using Unity.VisualScripting;
using UnityEngine;


public class ComputeShaderMesh : MonoBehaviour
{
    private Mesh grid;

    public ComputeShader _ComputeShader;
    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Indices;
    public ComputeBuffer b_HashTable;

    //private int c_MeshDetail = WorldOptions.CHUNK_QUAD_AMOUNT;
    //private float c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR;
    [Header("Mesh Options")]
    [SerializeField][Range(16,256)] int c_MeshQuantity;
    [SerializeField][Range(0.1f, 25f)] float c_MeshScale;
    private int m_IndexLimit;

    // Noise settings
    [Header("Perlin Noise Options")]
    [SerializeField] Vector2 c_GlobalOffset;
    [SerializeField][Range(1, 16)] int c_Octaves = 2;
    [SerializeField][Min(0.0033f)] float c_Frequency = 0.33f;
    [SerializeField][Min(0.05f)] float c_Amplitude = 1;
    [SerializeField][Range(0.25f, 2.5f)] float c_Persistence = 0.5f;
    [SerializeField][Range(1f, 2.5f)] float c_Lacurnity = 2f;

    private Vector4 c_GlobalPosition;
    private Vector3[] Vertices;
    private int[] Indices;


    private void Awake()
    {
        Initialize();
        GenerateMesh();
    }

    private void FixedUpdate()
    {
        Refresh();
        GenerateMesh();
    }

    // Generates Mesh by pushing any initial parameters into a Compute Shader for processing.
    void GenerateMesh()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);

        // Hash Table
        b_HashTable = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));
        b_HashTable.SetData(PerlinNoise2D._Noise2D);
        _ComputeShader.SetBuffer(0, "hash", b_HashTable);

        // Provides the Constants to the Kernel
        _ComputeShader.SetInt("meshSize", c_MeshQuantity);
        _ComputeShader.SetFloat("quadScale", c_MeshScale);
        _ComputeShader.SetFloat("PI", Mathf.PI);
        _ComputeShader.SetVector("globalPosition", c_GlobalPosition);
        _ComputeShader.SetVector("globalOffset", c_GlobalOffset);

        _ComputeShader.SetInt("octaves", c_Octaves);
        _ComputeShader.SetFloat("frequency", c_Frequency);
        _ComputeShader.SetFloat("amplitude", c_Amplitude);
        _ComputeShader.SetFloat("persistence", c_Persistence);
        _ComputeShader.SetFloat("lacurnity", c_Lacurnity);

        // Assigns our buffers that will have data written to and from the Kernel
        _ComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _ComputeShader.SetBuffer(0, "indices", b_Indices);

        // Dispatches the Kernel for processing.
        _ComputeShader.Dispatch(0, m_IndexLimit / 16, m_IndexLimit / 16, 1);

        b_Vertices.GetData(Vertices);
        b_Indices.GetData(Indices);

        b_HashTable.Dispose();
        b_Vertices.Dispose();
        b_Indices.Dispose();

        grid.vertices = Vertices;
        grid.triangles = Indices;

        GetComponent<MeshFilter>().mesh = grid;
    }

    // Initializes all vital data once.
    private void Initialize()
    {
        // Prevents leaks (hopefully).
        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Indices?.Dispose();


        if (grid == null)
        {
            grid = new Mesh();
        }

        if (GetComponent<MeshFilter>() == null)
        {
            this.AddComponent<MeshFilter>();
        }

        if (GetComponent<MeshRenderer>() == null)
        {
            this.AddComponent<MeshRenderer>();
        }

        Refresh();
    }

    // Refreshes
    private void Refresh()
    {
        c_GlobalPosition = transform.position;
        m_IndexLimit = c_MeshQuantity * c_MeshQuantity;

        if (Vertices == null || Vertices.Length != m_IndexLimit)
        {
            Vertices = new Vector3[m_IndexLimit];
        }

        if (Indices == null || Indices.Length != m_IndexLimit)
        {
            Indices = new int[m_IndexLimit * 6];
        }
    }
}

// Compute Shaders have two inputs - constants and buffers, only buffers can be retrieved back.
// Buffers need a correspondant array to write to in the script file.
// You must retrieve the buffer data after dispatching.
// Kernel Index represents a seperate function/method for execution within the Compute Shader file.