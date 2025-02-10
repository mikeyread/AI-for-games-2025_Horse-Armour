using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


// Basal level, allows infinite perlin grid generation taking in the X and Z coordinates on the grid as the offset.
public class PerlinNoise2D
{
    const int noiseQuality = 256;
    const int noiseIterator = noiseQuality - 1;

    // Hash contains a randomly generated table of values.
    public static float[,] _Noise2D = new float[noiseQuality, noiseQuality];


    static PerlinNoise2D()
    {
        _Noise2D = new float[noiseQuality, noiseQuality];
        for (int noiseX = 0; noiseX < noiseIterator; noiseX++)
        {
            for (int noiseY = 0; noiseY < noiseIterator; noiseY++)
            {
                _Noise2D[noiseX, noiseY] = Random.value;
            }
        }
    }

    public static float perlinLerp(float x1, float x2, float i)
    {
        return (1 - i) * x1 + i * x2;
    }


    // Smooths any normalized value (0 to 1).
    public static float Smooth(float value)
    {
        return Mathf.Clamp(Mathf.Pow(value, 2) * 3 - Mathf.Pow(value, 3) * 2, 0, 1);
    }


    // Iterates through multiple noise functions and tallies up the result, then normalises it by the theoretically highest value possible.
    public static float FractalSum(float xPos, float yPos, int octaves, float frequency, float scale)
    {
        float sum = 0;
        float highestSum = 0;

        for (int i = 0; i < octaves; i++)
        {
            sum += Noise2D(xPos * frequency, yPos * frequency) * scale;

            highestSum += scale;
            frequency *= 2f;
            scale /= 2f;
        }

        return sum / highestSum;
    }


    // Calculates a 2D noise value using a specified x and y coordinate.
    public static float Noise2D(float xPos, float yPos)
    {
        int flooredX = Mathf.Abs((int)Mathf.Floor(xPos));
        int flooredY = Mathf.Abs((int)Mathf.Floor(yPos));

        //Debug.Log(flooredX % noiseIterator);
        //Debug.Log(flooredY % noiseIterator);

        float bottomLeftCorner  = _Noise2D[  flooredX % noiseIterator,         flooredY % noiseIterator];
        float bottomRightCorner = _Noise2D[ (flooredX + 1) % noiseIterator,    flooredY % noiseIterator];
        float topLeftCorner     = _Noise2D[  flooredX % noiseIterator,        (flooredY + 1) % noiseIterator];
        float topRightCorner    = _Noise2D[ (flooredX + 1) % noiseIterator,   (flooredY + 1) % noiseIterator];

        float InterpolatedX = Smooth(xPos - flooredX);
        float InterpolatedY = Smooth(yPos - flooredY);

        float xLerp = perlinLerp(bottomLeftCorner, bottomRightCorner, InterpolatedX);
        float yLerp = perlinLerp(topLeftCorner, topRightCorner, InterpolatedX);

        return perlinLerp(xLerp, yLerp, InterpolatedY);
    }
}
