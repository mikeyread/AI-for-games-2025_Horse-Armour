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

    [SerializeField] bool Reset = false;
    [SerializeField][Range(1,8)] int octaves = 8;
    [SerializeField][Range(0.01f, 2.5f)] float frequency = 0.033f;
    [SerializeField][Range(0.01f, 2.5f)] float amplitude = 1f;
    [SerializeField][Range(0.01f, 2.5f)] float persistence = 0.5f;
    [SerializeField][Range(0.01f, 2.5f)] float lacurnity = 2f;

    [SerializeField] bool powOn = false;
    [SerializeField] bool clampOn = false;
    [SerializeField][Min(0.01f)] float Power = 1f;

    private float settingCheck;
    private float settingSum;

    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);

        QuadRenderer = GetComponent<Renderer>();
        QuadRenderer.material.mainTexture = GenerateTexture();
    }


    void FixedUpdate()
    {
        settingCheck = settingSum;

        if (Reset)
        {
            octaves = 8;
            frequency = 0.033f;
            amplitude = 1f;
            persistence = 0.5f;
            lacurnity = 2f;

            Reset = false;
        }

        settingSum = octaves + frequency + amplitude + persistence + lacurnity;

        if (settingCheck == settingSum) return;

        QuadRenderer.material.mainTexture = GenerateTexture();
    }

    private Texture2D GenerateTexture()
    {
        texture = new Texture2D(TextureScale, TextureScale);

        for (int x = 0; x < TextureScale; x++)
        {
            for (int y = 0; y < TextureScale; y++)
            {
                float fractal;

                if (powOn)
                {
                    fractal = Mathf.Pow(PerlinNoise2D.PerlinNoiseNormal(x, y, 0, octaves, frequency, amplitude, persistence, lacurnity), Power);
                }
                else
                {
                    fractal = PerlinNoise2D.PerlinNoiseNormal(x, y, 0, octaves, frequency, amplitude, persistence, lacurnity);
                }

                if (clampOn)
                {
                    fractal = PerlinNoise2D.PerlinCosineLerp(0, 1, fractal);
                }


                texture.SetPixel(x, y, new Color(fractal, fractal, fractal));
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return texture;
    }
}
