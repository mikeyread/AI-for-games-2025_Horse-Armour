using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

using UnityEngine;

using static WorldOptions;
using Random = UnityEngine.Random;


// Handles management of all chunks in the world.
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int seed;

    // Uses localised Chunk-Grid coordinates as a key to determine if a chunk already exists.
    private Dictionary<Vector2, NewChunk> _NewChunkBuffer;
    private PerlinNoise2D noise;
    private Vector3 w_PlayerPosition;
    private Vector2 w_FlooredPosition;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);
        noise = new PerlinNoise2D();

        w_PlayerPosition = new Vector2();
        w_FlooredPosition = new Vector2();

        _NewChunkBuffer = new Dictionary<Vector2, NewChunk>();
    }

    private void Update()
    {
        // Normalizes player position in respect to the chunk grid.
        w_PlayerPosition = Camera.main.transform.position / (CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT);
        
        w_FlooredPosition.x = Mathf.Floor(w_PlayerPosition.x);
        w_FlooredPosition.y = Mathf.Floor(w_PlayerPosition.z);

        Debug.Log(w_PlayerPosition);

        GenerateChunksV2(w_FlooredPosition);
    }


    // Generates new chunk terrain while skipping any already existing chunks.
    //private void GenerateChunks(Vector2 position)
    //{
    //    for(int bx = 0; bx < RENDER_DISTANCE * 2 + 1; bx++)
    //    {
    //        for (int bz = 0; bz < RENDER_DISTANCE * 2 + 1; bz++)
    //        {
    //            float trueBx = bx - RENDER_DISTANCE;
    //            float trueBz = bz - RENDER_DISTANCE;
    //            Vector2 renderPos = new Vector2(Mathf.Floor(position.x + trueBx), Mathf.Floor(position.y + trueBz));

    //            if (!chunkBuffer.ContainsKey(renderPos))
    //            {
    //                chunkBuffer.Add(renderPos, new Chunk(new Vector3(renderPos.x, 0, renderPos.y)));
    //            }
    //        }
    //    }
    //}

    // Compute Shader Chunk Generation
    private void GenerateChunksV2(Vector2 position)
    {
        for (int bx = 0; bx < RENDER_DISTANCE * 2 + 1; bx++)
        {
            for (int bz = 0; bz < RENDER_DISTANCE * 2 + 1; bz++)
            {
                float trueBx = bx - RENDER_DISTANCE;
                float trueBz = bz - RENDER_DISTANCE;
                Vector2 renderPos = new Vector2(Mathf.Floor(position.x + trueBx), Mathf.Floor(position.y + trueBz));

                if (!_NewChunkBuffer.ContainsKey(renderPos))
                {
                    _NewChunkBuffer.Add(renderPos, new NewChunk(renderPos));
                }
            }
        }
    }

    //private void UnloadChunks(Vector2 position)
    //{
    //    // Uses a ToRender List to define all of the positions within the render distance of the player.
    //    List<Vector2> toRender = new List<Vector2>();
    //    for (int bx = 0; bx < RENDER_DISTANCE * 2 + 1; bx++)
    //    {
    //        for (int bz = 0; bz < RENDER_DISTANCE * 2 + 1; bz++)
    //        {
    //            // Finds the floored position of the target chunk in the render distance.
    //            float trueBx = bx - RENDER_DISTANCE;
    //            float trueBz = bz - RENDER_DISTANCE;
    //            Vector2 renderPos = new Vector2(Mathf.Floor(position.x + trueBx), Mathf.Floor(position.y + trueBz));
                
    //            float distance = (renderPos - position).magnitude;

    //            toRender.Add(renderPos);

    //            // Unloads all chunks before reloading all visible chunks within the Render distance.
    //            // Unoptimised but I ain't gonna care till I get actual terrain generating.
    //            foreach (var chunk in chunkBuffer)
    //            {
    //                chunk.Value.Unload();
    //                foreach (var posID in toRender)
    //                {
    //                    if (chunk.Key != posID) continue;

    //                    if ((position - chunk.Key).magnitude < RENDER_DISTANCE + 0.33f) chunk.Value.Reload();
    //                }
    //            }
    //        }
    //    }
    //}
}