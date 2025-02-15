using UnityEngine;


/// <summary>
/// A class that contains multiple functions allowing generation of noise.
/// </summary>
public class PerlinNoise2D
{
    public const int noiseQuality = 256;
    const int noiseIterator = noiseQuality - 1;

    /// <summary>
    /// A hash-table containing a large list of randomized floats that is initialized on creation of the Noise. It allows noise to be deterministic on the selection of a seed.
    /// </summary>
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


    /// <summary>
    /// Performs Interpolation between two points using a percentage.
    /// </summary>
    /// <param name="left">The left side point to interpolate from.</param>
    /// <param name="right">The right side point to interpolate towards.</param>
    /// <param name="percent">The degree of interpolation between the left and right, zero to one.</param>
    public static float PerlinLerp(float left, float right, float percent)
    {
        return (1 - percent) * left + percent * right;
    }

    /// <summary>
    /// Similar to PerlinLerp, except it uses cosine Interpolation for a smoother transition.
    /// </summary>
    /// <param name="left">The left side point to interpolate from.</param>
    /// <param name="right">The right side point to interpolate towards.</param>
    /// <param name="percent">The degree of interpolation between the left and right.</param>
    public static float PerlinCosineLerp(float left, float right, float percent)
    {
        return ((Mathf.Cos(Mathf.PI * percent) + 1) / 2) * left + ((1 - Mathf.Cos(Mathf.PI * percent)) / 2 * right);
    }

    /// <summary>
    /// Applies a Cubic Hermite Interpolation modifier with a clamp to smooth an inputted value.
    /// </summary>
    /// <param name="value">The value you want to smooth</param>
    public static float SmoothStep(float value)
    {
        return Mathf.Clamp(Mathf.Pow(value, 2) * 3 - Mathf.Pow(value, 3) * 2, 0, 1);
    }

    /// <summary>
    /// Similar to Smoothstep, except the smoothing interpolation is stronger.
    /// </summary>
    public static float SmootherStep(float value)
    {
        return Mathf.Clamp(Mathf.Pow(value, 5) * 6 - Mathf.Pow(value, 4) * 15 + Mathf.Pow(value, 3) * 10, 0, 1);
    }


    /// <summary>
    /// Outputs a float created from the combination of multiple Perlin layers.
    /// </summary>
    /// <param name="octaves">The number of Perlin layers generated and combined together.</param>
    /// <param name="offset">An offset applied to the coordinates.</param>
    /// <param name="frequency">The initial amount of detail the Noise will have.</param>
    /// <param name="amplitude">The initial strength of vertical offset applied by the Noise.</param>
    /// <param name="persistence">The multiplier of the amplitude per Octave layer.</param>
    /// <param name="lacurnity">The multiplier of the frequency per Octave layer.</param>
    /// <param name="turbulent">Determines if the noise is turbulent.</param>
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency, float amplitude, float persistence, float lacurnity, bool turbulent, bool normalized)
    {
        float sum = 0;
        float totalSum = 0;

        for (int i = 0; i < octaves; i++)
        {
            sum += amplitude * Noise2D(Mathf.Abs(xPos + offset) * frequency, Mathf.Abs(yPos + offset) * frequency);
            totalSum += amplitude;

            amplitude *= persistence;
            frequency *= lacurnity;
        }

        if (turbulent) {
            sum = Mathf.Abs((sum - totalSum / 2) * 2);
        }

        if (normalized) {
            return sum / totalSum;
        }
        else
        {
            return sum;
        }
    }

    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency, float amplitude, float persistence, float lacurnity, bool Turbulent) { return PerlinNoise(xPos, yPos, offset, octaves, frequency, amplitude, persistence, lacurnity, Turbulent, false); }
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency, float amplitude, float persistence, float lacurnity) { return PerlinNoise(xPos, yPos, offset, octaves, frequency, amplitude, persistence, lacurnity, false); }
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency, float amplitude, float persistence) { return PerlinNoise(xPos, yPos, offset, octaves, frequency, amplitude, persistence, 1); }
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency, float amplitude) {return PerlinNoise(xPos, yPos, offset, octaves, frequency, amplitude, 1); }
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves, float frequency) { return PerlinNoise(xPos, yPos, offset, octaves, frequency, 1); }
    public static float PerlinNoise(float xPos, float yPos, float offset, int octaves) { return PerlinNoise(xPos, yPos, offset, octaves, 1); }
    public static float PerlinNoise(float xPos, float yPos, float offset) { return PerlinNoise(xPos, yPos, offset, 1); }


    public static float Noise2D(float xPos, float yPos)
    {
        int flooredX = (int)Mathf.Floor(xPos);
        int flooredY = (int)Mathf.Floor(yPos);

        float bottomLeftCorner  = _Noise2D[  flooredX % noiseIterator,         flooredY % noiseIterator];
        float bottomRightCorner = _Noise2D[ (flooredX + 1) % noiseIterator,    flooredY % noiseIterator];
        float topLeftCorner     = _Noise2D[  flooredX % noiseIterator,        (flooredY + 1) % noiseIterator];
        float topRightCorner    = _Noise2D[ (flooredX + 1) % noiseIterator,   (flooredY + 1) % noiseIterator];

        float InterpolatedX = xPos - flooredX;
        float InterpolatedY = yPos - flooredY;

        float xLerp = PerlinCosineLerp(bottomLeftCorner, bottomRightCorner, InterpolatedX);
        float yLerp = PerlinCosineLerp(topLeftCorner, topRightCorner, InterpolatedX);

        return PerlinCosineLerp(xLerp, yLerp, InterpolatedY);
    }
}


// TODO:
// Noise begins showing artefacts after five or so Kilometers, switch to using doubles.