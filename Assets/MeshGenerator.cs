using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vertex[] vertexElements = { };
    
    public void Initialize(IEnumerable<Vertex> vertexElements) {
        this.vertexElements = vertexElements.ToArray();
        GetComponent<MeshFilter>().mesh = mesh = new();
        mesh.Clear();
        UpdateVertices();
        UpdateTriangles();
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
            vectorSum += vertexElements[i].transform.position;
            vertices[i+1] = vertexElements[i].transform.position;
        }
        vertices[0] = vectorSum / vertexElements.Length;
        mesh.vertices = vertices;
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
