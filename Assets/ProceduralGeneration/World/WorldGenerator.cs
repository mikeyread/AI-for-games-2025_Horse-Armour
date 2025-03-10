using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static WorldOptions;
using Random = UnityEngine.Random;


// Handles management of all chunks in the world.
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int seed;

    // Uses localised Chunk-Grid coordinates as a key to determine if a chunk already exists.
    private Dictionary<Vector3, Chunk> _ChunkBuffer;
    private List<Vector3> _ChunksToRender;

    private Vector3 w_PlayerPosition;
    private Vector3 w_RenderingChunkPosition;
    private Vector3 w_ChunkGridPosition;

    // Camera parameter for Chunk Snapping, unused because the current implementation is janky.
    private Vector3 minimumY;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);

        w_PlayerPosition = new Vector3();
        w_ChunkGridPosition = new Vector3();

        _ChunkBuffer = new Dictionary<Vector3, Chunk>();
        _ChunksToRender = new List<Vector3>();

        minimumY = Vector3.zero;
    }

    private void Update()
    {
        // Normalizes player position in respect to the chunk grid.
        w_PlayerPosition = Camera.main.transform.position / (CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT);
        
        w_ChunkGridPosition.x = Mathf.Floor(w_PlayerPosition.x + 0.5f);
        w_ChunkGridPosition.z = Mathf.Floor(w_PlayerPosition.z + 0.5f);

        UpdateRenderedChunks();
        //UpdateCameraHeight();
    }


    // Compute Shader Chunk Generation
    private void UpdateRenderedChunks()
    {
        for (int bx = 0; bx < RENDER_DISTANCE * 2 + 1; bx++)
        {
            for (int bz = 0; bz < RENDER_DISTANCE * 2 + 1; bz++)
            {
                float trueBx = bx - RENDER_DISTANCE;
                float trueBz = bz - RENDER_DISTANCE;

                w_RenderingChunkPosition = new Vector3(w_ChunkGridPosition.x + trueBx, 0, w_ChunkGridPosition.z + trueBz);
                float distance = (w_RenderingChunkPosition - w_PlayerPosition).sqrMagnitude;

                _ChunksToRender.Add(w_RenderingChunkPosition);

                if (!_ChunkBuffer.ContainsKey(w_RenderingChunkPosition) && distance - 75f <= RENDER_DISTANCE * RENDER_DISTANCE)
                {
                    _ChunkBuffer.Add(w_RenderingChunkPosition, new Chunk(w_RenderingChunkPosition));
                }
            }
        }

        foreach(var chunk in _ChunkBuffer)
        {
            chunk.Value.CheckThread();
        }
    }

    // very basic camera repositioner, very janky and likely won't be used.
    private void UpdateCameraHeight()
    {
        if (!_ChunkBuffer.ContainsKey(w_ChunkGridPosition)) return;

        // Local position
        Vector3 localPosition = (w_PlayerPosition - w_ChunkGridPosition) * CHUNK_QUAD_AMOUNT;
        localPosition.x += CHUNK_QUAD_AMOUNT * CHUNK_QUAD_SCALAR;
        localPosition.z += CHUNK_QUAD_AMOUNT * CHUNK_QUAD_SCALAR;

        //Debug.Log(localPosition);

        // Repositions based on the local position
        minimumY = Camera.main.transform.position;
        minimumY.y = Mathf.Max(minimumY.y, _ChunkBuffer[w_ChunkGridPosition].FindVertexHeight(localPosition));

        Camera.main.transform.position = minimumY;
    }

    //foreach(var ID in _ChunksToRender)
    //{
    //    float distance = (ID - w_PlayerPosition).sqrMagnitude;
    //    if (!_ChunkBuffer.ContainsKey(w_RenderingChunkPosition) && distance - 50f <= RENDER_DISTANCE * RENDER_DISTANCE)
    //    {
    //        _ChunkBuffer.Add(w_RenderingChunkPosition, new Chunk(w_RenderingChunkPosition));
    //    }
    //}
}



// LEGACY CODE FOR OLD CHUNK GENERATION
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
