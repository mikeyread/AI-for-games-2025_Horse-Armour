using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;


public class ObjectInstancing : MonoBehaviour
{
    [SerializeField] int grassRadius;
    int prevRadius;
    private int index;

    private ComputeShader _GrassField;

    private ComputeBuffer b_GrassPositions;

    private Vector3[] o_GrassPositions;

    private void Awake()
    {
        b_GrassPositions?.Dispose();
        _GrassField = Instantiate(Resources.Load<ComputeShader>("CS_ChunkObjects"));
        
        index = grassRadius * grassRadius;
        o_GrassPositions = new Vector3[index];
    }

    private void Update()
    {
        if (grassRadius != prevRadius)
        {
            prevRadius = grassRadius;
            index = grassRadius * grassRadius;
            o_GrassPositions = new Vector3[index];
        }
        if (grassRadius <= 0) return;

        b_GrassPositions = new ComputeBuffer(index, sizeof(float) * 3);

        _GrassField.SetInt("size", grassRadius);
        _GrassField.SetVector("playerPos", Camera.main.transform.position);
        _GrassField.SetBuffer(0, "positions", b_GrassPositions);

        _GrassField.Dispatch(0, index / 32, index / 32, 1);

        b_GrassPositions.GetData(o_GrassPositions);

        b_GrassPositions?.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (o_GrassPositions == null) return;

        foreach (var pos in o_GrassPositions) {
            Gizmos.DrawWireCube(pos, Vector3.one / 2);
        }
    }
}
