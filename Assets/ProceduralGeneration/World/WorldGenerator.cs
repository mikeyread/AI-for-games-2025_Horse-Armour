using System.Collections.Generic;
using UnityEngine;

using static WorldOptions;
using Random = UnityEngine.Random;


// Handles management of all chunks in the world.
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int seed;

    private Dictionary<Vector2, Chunk> chunkBuffer;
    private PerlinNoise2D noise;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);
        noise = new PerlinNoise2D();

        chunkBuffer = new Dictionary<Vector2, Chunk>();
    }


    // Generates new chunk terrain while skipping any already existing chunks.
    private void RegenerateChunks()
    {
        foreach (var chunk in chunkBuffer) {
            if (chunk.Value.generated) continue;

            chunk.Value.GenerateMesh(noise);
        }
    }

    // Unloads all chunks outside of the players Render Distance.
    private void UnloadChunks(Vector2 position)
    {
        List<Vector2> toUnload = new List<Vector2>();
        foreach (var chunk in chunkBuffer)
        {
            if (chunk.Key == position) continue;
            
            toUnload.Add(chunk.Key);
        }

        foreach (var destroy in toUnload)
        {
            chunkBuffer[destroy].Unload();
        }
    }


    // TODO: Automatic Chunk Generation within a defined render distance.
    // https://www.redblobgames.com/grids/circle-drawing/
    private void Update()
    {
        // Normalizes player position in respect to the chunk grid.
        Vector3 playerPos = Camera.main.transform.position / (CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT);
        Vector2 floored = new Vector2(Mathf.Floor(playerPos.x), Mathf.Floor(playerPos.z));

        if (!chunkBuffer.ContainsKey(floored))
        {
            chunkBuffer.Add(floored, new Chunk(new Vector3(floored.x, 0 ,floored.y)));
            RegenerateChunks();
        }
        else
        {
            chunkBuffer[floored].Reload();
        }

        UnloadChunks(floored);
    }
}