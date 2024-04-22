using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    public Vertex[] vertexElements = { };
    MeshManager meshManager;
    public string type;

    public void Initialize(IEnumerable<Vertex> vertexElements, Color color, MeshManager meshManager, string type) {
        this.meshManager = meshManager;
        this.vertexElements = vertexElements.ToArray();
        this.type = type;
        GetComponent<MeshFilter>().mesh = mesh = new();
        mesh.Clear();
        UpdateVertices();
        UpdateTriangles();
        GetComponent<MeshRenderer>().material.color = color;
    }   

    // Update is called once per frame
    void Update()
    {
        UpdateVertices();
    }


    void UpdateVertices() {
        Vector3[] vertices = new Vector3[vertexElements.Length+1];
        Vector3 vectorSum = Vector3.zero;
        for(int i = 0; i < vertexElements.Length; i++) {
            if (vertexElements[i] == null) {
                Debug.Log($"A Vertex of mesh {name} was destroyed. Destroying the mesh as well!");
                SelfDestroy();
                return;
            }
            var position = vertexElements[i].transform.position;
            vectorSum += position;
            vertices[i+1] = position;
        }
        vertices[0] = vectorSum / vertexElements.Length;
        mesh.vertices = vertices;
    }

    void SelfDestroy() {
        meshManager.RemoveMesh(this);
        Destroy(gameObject);
    }

    void UpdateTriangles() {
        int[] triangles = new int[2 * 3 * vertexElements.Length];
        triangles[0] = 0;
        triangles[1] = vertexElements.Length;
        triangles[2] = 1;
        triangles[3*vertexElements.Length] = 0;
        triangles[3*vertexElements.Length+1] = 1;
        triangles[3*vertexElements.Length+2] = vertexElements.Length; 

        for(int i = 1; i < vertexElements.Length; i++) {
            triangles[3*i] = 0;
            triangles[3*i+1] = i;
            triangles[3*i+2] = i+1;
            triangles[3*vertexElements.Length + 3*i] = 0;
            triangles[3*vertexElements.Length + 3*i+1] = i+1;
            triangles[3*vertexElements.Length + 3*i+2] = i;
        }
        mesh.triangles = triangles;
    }

    
}
