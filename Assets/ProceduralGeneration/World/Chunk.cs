using System.Collections.Generic;
using UnityEngine;

using static WorldOptions;


// Chunks are mesh and collision regions which form a unified terrain, including mesh generation and loading/unloading tasks.
public class Chunk {

    public GameObject chunk;
    private Mesh terrain;

    private List<Vector3> vertices;
    private List<int> indices;

    private const float frequency = 1f;
    private const float amplitude = 1f;

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
    }


    public void GenerateMesh(PerlinNoise2D noise)
    {
        if (generated) return;

        vertices = new List<Vector3>();
        indices = new List<int>();

        for (int z = 0; z < CHUNK_QUAD_AMOUNT + 1; z++)
        {
            for (int x = 0; x < CHUNK_QUAD_AMOUNT + 1; x++)
            {
                // TODO: Fix Noise
                float trueX = (chunk.transform.position.x - chunk.transform.position.x * CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT) + x;
                float trueZ = (chunk.transform.position.z - chunk.transform.position.z * CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT) + z;
                float y = noise.Perlin2D(trueX * frequency, trueZ * frequency) * amplitude;

                vertices.Add(new Vector3(x * CHUNK_QUAD_SCALAR, y, z * CHUNK_QUAD_SCALAR));

                if (z <= CHUNK_QUAD_AMOUNT && x < CHUNK_QUAD_AMOUNT)
                {
                    int zOffset = z * CHUNK_QUAD_AMOUNT;

                    indices.Add(x + 1 + zOffset);
                    indices.Add(x + zOffset);
                    indices.Add(x +  CHUNK_QUAD_AMOUNT + 1 + zOffset);

                    indices.Add(x + zOffset);
                    indices.Add(x + CHUNK_QUAD_AMOUNT + zOffset);
                    indices.Add(x + CHUNK_QUAD_AMOUNT + 1 + zOffset);
                }
            }
        }

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.triangles = indices.ToArray();

        chunk.GetComponent<MeshFilter>().mesh = terrain;

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
