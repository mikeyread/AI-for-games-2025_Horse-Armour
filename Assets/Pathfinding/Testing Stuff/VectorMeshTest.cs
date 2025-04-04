using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class NewBehaviourScript : MonoBehaviour
{
    public UnityEngine.Vector3[] vertices;
    int[] indices;
    Mesh mesh;


    private void Awake()
    {
        // Initialize in the Awake function
        vertices = new UnityEngine.Vector3[4];
        indices = new int[6];
        mesh = new Mesh();

        CreateMesh();
        this.AddComponent<MeshFilter>();
        this.AddComponent<MeshRenderer>();
        this.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

    }
    void CreateMesh ()
    {
        
        vertices[0] = new Vector3(-1, 0, -1) + this.transform.position;
        vertices[1] = new Vector3(1, 0, -1) + this.transform.position;
        vertices[2] = new Vector3(-1, 0, 1) + this.transform.position;
        vertices[3] = new Vector3(1, 0, 1) + this.transform.position;

        indices[0] = 1;
        indices[1] = 0;
        indices[2] = 2;
        indices[3] = 2;
        indices[4] = 3;
        indices[5] = 1;

        mesh.vertices = vertices;
        mesh.triangles = indices;

        this.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
