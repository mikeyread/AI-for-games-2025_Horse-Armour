using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class computerShaderTester : MonoBehaviour
{
    public ComputeShader _ComputeShader;
    public RenderTexture _Texture;

    [SerializeField][Range(8,PerlinNoise2D.noiseQuality)] public int resolution = 256;


    public void Update()
    {
        NoiseHashTexture();
        _Texture.DiscardContents();
    }


    void NoiseHashTexture()
    {
        if (_Texture == null || _Texture.width != resolution)
        {
            _Texture = new RenderTexture(resolution, resolution, 24);
            _Texture.enableRandomWrite = true;
            _Texture.Create();
        }


        ComputeBuffer hashBuffer = new ComputeBuffer(PerlinNoise2D.noiseQuality * PerlinNoise2D.noiseQuality, sizeof(float));
        hashBuffer.SetData(PerlinNoise2D._Noise2D);

        _ComputeShader.SetTexture(0, "Result", _Texture);
        _ComputeShader.SetBuffer(0, "hash", hashBuffer);
        _ComputeShader.Dispatch(0, _Texture.width, _Texture.height, 1);

        // Assign the compute result to the material.
        if (GetComponent<MeshRenderer>().material == null)
        {
            this.AddComponent<MeshRenderer>();
        }

        _Texture.filterMode = FilterMode.Point;
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = _Texture;

        hashBuffer.Dispose();
    }
}
