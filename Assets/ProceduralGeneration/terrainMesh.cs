using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class terrainMesh : MonoBehaviour {

    Mesh terrain;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uv = new List<Vector2>();
    private List<int> triangles = new List<int>();

    private List<Vector2> perlinVector = new List<Vector2>();

    [SerializeField]int length = 10;
    [SerializeField]int width = 10;
    [SerializeField]float size = 2.5f;

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

    // Finds the normalised coords of a point on the whole map.
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
        this.GetComponent<MeshRenderer>().material = new Material(Shader.Find("VertexLit"));
        GenerateMesh();
    }

    private void Update()
    {
        // Need to fix this
        if (!prevLength.Equals(length) || !prevWidth.Equals(width) || !prevSize.Equals(size))
        {
            Debug.Log("Change detected, refreshing mesh!");

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

        float zOffset = (length * size) / 2;
        float xOffset = (width * size) / 2;

        int triIndex = 0;
        for (int z = 0; z < length; z++)
        {
            float newZ = z * size - zOffset;

            for (int x = 0; x < width; x++) {
                Vector2 normalCoord = normalisedCoordinate(x, z);

                float newX = x * size - xOffset;
                vertices.Add(new Vector3(newX, 0, newZ));
                vertices.Add(new Vector3(newX, 0, newZ + size));
                vertices.Add(new Vector3(newX + size, 0, newZ));
                vertices.Add(new Vector3(newX + size, 0, newZ + size));

                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(0, 1));
                uv.Add(new Vector2(1, 0));
                uv.Add(new Vector2(1, 1));

                triangles.Add(triIndex);
                triangles.Add(triIndex + 1);
                triangles.Add(triIndex + 2);
                triangles.Add(triIndex + 1);
                triangles.Add(triIndex + 3);
                triangles.Add(triIndex + 2);
                triIndex += 4;
            }
        }

        terrain.Clear();

        terrain.vertices = vertices.ToArray();
        terrain.normals = normals.ToArray();
        terrain.uv = uv.ToArray();
        terrain.triangles = triangles.ToArray();

        terrain.RecalculateBounds();
    }
}
