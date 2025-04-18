using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;



public struct MockChunk
{
    public Vector3 position;
    public Vector2 bounds;
    public List<Vector3> objectPoints;
}



public class ObjectInstancing : MonoBehaviour
{
    [SerializeField] private float loadRadius;
    [SerializeField] private uint chunkSize;
    [SerializeField] private GameObject prefab;

    private Dictionary<Vector2, MockChunk> chunkMap;

    // Concept, tie object generation to the chunk itself, rather than a seperate system.
    // Objects are populated after the terrain generation (obviously) so that the terrain data is already present.
    // The process of generation figures out an optimal "seed point" with variable density, then once a seed point is found it offsets the object by a minor amount to remove tiling.

    private void Awake() {
        chunkMap = new Dictionary<Vector2, MockChunk>();
    }

    private void Update()
    {
        Vector2 PlayerPosition = new Vector2(Mathf.Floor(Camera.main.transform.position.x / chunkSize) + 0.5f, Mathf.Floor(Camera.main.transform.position.z / chunkSize) + 0.5f);

        if (!chunkMap.TryGetValue(PlayerPosition, out MockChunk chunk)) chunkMap.Add(PlayerPosition, GenerateChunk(PlayerPosition));
        Debug.Log("Current Quantity of Mock Chunks: " + chunkMap.Count);
    }



    private MockChunk GenerateChunk(Vector2 chunkPos) {
        MockChunk chunk = new MockChunk();

        chunk.position = new(chunkPos.x * chunkSize, 0, chunkPos.y * chunkSize);
        chunk.bounds = new(chunkSize, chunkSize);
        chunk.objectPoints = new List<Vector3>();

        Vector3 truePos = chunkPos * chunkSize;

        for (int y = 0; y < chunkSize; y++) {
            for (int x = 0; x < chunkSize; x++)
            {
                // Determines if the point on the chunk is viable for placement
                if (PerlinNoise2D.PerlinNoise(truePos.x + x, truePos.z + y, 561928) > 0.8) chunk.objectPoints.Add(new Vector3(x, 0, y) + truePos);
            }
        }

        return chunk;
    }

    private void OnDrawGizmos()
    {
        if (chunkMap == null) return;

        foreach (var chunk in chunkMap.Values) {
            Gizmos.DrawWireCube(chunk.position, new(chunk.bounds.x, 0, chunk.bounds.y));

            if (chunk.objectPoints == null) continue;
            foreach (var point in chunk.objectPoints) {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
}