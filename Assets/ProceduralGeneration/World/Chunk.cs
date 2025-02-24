using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class Chunk {

    public GameObject o_Chunk;
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

    // Multi-threading
    Thread t_MeshGenerator;
    ChunkMeshCallBack t_ChunkMeshCallback;
    ChunkMeshThread t_ChunkMesh;
    bool hasChecked = false;


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

        //Debug.Log("Thread Value before thread: " + ThreadVal);
        //chunkCallback = new ChunkVariableCallback(ReturnValue);
        //output = new ChunkVariable(chunkCallback, 10f);

        //t_MeshGenerator = new Thread(new ThreadStart(output.WaitValue));
        //t_MeshGenerator.Start();

        GenerateMesh();
    }



    public void CheckThread()
    {
        if (t_MeshGenerator == null) return;
        if (t_MeshGenerator.IsAlive || hasChecked) return;

        t_MeshGenerator.Join();
        hasChecked = true;
    }



    private void GenerateMesh()
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

        // TODO: Manual Normal generation, using recalculation does not account for chunk borders properly.
        o_Chunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }


    private void GenerateMeshThreaded()
    {
        b_Vertices = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Normals = new ComputeBuffer(m_IndexLimit, sizeof(float) * 3);
        b_Indices = new ComputeBuffer(m_IndexLimit, sizeof(int) * 6);
        b_HashTable = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));


        t_ChunkMeshCallback = new ChunkMeshCallBack(ReturnChunkMesh);
        t_ChunkMesh = new ChunkMeshThread(t_ChunkMeshCallback, _ComputeShader, b_HashTable, b_Vertices, b_Normals, b_Indices, PerlinNoise2D._Noise2D, m_IndexLimit, c_GlobalPosition);

        t_MeshGenerator = new Thread(new ThreadStart(t_ChunkMesh.GenerateMesh));
        t_MeshGenerator.Start();

        b_HashTable.Dispose();
        b_Vertices.Dispose();
        b_Normals.Dispose();
        b_Indices.Dispose();
    }


    // Finds the height of a chunks vertex. Needs a rework.
    public float FindVertexHeight(Vector3 pos)
    {
        uint index = (uint)Mathf.Abs(pos.x + pos.z * c_MeshQuantity);

        if (index >= m_Grid.vertices.Length) return m_Grid.vertices[m_Grid.vertices.Length].y;

        //Debug.Log("Index: " + index + " Y: " + m_Grid.vertices[index].y);

        return m_Grid.vertices[index].y;
    }

    
    public void Unload()
    {
        GameObject.Destroy(o_Chunk);
    }


    // CHUNK MESH THREADING
    public static void ReturnChunkMesh(string message)
    {
        Debug.Log(message);
    }


    public delegate void ChunkMeshCallBack(string message);
    class ChunkMeshThread
    {
        ChunkMeshCallBack _chunkMeshCallback;

        ComputeShader _ComputeShader;

        ComputeBuffer b_HashTable;
        ComputeBuffer b_Vertices;
        ComputeBuffer b_Normals;
        ComputeBuffer b_Indices;

        Vector4 c_GlobalPosition;
        private Vector3[] m_Vertices;
        private Vector3[] m_Normals;
        private int[] m_Indices;

        private float[,] _Noise;

        int m_IndexLimit;




        public ChunkMeshThread(ChunkMeshCallBack callback, ComputeShader shader, ComputeBuffer a, ComputeBuffer b, ComputeBuffer c, ComputeBuffer d, float[,] noise, int indexLimit, Vector4 position)
        {
            this._chunkMeshCallback = callback;
            this.m_IndexLimit = indexLimit;
            this._ComputeShader = shader;

            this._Noise = noise;

            this.b_HashTable = a;
            this.b_Vertices = b;
            this.b_Normals = c;
            this.b_Indices = d;

            m_Vertices = new Vector3[m_IndexLimit];
            m_Indices = new int[m_IndexLimit * 6];
            this.c_GlobalPosition = position;
        }

        // ISSUE, CANNOT PEROFRM ACTION OR DISPATCH COMPUTE SHADERS FROM A NON-MAIN THREAD.
        // CONCEPT #01: Dispatch and aquire data from the main thread, but don't generate the mesh until the thread has the parameters necessary to make it.
        // CONCEPT #02: Asynchronous calls to the Compute Shader while remaining entirely within the main thread.
        // https://www.reddit.com/r/Unity3D/comments/vdncwe/compute_shaders_from_a_background_thread/
        public void GenerateMesh()
        {
            // Inserts the Perlin Hash Tables
            b_HashTable.SetData(_Noise);
            _ComputeShader.SetBuffer(0, "hash", b_HashTable);


            // Provides the Constants to the Kernel
            _ComputeShader.SetInt("meshSize", WorldOptions.CHUNK_QUAD_AMOUNT);
            _ComputeShader.SetFloat("quadScale", WorldOptions.CHUNK_QUAD_SCALAR);

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

            _chunkMeshCallback?.Invoke("Chunk Mesh Thread Finished");
        }
    }




    // A basic Threaded Chunk pass, changes the debug ThreadVal.
    private float ThreadVal;
    public void ReturnValue(float val) { ThreadVal = val; }

    public delegate void ChunkVariableCallback(float value);
    class ChunkVariable
    {
        public float _ReturnValue;
        ChunkVariableCallback _chunkValueCallback;

        public ChunkVariable(ChunkVariableCallback callback, float val)
        {
            this._chunkValueCallback = callback;
            this._ReturnValue = val;
        }

        // Waits three seconds before changing the chunks value.
        public void WaitValue()
        {
            Thread.Sleep(3333);
            _chunkValueCallback?.Invoke(_ReturnValue);
        }
    }
}
