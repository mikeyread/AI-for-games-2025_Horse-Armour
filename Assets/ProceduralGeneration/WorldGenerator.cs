using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] int seed;

    List<Chunk> chunks = new List<Chunk>();
    PerlinNoise2D noise;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);
        noise = new PerlinNoise2D();
        chunks.Add(new Chunk(new Vector3(0,0,0)));
        //chunks.Add(new Chunk(new Vector3(0,0,1)));
    }

    private void Start()
    {
        chunks[0].GenerateGrid(noise);
        //chunks[1].GenerateGrid(noise);
    }
}


// Calculate Perlin noise values at x and z coordinate specification
// If coordinate specification is identical (ie at chunk seams) it will always result in the same value.
// Perlin Noise itself is a virtual grid, it is generate on demand but represents a theoretically infinite grid.
// Noise itself is a function, look-up noise function algorithmns.