using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GraphManager : MonoBehaviour {

    public GameObject vertexPrefab;
    public GameObject edgePrefab;
    public Color[] colourList = new Color[] { new Color(255, 0, 0), new Color(0, 0, 255), new Color(0, 255, 0), new Color(255, 255, 0) };
    IDictionary<char, Color> operationColors;
    private int idCounter; // Starts by 1 as 0 is reserved for the neutral element
    public GameObject neutralElement;
    List<Vertex> vertices = new List<Vertex>();
    List<Edge> edges = new List<Edge>();

    public void Initialize(char[] generators) {
        operationColors = new Dictionary<char, Color>();
        for (int i = 0; i < generators.Length; i++) {
            if (i < colourList.Length) {
                operationColors.Add(generators[i], colourList[i]);
            }
            else {
                operationColors.Add(generators[i], new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255)));
            }

        }
        vertices.Add(neutralElement.GetComponent<Vertex>());
    }

    public ICollection<Vertex> getVertex() {
        return vertices;
    }

    public void AddVertex(Vertex vertex) {
        vertex.id = idCounter;
        idCounter++;
        vertices.Add(vertex);
    }

    public Vertex getNeutral() {
        return neutralElement.GetComponent<Vertex>();
    }

    public void ResetGraph() {
        List<Vertex> verticesCopy = new List<Vertex>(vertices); 
        foreach (Vertex vertex in verticesCopy) {
            RemoveVertex(vertex);
        }
        vertices.Clear();
        edges.Clear();
        idCounter = 1;
    }


    public ICollection<Edge> GetKanten() {
        return edges;
    }

    /**public Edge GetEdge(string von, string zu)
    {
        return edges[(von, zu)];
    }**/

    public void AddEdge(Edge edge) {
        edges.Add(edge);
    }

    public void RemoveVertex(Vertex vertex) {
        vertex.isActive = false;
        foreach (List<Edge> genEdges in vertex.GetEdges().Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                RemoveEdge(edge);
            }
        }
        if(vertex.Equals(neutralElement.GetComponent<Vertex>())) {
            vertex.transform.position = Vector3.zero;
            vertex.age = 0;
        }else {
            vertices.Remove(vertex);
            vertex.Destroy();
        }
    }

    public void RemoveEdge(Edge edge) {
        edges.Remove(edge);
        edge.Destroy();
    }

    public Vertex followEdge(Vertex vertex, char op) {
        return vertex.FollowEdge(op);
    }

    public Vertex CreateVertex(Vector3 position) {
        Vertex newVertex = Instantiate(vertexPrefab, position, Quaternion.identity, transform).GetComponent<Vertex>();
        AddVertex(newVertex);
        newVertex.name = "";
        newVertex.distanceToNeutralElement = 0;
        return newVertex;
    }

    public Edge CreateEdge(Vertex startvertex, Vertex endvertex, char op) {
        // If the edge already exists, no edge is created and the existing edge is returned
        foreach (Edge edge in startvertex.GetEdges(op)) {
            if(edge.getOpposite(startvertex).Equals(endvertex)) {
                return edge;
            }
        }

        Edge newEdge = Instantiate(edgePrefab, transform).GetComponent<Edge>();
        newEdge.SetFarbe(operationColors[char.ToLower(op)], new Color(100, 100, 100));
        newEdge.SetEndpoints(startvertex, endvertex, op);
        AddEdge(newEdge);
        return newEdge;
    }
}