using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// Perlin noise implementation taken from: https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-2D-noise.html
// Basal level, allows infinite perlin grid generation taking in the X and Z coordinates on the grid as the offset.
public class PerlinNoise2D
{
    static float[] hash = new float[tableSize * tableSize];


    static PerlinNoise2D()
    {
        for (int k = 0; k < tableSize * tableSize; k++)
        {
            hash[k] = Random.value;
        }
    }

    public float Perlin2D(float x, float z)
    {
        // Search the x and z coordinate position.
        int xi = (int)Mathf.Floor(x);
        int zi = (int)Mathf.Floor(z);

        // Finds the left and bottom corners of the noise Grid.
        float tx = x - xi;
        float tz = z - zi;

        // Find the corners of the internal Perlin grid.
        int rx0 = xi & maxTableSize;
        int rx1 = (rx0 + 1) & maxTableSize;
        int rz0 = zi & maxTableSize;
        int rz1 = (rz0 + 1) & maxTableSize;

        // Values of the perlin corners
        float c00 = hash[rz0 * maxTableSize + rx0];
        float c10 = hash[rz0 * maxTableSize + rx1];
        float c01 = hash[rz1 * maxTableSize + rx0];
        float c11 = hash[rz1 * maxTableSize + rx1];

        // Remapping and Interpolation here
        //

        return c00 * c10 * c01 * c11;
    }

    const int tableSize = 256;
    const int maxTableSize = tableSize - 1;
}
