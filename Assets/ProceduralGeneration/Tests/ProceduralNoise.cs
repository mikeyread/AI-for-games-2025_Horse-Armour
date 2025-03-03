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
    [SerializeField] bool turbulent = false;
    [SerializeField] bool normalized = false;

    private float settingCheck;
    private float settingSum;

    private bool turblentCheck;
    private bool normalizedCheck;


    private void Awake()
    {
        if (seed != 0) Random.InitState(seed);

        QuadRenderer = GetComponent<Renderer>();
        QuadRenderer.material.mainTexture = GenerateTexture();
    }


    void FixedUpdate()
    {
        if (Reset)
        {
            octaves = 8;
            frequency = 0.033f;
            amplitude = 1f;
            persistence = 0.5f;
            lacurnity = 2f;

            turbulent = false;
            normalized = false;

            Reset = false;
        }

        settingSum = octaves + frequency + amplitude + persistence + lacurnity;

        if (settingCheck == settingSum && turblentCheck == turbulent && normalizedCheck == normalized) return;
        else {
            settingCheck = settingSum;
            turblentCheck = turbulent;
            normalizedCheck = normalized;
        }


        QuadRenderer.material.mainTexture = GenerateTexture();
    }

    private Texture2D GenerateTexture()
    {
        texture = new Texture2D(TextureScale, TextureScale);

        for (int x = 0; x < TextureScale; x++)
        {
            for (int y = 0; y < TextureScale; y++)
            {
                float fractal = NoiseExperimentation(x, y);

                texture.SetPixel(x, y, new Color(fractal, fractal, fractal));
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();

        return texture;
    }

    public float NoiseExperimentation(float x, float y)
    {
        float cloudy = PerlinNoise2D.PerlinNoise(x, y, -2183, 8, 0.1f, 1f, 0.5f, 2f, false, true);

        return PerlinNoise2D.SmootherStep(Mathf.Pow(2, cloudy) - 1);
    }
}
