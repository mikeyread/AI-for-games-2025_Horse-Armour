using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class TerrainGenerator : MonoBehaviour
{
    [Tooltip("Sets the amount of vertices along the width of the grid")]
    [SerializeField][Range(2, 256)] private int width = 256;
    [Tooltip("Sets the amount of vertices along the length of the grid")]
    [SerializeField][Range(2,256)] private int height = 256;
    [Tooltip("Sets the distance between vertices")]
    [SerializeField][Range(0.001f, 1)] float scale = 0.5f;

    [SerializeField][Range(0.001f, 1)] float blWeight = 1;
    [SerializeField][Range(0.001f, 1)] float brWeight = 1;
    [SerializeField][Range(0.001f, 1)] float tlWeight = 1;
    [SerializeField][Range(0.001f, 1)] float trWeight = 1;

    float blPreviousWeight = 1;
    float brPreviousWeight = 1;
    float tlPreviousWeight = 1;
    float trPreviousWeight = 1;


    private static Vector2 randomVector()
    {
        Vector2 unitVector = new Vector2(0, 1);
        float random = Random.Range(0, math.PI * 2);
        
        unitVector.x = unitVector.x * Mathf.Cos(random) - unitVector.y * Mathf.Sin(random);
        unitVector.y = unitVector.x * Mathf.Sin(random) + unitVector.y * Mathf.Cos(random);

        return unitVector;
    }


    private static Vector3[,] GenerateGrid(int gridWidth, int gridHeight, float scalar)
    {
        // Column [ z ], Row [ x ]
        Vector3[,] Grid = new Vector3[gridHeight, gridWidth];

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Grid[z,x] = new Vector3((float)x * scalar, scalar, (float)z * scalar);
            }
        }
        return Grid;
    }


    private static Vector2[,] normaliseCoords(int gridWidth, int gridHeight)
    {
        Vector2[,] Grid = new Vector2[gridHeight, gridWidth];

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Grid[z, x] = new Vector2((float)x / gridWidth, (float)z / gridHeight);
            }
        }

        return Grid;
    }

    private static float Perlin(Vector2 normal, Vector2[] PerlinCorners)
    {

        float BLScaler = Vector2.Dot(normal, PerlinCorners[0]);
        float BRScaler = Vector2.Dot(normal, PerlinCorners[1]);
        float TLScaler = Vector2.Dot(normal, PerlinCorners[2]);
        float TRScaler = Vector2.Dot(normal, PerlinCorners[3]);

        //Debug.Log("Scalers BL: " + BLScaler + " BR: " + BRScaler + " TL: " + TLScaler + " TR: " + TRScaler);
        return BLScaler * BRScaler * TLScaler * TRScaler;
    }

    private Vector3[,] quadGrid;
    private Vector2[,] normalisedGrid;
    private Vector2[] PerlinCorners;
    private Vector2[] PerlinWeightedCorners;

    private void Start()
    {
        PerlinCorners = new Vector2[] {
            randomVector(),
            randomVector(),
            randomVector(),
            randomVector()
        };

        Generate();
    }

    private void Update()
    {
        if (!blPreviousWeight.Equals(blWeight) || !brPreviousWeight.Equals(brWeight) || !tlPreviousWeight.Equals(tlWeight) || !trPreviousWeight.Equals(trWeight))
        {
            Generate();
        }
    }

    private void Generate()
    {
        blPreviousWeight = blWeight;
        brPreviousWeight = brWeight;
        tlPreviousWeight = tlWeight;
        trPreviousWeight = trWeight;

        PerlinWeightedCorners = new Vector2[]
        {
            Vector2.Scale(PerlinCorners[0], new Vector2(blWeight,blWeight)),
            Vector2.Scale(PerlinCorners[1], new Vector2(brWeight,brWeight)),
            Vector2.Scale(PerlinCorners[2], new Vector2(tlWeight,tlWeight)),
            Vector2.Scale(PerlinCorners[3], new Vector2(trWeight,trWeight))
        };

        quadGrid = GenerateGrid(width, height, scale);
        normalisedGrid = normaliseCoords(width, height);

        for (int z = 0; z < quadGrid.GetLength(0); z++)
        {
            for (int x = 0; x < quadGrid.GetLength(1); x++)
            {
                quadGrid[z, x] = new Vector3(quadGrid[z, x].x, Perlin(normalisedGrid[z, x], PerlinWeightedCorners) * 5, quadGrid[z, x].z);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (quadGrid == null || normalisedGrid == null || PerlinCorners == null) return;

        Vector3 position = this.transform.localPosition;

        for (int z = 0; z < quadGrid.GetLength(0); z++)
        {
            for (int x = 0; x < quadGrid.GetLength(1); x++)
            {
                Gizmos.DrawLine(
                    new Vector3(quadGrid[z, x].x, 0, quadGrid[z, x].z) + position,
                    new Vector3(quadGrid[z,x].x, quadGrid[z, x].y, quadGrid[z, x].z) + position
                );
            }
        }
    }
}
