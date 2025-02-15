using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public class NewChunk {
    private GameObject chunk;
    private Mesh grid;

    public ComputeShader _ComputeShader;

    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Indices;
    public ComputeBuffer b_HashTable;

    //private int c_MeshDetail = WorldOptions.CHUNK_QUAD_AMOUNT;
    //private float c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR;
    private int c_MeshQuantity = WorldOptions.CHUNK_QUAD_AMOUNT;
    private float c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR;
    private int m_IndexLimit;

    // Noise settings
    private Vector2 c_GlobalOffset;
    private int c_Octaves = 8;
    private float c_Frequency = 0.0075f;
    private float c_Amplitude = 64;
    private float c_Persistence = 0.5f;
    private float c_Lacurnity = 2f;

    private Vector4 c_GlobalPosition;
    private Vector3[] Vertices;
    private int[] Indices;

    public NewChunk(Vector2 position) {
        b_HashTable?.Dispose();
        b_Vertices?.Dispose();
        b_Indices?.Dispose();

        m_IndexLimit = c_MeshQuantity * c_MeshQuantity;

        if (chunk == null)
        {
            chunk = new GameObject();
        }

        if (grid == null)
        {
            grid = new Mesh();
        }

        if (_ComputeShader == null)
        {
            _ComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_Chunk"));
        }

        if (Vertices == null || Vertices.Length != m_IndexLimit)
        {
            Vertices = new Vector3[m_IndexLimit];
        }

        if (Indices == null || Indices.Length != m_IndexLimit)
        {
            Indices = new int[m_IndexLimit * 6];
        }

        if (chunk.GetComponent<MeshFilter>() == null)
        {
            chunk.AddComponent<MeshFilter>();
        }

        if (chunk.GetComponent<MeshRenderer>() == null)
        {
            chunk.AddComponent<MeshRenderer>();
        }

        c_GlobalPosition = new Vector3(position.x * c_MeshScale * c_MeshQuantity, 0, position.y * c_MeshScale * c_MeshQuantity);
        chunk.transform.position = c_GlobalPosition;

        GenerateMesh();
    }


    void GenerateMesh()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);

        // Inserts the Perlin Hash Table
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

        chunk.GetComponent<MeshFilter>().mesh = grid;
    }

    public void Unload()
    {
        //chunk.Destroy(grid);
        GameObject.Destroy(chunk);
    }
}
