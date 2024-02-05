using System.Collections.Generic;
using UnityEngine;

/**
 * This class is used to manage the graph. It is responsible for storing the vertices and edges and for keeping track of the idCounter.   
 * It does not contain any logic for the forces or the algebra of the graph.
 */
public class GraphManager : MonoBehaviour {
    private int idCounter = 0; // Starts by 1 as 0 is reserved for the neutral element
    List<Vertex> vertices = new List<Vertex>();
    List<Edge> edges = new List<Edge>();

    public void Initialize(char[] generators) {
    }

    public ICollection<Vertex> getVertex() {
        return vertices;
    }

    public void AddVertex(Vertex vertex) {
        vertex.Id = idCounter;
        idCounter++;
        vertices.Add(vertex);
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

    public void AddEdge(Edge edge) {
        edges.Add(edge);
    }

    /** 
     * ToDo: Only remove the edge, delete will be done from cayleyGraphMaker it 
     **/
    public void RemoveVertex(Vertex vertex) {
        foreach (List<Edge> genEdges in vertex.LabeledIncomingEdges.Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                RemoveEdge(edge);
            }
        }
        foreach (List<Edge> genEdges in vertex.LabeledOutgoingEdges.Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                RemoveEdge(edge);
            }
        }
        vertices.Remove(vertex);
    }

    public void RemoveEdge(Edge edge) {
        edges.Remove(edge);
    }


    public List<Edge> GetEdges() {
        return edges;
    }
}