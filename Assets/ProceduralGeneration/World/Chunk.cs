using System.Collections.Generic;
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
    }


    public void GenerateMesh()
    {
        if (generated) return;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();

        float cornerX = chunk.transform.position.x - CHUNK_QUAD_AMOUNT / 2;
        float cornerZ = chunk.transform.position.z - CHUNK_QUAD_AMOUNT / 2;

        for (int z = 0; z < CHUNK_QUAD_AMOUNT + 1; z++)
        {
            for (int x = 0; x < CHUNK_QUAD_AMOUNT + 1; x++)
            {
                float trueX = cornerX + (x * CHUNK_QUAD_SCALAR);
                float trueZ = cornerZ + (z * CHUNK_QUAD_SCALAR);


                float y = 0;
                if (PerlinNoise2D._Noise2D == null)
                {
                    Debug.Log("No Noise Array!");

                    y = 1;
                }
                else {
                    //Debug.Log(PerlinNoise2D._Noise2D.Length);
                    //Debug.Log(PerlinNoise2D._Noise2D[0,0]);

                    y = PerlinNoise2D.FractalSum(trueX, trueZ, 6, 0.33f, 64);
                }

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

        // Vertex Normals
        // Find cross product between neighboring indices
        for(int i = 0; i < vertices.Count; i++)
        {

            normals.Add(new Vector3(0,1,0));
        }

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.triangles = indices.ToArray();
        terrain.normals = normals.ToArray();

        chunk.GetComponent<MeshFilter>().mesh = terrain;
        chunk.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

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
