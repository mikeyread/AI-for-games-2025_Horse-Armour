using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    float offset = 1;
    int chunkScale = 4;


    GameObject chunk;
    Mesh terrain;

    public Chunk(Vector3 position)
    {
        chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        terrain = new Mesh();
        chunk.transform.position = new Vector3(position.x * offset, 0, position.z * offset);
    }

    public void GenerateGrid(PerlinNoise2D noise)
    {

        // Mesh Generation
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int z = 0; z < chunkScale + 1; z++)
        {
            for (int x = 0; x < chunkScale + 1; x++)
            {
                vertices.Add(new Vector3(x, 0, z));

                if (z < chunkScale && x < chunkScale)
                {
                    indices.Add(x + 1 + z * chunkScale);
                    indices.Add(x + z * chunkScale);
                    indices.Add(x + z * chunkScale + 1);

                }
            }
        }

        // x = 3 z = 3
        // 13, 15 - need plus 3.

        terrain.vertices = vertices.ToArray();
        terrain.triangles = indices.ToArray();
        chunk.GetComponent<MeshFilter>().mesh = terrain;
    }
}
