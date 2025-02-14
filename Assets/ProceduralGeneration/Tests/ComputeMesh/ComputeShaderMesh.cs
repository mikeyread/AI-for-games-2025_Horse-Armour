using Unity.VisualScripting;
using UnityEngine;


public class ComputeShaderMesh : MonoBehaviour
{
    private Mesh grid;

    public ComputeShader _ComputeShader;

    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Indices;

    //private int c_MeshDetail = WorldOptions.CHUNK_QUAD_AMOUNT;
    //private float c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR;
    [SerializeField][Range(2,256)] int c_MeshQuantity;
    [SerializeField][Range(0.01f, 2.5f)] float c_MeshScale;
    private Vector4 c_GlobalPosition;
    private int m_IndexLimit;



    private Vector3[] Vertices;
    private int[] Indices;


    private void Awake()
    {
        Initialize();
        GenerateMesh();
    }

    private void FixedUpdate()
    {   
        //GenerateMesh();
    }

    // Generates Mesh by pushing any initial parameters into a Compute Shader for processing.
    void GenerateMesh()
    {
        // Provides the Constants to the Kernel
        _ComputeShader.SetVector("globalPosition", c_GlobalPosition);
        _ComputeShader.SetInt("meshSize", c_MeshQuantity);
        _ComputeShader.SetFloat("quadScale", c_MeshScale);

        // Assigns our buffers that will have data written from the Kernel
        _ComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _ComputeShader.SetBuffer(0, "indices", b_Indices);

        // Dispatches the Kernel for processing.
        _ComputeShader.Dispatch(0, m_IndexLimit / 8, m_IndexLimit / 8, 1);

        b_Vertices.GetData(Vertices);
        b_Indices.GetData(Indices);

        b_Vertices.Dispose();
        b_Indices.Dispose();

        grid.vertices = Vertices;
        grid.triangles = Indices;

        GetComponent<MeshFilter>().mesh = grid;
    }

    // Initializes all variables and other stuff once.
    private void Initialize()
    {
        c_GlobalPosition = this.transform.position;
        m_IndexLimit = c_MeshQuantity * c_MeshQuantity;

        // Prevents leaks (hopefully).
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

        if (Vertices == null || Vertices.Length != m_IndexLimit)
        {
            Vertices = new Vector3[m_IndexLimit];
        }

        if (Indices == null || Indices.Length != m_IndexLimit)
        {
            Indices = new int[m_IndexLimit * 3];
        }

        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 3);
    }


    // Just spawns cubes at the vertices.
    Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
    private void OnDrawGizmos()
    {
        if (Vertices != null)
        {
            foreach (var pos in Vertices)
            {
                Gizmos.DrawCube(pos + this.transform.position, scale);
            }
        }
    }
}

// Compute Shaders have two inputs - constants and buffers, only buffers can be retrieved back.
// Buffers need a correspondant array to write to in the script file.
// You must retrieve the buffer data after dispatching.
// Kernel Index represents a seperate function/method for execution within the Compute Shader file.