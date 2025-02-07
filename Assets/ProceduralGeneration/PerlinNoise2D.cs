using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


// Perlin noise implementation taken from: https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-2D-noise.html
// Basal level, allows infinite perlin grid generation taking in the X and Z coordinates on the grid as the offset.
public class PerlinNoise2D
{
    // Hash contains a randomly generated table of values.
    static float[] hash = new float[tableSize * tableSize];

    const int tableSize = 256;
    const int maxTableSize = tableSize - 1;

    static PerlinNoise2D()
    {
        for (int k = 0; k < tableSize * tableSize; k++)
        {
            hash[k] = Random.value;
        }
    }

    static float perlinStep(float t)
    {
        return t * t * (3 - 2 * t);
    }

    static float perlinLerp(float lo, float t, float hi)
    {
        return lo * (1 - t) + hi * t;
    }


    // Aquires the values found at a specified x and z coordinate.
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
        float sx = perlinStep(tx);
        float sz = perlinStep(tz);

        float nx0 = perlinLerp(c00, c10, sx);
        float nx1 = perlinLerp(c01, c11, sx);


        return perlinLerp(nx0, nx1, sz);
    }
}
