using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk {

    private GameObject o_Chunk;
    private Mesh m_Grid;

    public ComputeShader _ComputeShader;

    public ComputeBuffer b_HashTable;
    public ComputeBuffer b_Vertices;
    public ComputeBuffer b_Normals;
    public ComputeBuffer b_Indices;

    private float c_MeshScale = WorldOptions.CHUNK_QUAD_SCALAR;
    private int c_MeshQuantity = WorldOptions.CHUNK_QUAD_AMOUNT;
    private int m_IndexLimit;

    // Mesh and Object Settings
    private Vector4 c_GlobalPosition;
    private Vector3[] m_Vertices;
    private Vector3[] m_Normals;
    private int[] m_Indices;



    public Chunk(Vector3 position) {
        b_HashTable?.Dispose();

        b_Vertices?.Dispose();
        b_Normals?.Dispose();
        b_Indices?.Dispose();

        m_IndexLimit = c_MeshQuantity * c_MeshQuantity;


        if (o_Chunk == null)
        {
            o_Chunk = new GameObject("World Chunk");
        }

        if (m_Grid == null)
        {
            m_Grid = new Mesh();
        }

        if (_ComputeShader == null)
        {
            _ComputeShader = Object.Instantiate(Resources.Load<ComputeShader>("CS_Chunk"));
        }

        if (m_Vertices == null || m_Vertices.Length != m_IndexLimit)
        {
            m_Vertices = new Vector3[m_IndexLimit];
        }

        if (m_Indices == null || m_Indices.Length != m_IndexLimit)
        {
            m_Indices = new int[m_IndexLimit * 6];
        }

        if (o_Chunk.GetComponent<MeshFilter>() == null)
        {
            o_Chunk.AddComponent<MeshFilter>();

        }

        if (o_Chunk.GetComponent<MeshRenderer>() == null)
        {
            o_Chunk.AddComponent<MeshRenderer>();
            o_Chunk.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        }

        c_GlobalPosition = position * (c_MeshQuantity * c_MeshScale - 1 * c_MeshScale);
        
        GenerateMesh();
    }


    void GenerateMesh()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Normals = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);

        // Inserts the Perlin Hash Tables
        b_HashTable = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));
        b_HashTable.SetData(PerlinNoise2D._Noise2D);
        _ComputeShader.SetBuffer(0, "hash", b_HashTable);


        // Provides the Constants to the Kernel
        _ComputeShader.SetInt("meshSize", c_MeshQuantity);
        _ComputeShader.SetFloat("quadScale", c_MeshScale);

        _ComputeShader.SetFloat("PI", Mathf.PI);
        _ComputeShader.SetVector("globalPosition", c_GlobalPosition);


        // Assigns our buffers that will have data written to and from the Kernel
        _ComputeShader.SetBuffer(0, "vertices", b_Vertices);
        _ComputeShader.SetBuffer(0, "normals", b_Normals);
        _ComputeShader.SetBuffer(0, "indices", b_Indices);


        // Dispatches the Kernel for processing.
        _ComputeShader.Dispatch(0, m_IndexLimit / 16, m_IndexLimit / 16, 1);

        b_Vertices.GetData(m_Vertices);
        b_Indices.GetData(m_Indices);

        b_HashTable.Dispose();
        b_Vertices.Dispose();
        b_Normals.Dispose();
        b_Indices.Dispose();

        m_Grid.vertices = m_Vertices;
        m_Grid.normals = m_Normals;
        m_Grid.triangles = m_Indices;

        o_Chunk.GetComponent<MeshFilter>().mesh = m_Grid;

        // TODO: Manual Normal generation, using  recalculation does not account for chunk borders properly.
        o_Chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    
    public void Unload()
    {
        GameObject.Destroy(o_Chunk);
    }
}
