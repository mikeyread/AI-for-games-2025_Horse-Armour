using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using static WorldOptions;


// Chunks are mesh and collision regions which form a unified terrain, including mesh generation and loading/unloading tasks.
public class Chunk {

    public GameObject chunk;
    private Mesh terrain;

    public bool generated = false;


    public Chunk(Vector3 position)
    {
        chunk = new GameObject("World Chunk");

        // Mesh Components
        chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>();
        terrain = new Mesh();

        Vector3 offset = position * CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT;
        chunk.transform.position = offset;

        GenerateMesh();
    }

    /// <summary>
    ///  Generates the Mesh of the Chunk
    /// </summary>
    private void GenerateMesh()
    {
        if (generated) return;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        float cornerX = chunk.transform.position.x - CHUNK_QUAD_AMOUNT / 2;
        float cornerZ = chunk.transform.position.z - CHUNK_QUAD_AMOUNT / 2;

        for (int z = 0; z < CHUNK_QUAD_AMOUNT + 1; z++)
        {
            for (int x = 0; x < CHUNK_QUAD_AMOUNT + 1; x++)
            {
                float trueX = cornerX + (x * CHUNK_QUAD_SCALAR);
                float trueZ = cornerZ + (z * CHUNK_QUAD_SCALAR);

                float baseY = PerlinNoise2D.FractalSum(Mathf.Abs(trueX), Mathf.Abs(trueZ), 12, 0.33f, 1f, 0.5f, 2f);
                float hilly = PerlinNoise2D.FractalSum(Mathf.Abs(trueX), Mathf.Abs(trueZ), 8, 0.033f, 32f, 0.5f, 2f) - 8f;
                float mountainous = PerlinNoise2D.FractalSum(Mathf.Abs(trueX), Mathf.Abs(trueZ), 8, 0.0033f, 128f, 0.5f, 2f);

                float finalY = Mathf.Max(Mathf.Max(baseY, hilly),mountainous);


                vertices.Add(new Vector3(x * CHUNK_QUAD_SCALAR, finalY, z * CHUNK_QUAD_SCALAR));
                colors.Add(new Color(1, 0, 0));

                if (z < CHUNK_QUAD_AMOUNT && x < CHUNK_QUAD_AMOUNT)
                {
                    int zOffset = z * (CHUNK_QUAD_AMOUNT + 1);

                    indices.Add(x + zOffset + 1);
                    indices.Add(x + zOffset);
                    indices.Add(x + 1 + zOffset + CHUNK_QUAD_AMOUNT);

                    indices.Add(x + 1 + zOffset);
                    indices.Add(x + 1 + zOffset + CHUNK_QUAD_AMOUNT);
                    indices.Add(x + 2 + zOffset + CHUNK_QUAD_AMOUNT);
                }
            }
        }

        // Bad Normal Generation, buggy at chunk borders, but it works otherwise.
        for (int z = 0; z < CHUNK_QUAD_AMOUNT + 1; z++)
        {
            for (int x = 0; x < CHUNK_QUAD_AMOUNT + 1; x++)
            {
                int zOffset = z * (CHUNK_QUAD_AMOUNT + 1);

                Vector3 o1;
                Vector3 o2;
                if (z >= CHUNK_QUAD_AMOUNT)
                {
                    if (x >= CHUNK_QUAD_AMOUNT)
                    {
                        o1 = vertices[x + zOffset - 1] - vertices[x + zOffset];
                        o2 = vertices[x + zOffset - 1 - CHUNK_QUAD_AMOUNT] - vertices[x + zOffset];
                        normals.Add(Vector3.Cross(o2, o1).normalized);
                        continue;
                    }

                    o1 = vertices[x + zOffset + 1] - vertices[x + zOffset];
                    o2 = vertices[x + zOffset + 1 - CHUNK_QUAD_AMOUNT] - vertices[x + zOffset];

                    normals.Add(Vector3.Cross(o2, o1).normalized);
                    continue;
                }
                else
                {
                    o1 = vertices[x + zOffset + 1] - vertices[x + zOffset];
                    o2 = vertices[x + zOffset + 1 + CHUNK_QUAD_AMOUNT] - vertices[x + zOffset];

                    normals.Add(Vector3.Cross(o2, o1).normalized);
                }
            }
        }

        //Debug.Log("Normals: " + normals.Count + " to Vertices:" + vertices.Count);

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.triangles = indices.ToArray();
        terrain.normals = normals.ToArray();
        terrain.colors = colors.ToArray();

        chunk.GetComponent<MeshFilter>().mesh = terrain;
        chunk.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        //chunk.isStatic = true;

        generated = true;
    }


    public void Reload()
    {
        chunk.SetActive(true);
    }

    public void Unload()
    {
        chunk.SetActive(false);
    }
}
