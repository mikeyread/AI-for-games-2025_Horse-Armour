using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


// A debug script that displays Noise as a 2D texture.
public class ProceduralNoise : MonoBehaviour
{
    public Texture2D texture;
    private Renderer QuadRenderer;
    private int TextureScale = 256;

    [SerializeField] int seed;
    private float[] _Noise1D;
    private float[,] _Noise2D;

    private const int noiseQuality = 256;
    private const int noiseIterator = noiseQuality - 1;

    [SerializeField][Range(0.01f,2.5f)] float frequency = 1;
    [SerializeField][Range(0.01f,2.5f)] float octaves = 4;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);
        initNoise();

        QuadRenderer = GetComponent<Renderer>();
        QuadRenderer.material.mainTexture = GenerateTexture();
    }


    void Update()
    {
        QuadRenderer.material.mainTexture = GenerateTexture();
    }


    private void initNoise()
    {
        _Noise1D = new float[noiseQuality];
        for (int i = 0; i < noiseIterator; i++)
        {
            _Noise1D[i] = Random.value;
        }

        // 2D noise Initializer
        _Noise2D = new float[noiseQuality, noiseQuality];
        for (int noiseX = 0; noiseX < noiseIterator; noiseX++)
        {
            for (int noiseY = 0; noiseY < noiseIterator; noiseY++)
            {
                _Noise2D[noiseX, noiseY] = Random.value;
            }
        }
    }


    private Texture2D GenerateTexture()
    {
        texture = new Texture2D(TextureScale, TextureScale);

        for (int x = 0; x < TextureScale; x++)
        {
            for (int y = 0; y < TextureScale; y++)
            {
                float fractal = FractalSum(x, y, octaves, frequency, 1);

                texture.SetPixel(x, y, new Color(fractal, fractal, fractal));
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return texture;
    }


    static float perlinLerp(float x1, float x2, float i)
    {
        return (1 - i) * x1 + i * x2;
    }

    static float perlinCosineLerp(float x1, float x2, float i)
    {
        return ((Mathf.Cos(Mathf.PI * i) + 1) / 2) * x1 + ((1 - Mathf.Cos(Mathf.PI * i)) / 2 * x2);
    }

    // Smooths any normalized value (0 to 1).
    static float Smooth(float value)
    {
        return Mathf.Clamp(Mathf.Pow(value, 2) * 3 - Mathf.Pow(value, 3) * 2,0,1);
    }


    // Calculates a 1D noise value using a specified x coordinate.
    private float Noise1D(float xPos)
    {
        int flooredX = (int)Mathf.Floor(xPos);

        float noiseBehind   = _Noise1D[flooredX % noiseIterator];
        float noiseInFront  = _Noise1D[(flooredX + 1) % noiseIterator];

        float Interpolation = xPos - flooredX;

        float noiseValue = perlinLerp(noiseBehind, noiseInFront, Interpolation);

        return noiseValue;
    }


    // Iterates through multiple noise functions and tallies up the result, then normalises it by the theoretically highest value possible.
    private float FractalSum(float xPos, float yPOs, float octaves, float frequency, float scale)
    {
        float sum = 0;
        float highestSum = 0;

        for (int i = 0; i < octaves; i++) {

            sum += Noise2D(xPos * frequency, yPOs * frequency) * scale;

            highestSum += scale;
            frequency /= 0.5f;
            scale /= 2f;
        }

        return sum / highestSum;
    }


    [SerializeField] bool smoothStepEnabled;
    [SerializeField] bool cosineLerp;
    // Calculates a 2D noise value using a specified x and y coordinate.
    private float Noise2D(float xPos, float yPos)
    {
        int flooredX = (int)Mathf.Floor(xPos);
        int flooredY = (int)Mathf.Floor(yPos);

        float bottomLeftCorner  = _Noise2D[ flooredX % noiseIterator,        flooredY % noiseIterator];
        float bottomRightCorner = _Noise2D[(flooredX + 1) % noiseIterator,   flooredY % noiseIterator];
        float topLeftCorner     = _Noise2D[ flooredX % noiseIterator,        (flooredY + 1) % noiseIterator];
        float topRightCorner    = _Noise2D[(flooredX + 1) % noiseIterator,   (flooredY + 1) % noiseIterator];

        float InterpolatedX = xPos - flooredX;
        float InterpolatedY = yPos - flooredY;

        if (smoothStepEnabled) {
            InterpolatedX = Smooth(xPos - flooredX);
            InterpolatedY = Smooth(yPos - flooredY);
        }

        float xLerp = perlinLerp(bottomLeftCorner, bottomRightCorner, InterpolatedX);
        float yLerp = perlinLerp(topLeftCorner, topRightCorner, InterpolatedX);

        if (cosineLerp)
        {
            xLerp = perlinCosineLerp(bottomLeftCorner, bottomRightCorner, InterpolatedX);
            yLerp = perlinCosineLerp(topLeftCorner, topRightCorner, InterpolatedX);

            return perlinCosineLerp(xLerp, yLerp, InterpolatedY);
        }

        return perlinLerp(xLerp, yLerp, InterpolatedY);
    }
}
