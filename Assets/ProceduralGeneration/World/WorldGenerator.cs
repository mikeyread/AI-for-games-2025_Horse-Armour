using System;
using System.Collections.Generic;
using UnityEngine;

using static WorldOptions;
using Random = UnityEngine.Random;


// Handles management of all in-scene chunks.
public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int seed;

    List<Chunk> chunks;
    private PerlinNoise2D noise;

    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);

        noise = new PerlinNoise2D();
        chunks = new List<Chunk>();


        chunks.Add(new Chunk(new Vector3(0,0,0)));
        chunks.Add(new Chunk(new Vector3(0,0,1)));
    }

    private void Start()
    {
        chunks[0].GenerateMesh(noise);
        chunks[1].GenerateMesh(noise);
    }

    // TODO: Automatic Chunk Generation.
    // https://www.redblobgames.com/grids/circle-drawing/
    private void Update()
    {
        // Normalizes player in respect to the chun
        Vector3 playerPos = Camera.main.transform.position / (CHUNK_QUAD_SCALAR * CHUNK_QUAD_AMOUNT);
        Debug.Log(playerPos);
    }
}