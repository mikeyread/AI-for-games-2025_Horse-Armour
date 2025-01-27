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
    // https://stackoverflow.com/questions/58241309/how-to-continuously-generate-perlin-noise-as-an-infinite-map-grows
    // https://www.youtube.com/watch?v=jv6YT9pPIHw
    void PerlinNoise(Vector2 normalCoord)
    {
        /*
        float brScalar = Vector2.Dot(normalCoord, perlinVector[0]);
        float blScalar = Vector2.Dot(normalCoord, perlinVector[1]);
        float trScalar = Vector2.Dot(normalCoord, perlinVector[2]);
        float tlScalar = Vector2.Dot(normalCoord, perlinVector[3]);
        */
    }

    // Rotates a one length vector for use in Perlin noise.
    // Will likely need to scrap as this does not factor in a seed.
    private static Vector2 normalisedRandomVector()
    {
        float angle = Random.Range(0, math.PI * 2);

        float rotX = 0 * math.cos(angle) - 1 * math.sin(angle);
        float rotY = 0 * math.sin(angle) + 1 * math.cos(angle);

        return new Vector2(rotX, rotY);
    }

    /*
    private Vector2 normalisedCoordinate(float x, float z)
    {
        // Calculate bounds

        float normalX = (float)x / (float)width;
        float normalZ = (float)z / (float)length;
        Debug.Log("Map X fraction: " + normalX + " Map Z fraction: " + normalZ);

        return new Vector2(normalX, normalZ);
    }
    */

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
        GeneratePerlin();
    }


    // Just dynamically updates the mesh if the Serialized fields are changed during runtime.
    private void Update()
    {
        if (!prevLength.Equals(length) || !prevWidth.Equals(width) || !prevSize.Equals(size))
        { 
            Debug.Log("Change Detected: Refreshing Mesh");

            GenerateMesh();
            GeneratePerlin();
            prevLength = length;
            prevWidth = width;
            prevSize = size;
        }
    }


    // We generate a mesh grid of quads using pre-defined length and width.
    private void GenerateMesh()
    {

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

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.normals = normals.ToArray();
        terrain.uv = uv.ToArray();
        terrain.triangles = triangles.ToArray();

        terrain.RecalculateBounds();
    }


    // Generates coordinates of a Perlin grid, will likely scrap as Perlin should be a virtual infinite grid that isn't bound within an area.
    private void GeneratePerlin()
    {
        if (vertices == null) return;

        // Calculates the bounding area for the perlin grid
        int lastIndex = vertices.Count - 1;

        Vector3 bounds = new Vector3(Mathf.Abs(vertices[0].x), Mathf.Abs(vertices[0].y), Mathf.Abs(vertices[0].z)) + new Vector3(Mathf.Abs(vertices[lastIndex].x), Mathf.Abs(vertices[lastIndex].y), Mathf.Abs(vertices[lastIndex].z));
        Debug.Log("Bound Size: " + bounds);
    }




    private void OnDrawGizmos()
    {
        if (vertices == null) return;
        if (perlinVector == null) return;


        /* Rendering the perlin corner vectors to check if they are actually random
        
        Vector3 localPerlinBL = vertices[0] + new Vector3(perlinVector[0].x, 0, perlinVector[0].y);
        Vector3 localPerlinBR = vertices[width] + new Vector3(perlinVector[1].x, 0, perlinVector[1].y);
        Vector3 localPerlinTL = vertices[(width + 1) * (length + 1) - (width + 1)] + new Vector3(perlinVector[2].x, 0, perlinVector[2].y);
        Vector3 localPerlinTR = vertices[(width + 1) * (length + 1) - 1] + new Vector3(perlinVector[3].x, 0, perlinVector[3].y);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(vertices[0], localPerlinBL);
        Gizmos.DrawLine(vertices[width], localPerlinBR);
        Gizmos.DrawLine(vertices[(width + 1) * (length + 1) - (width + 1)], localPerlinTL);
        Gizmos.DrawLine(vertices[(width + 1) * (length + 1) - 1], localPerlinTR);
        */
    }
}