using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using UnityEditor;
using UnityEngine.UIElements;



[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class terrainMesh : MonoBehaviour {

    // Mesh Parameters
    Mesh terrain;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uv = new List<Vector2>();
    private List<int> triangles = new List<int>();

    // Perlin Vectors
    private List<Vector2> perlinVector = new List<Vector2>();

    // Exceeding 65536 quads will cause issues due to missing meshes, please be careful when exceeding.
    [Tooltip("Assigns the x Width of the procedural Quad Grid")]
    [SerializeField][Range(1, 255)] int width = 64;
    [Tooltip("Assigns the z Length of the procedural Quad Grid")]
    [SerializeField][Range(1, 255)] int length = 64;
    [Tooltip("A scaler for the Grids individual quad meshes")]
    [SerializeField][Range(0.033f, 10f)] float size = 2.5f;


    private int prevLength;
    private int prevWidth;
    private float prevSize;



    // TO BE DONE
    // https://www.cl.cam.ac.uk/teaching/1718/FGraphics/Appendix%20B%20-%20Perlin%20Noise.pdf
    void PerlinNoise(float normalCoord)
    {

    }

    // Rotates a one length vector for use in Perlin noise.
    private static Vector2 normalisedRandomVector()
    {
        float angle = Random.Range(0, math.PI * 2);

        float rotX = 0 * math.cos(angle) - 1 * math.sin(angle);
        float rotY = 0 * math.sin(angle) + 1 * math.cos(angle);

        return new Vector2(rotX, rotY);
    }

    // Finds the normalised coords of a point on the whole map. Will refactor later (maybe).
    private Vector2 normalisedCoordinate(int x, int z)
    {
        float normalX = (float)x / (float)width;
        float normalZ = (float)z / (float)length;
        Debug.Log("Map X fraction: " + normalX + " Map Z fraction: " + normalZ);

        return new Vector2(normalX, normalZ);
    }

    private void OnEnable()
    {
        terrain = new Mesh() { name = "Terrain Mesh" };

        prevWidth = width;
        prevLength = length;
        prevSize = size;
    }

    void Start()
    {
        this.GetComponent<MeshFilter>().mesh = terrain;
        this.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
        GenerateMesh();
    }


    // Just dynamically updates the mesh if the Serialized fields are changed during runtime.
    private void Update()
    {
        if (!prevLength.Equals(length) || !prevWidth.Equals(width) || !prevSize.Equals(size))
        { 
            Debug.Log("Change Detected: Refreshing Mesh");

            GenerateMesh();
            prevLength = length;
            prevWidth = width;
            prevSize = size;
        }
    }


    // We generate a mesh grid of quads using pre-defined length and width.
    private void GenerateMesh()
    {
        perlinVector.Add(normalisedRandomVector());
        perlinVector.Add(normalisedRandomVector());
        perlinVector.Add(normalisedRandomVector());
        perlinVector.Add(normalisedRandomVector());

        vertices.Clear();
        triangles.Clear();
        uv.Clear();
        normals.Clear();

        float xOffset = (width * size) / 2;
        float zOffset = (length * size) / 2;

        
        if (length <= 0 || width <= 0) return;

        for (int vertIndex = 0; vertIndex < (length + 1) * (width + 1); vertIndex++) {
            float xCoord = ((vertIndex % (width + 1)) * size) - xOffset;
            float zCoord = ((vertIndex / (width + 1)) * size) - zOffset;

            vertices.Add(new Vector3(xCoord, 0, zCoord));

            normals.Add(Vector3.up);
            uv.Add(new Vector2(0, 0));

            // We need to determine the walls and cieling of the grid so we don't perform unecessary connections.
            if (vertIndex % (width + 1) != width && vertIndex / (width + 1) != length)
            {
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex);
                triangles.Add(vertIndex + width + 1);

                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + width + 1);
                triangles.Add(vertIndex + width + 2);
            }
        }

        //Debug.Log("Vertices: " + vertices.Count + " Triangles: " + triangles.Count);
        //Debug.Log("Expected Vertices:" + (length + 1) * (width + 1) + " Expected Triangles: " + length * width * 6);

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.normals = normals.ToArray();
        terrain.uv = uv.ToArray();
        terrain.triangles = triangles.ToArray();

        terrain.RecalculateBounds();
    }
}