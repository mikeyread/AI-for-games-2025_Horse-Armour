using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

using static WorldOptions;


// Chunks are mesh and collision regions which form a unified terrain, including mesh generation and loading/unloading tasks.
public class OldChunk {
    
    public GameObject chunk { get; private set; }
    private Mesh terrain;


    public bool generated = false;

    public OldChunk(Vector3 position)
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
    ///  Generates the Geometry Mesh of the Chunk
    /// </summary>
    private void GenerateMesh()
    {
        if (generated) return;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();

        List<Vector2> uv = new List<Vector2>();
        List<Color> colors = new List<Color>();


        float cornerX = chunk.transform.position.x - CHUNK_QUAD_AMOUNT / 2;
        float cornerZ = chunk.transform.position.z - CHUNK_QUAD_AMOUNT / 2;

        for (int z = 0; z < CHUNK_QUAD_AMOUNT + 1; z++)
        {
            for (int x = 0; x < CHUNK_QUAD_AMOUNT + 1; x++)
            {
                float trueX = cornerX + (x * CHUNK_QUAD_SCALAR);
                float trueZ = cornerZ + (z * CHUNK_QUAD_SCALAR);

                // Basic layered noise
                float y = ChunkNoise(trueX, trueZ);
                vertices.Add(new Vector3(x * CHUNK_QUAD_SCALAR, y, z * CHUNK_QUAD_SCALAR));

                y = y / 200;
                colors.Add(new Color(y, y, y));
                
                if (x % 2 == 0)
                {
                    uv.Add(new Vector2(1, 1));
                } else
                {
                    uv.Add(new Vector2(0, 0));
                }

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
        terrain.uv = uv.ToArray();

        chunk.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Particles/Standard Unlit"));
        
        chunk.GetComponent<MeshFilter>().mesh = terrain;
        chunk.GetComponent<MeshFilter>().mesh.Optimize();

        generated = true;
    }


    public float ChunkNoise(float x, float y)
    {
        float roads = PerlinNoise2D.PerlinNoise(x, y, -3974, 8, 0.1f, 12f, 0.5f, 2f, true, true);
        float valleys = PerlinNoise2D.PerlinNoise(x, y, 1272, 8, 0.01f, 12f, 0.5f, 2f, true, false);
        float variation = PerlinNoise2D.PerlinNoise(x, y, -7612, 12, 0.15f, 3f, 0.5f, 2f, false, false);
        float mountainous = PerlinNoise2D.PerlinNoise(x, y, -16183, 8, 0.002f, 128f, 0.5f, 2f) - 64f;


        return (mountainous - valleys) + (variation * roads);
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
