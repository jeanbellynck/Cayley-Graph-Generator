using System;
using System.Collections.Generic;
using System.Linq;
using SharpGraph;
using UnityEngine;

/**
 * This class is used to manage the graph. It is responsible for storing the vertices and edges and for keeping track of the idCounter.   
 * It does not contain any logic for the forces or the algebra of the graph.
 *
 * The edges are stored in a dictionary, sorted by the label of the edge. This is done to make it easier to find the edges of a certain label.
 * (It also makes it possible to access the edges of a subgroup)
 *
 * ToDo: Move all of the visual stuff to a new class.
 */
public class LabelledGraphManager {
    private int idCounter = 0; // Starts by 1 as 0 is reserved for the neutral element
    readonly List<Vertex> vertices = new();
    readonly Dictionary<char, List<Edge>> edges = new(); // The edges are stored in a dictionary, sorted by the label of the edge. This is done to make it easier to find the edges of a certain label. (It also makes it possible to access the edges of a subgroup)
    //readonly List<Edge> edges = new();
    public int LabelCount { get; set; }

    public delegate void OnEdgeAdded(Edge edge);
    public event OnEdgeAdded onEdgeAdded;
    public event Action<Vertex, Kamera> OnCenterChanged;

    public List<Vertex> GetVertices() {
        return vertices;
    }


    public void AddVertex(Vertex vertex) {
        vertex.Id = idCounter;
        idCounter++;
        vertices.Add(vertex);
        //vertex.graphVisualizer = this;
        vertex.OnCenter += kamera => OnCenterChanged?.Invoke(vertex, kamera); // Stupid workaround to allow vertex to indirectly change the kamera's centerPointer, since now the vertex has no reference to the kameras anymore.
    }

    public void ResetGraph() {
        foreach (Vertex vertex in new List<Vertex>(vertices)) vertex.Destroy();
        vertices.Clear();
        edges.Clear();
        idCounter = 1;
    }

    /**
     * Adds an edge to the graph. The edge is added to the list of edges and to the dictionary of edges, sorted by the label of the edge.
     */
    public void AddEdge(Edge edge) {
        if (!edges.ContainsKey(edge.Label)) edges[edge.Label] = new List<Edge>();
        edges[edge.Label].Add(edge);
        onEdgeAdded?.Invoke(edge);
    }

    public void RemoveVertex(Vertex vertex) {
        foreach (HashSet<Edge> genEdges in vertex.LabeledIncomingEdges.Values) {
            HashSet<Edge> genEdgesCopy = new HashSet<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                RemoveEdge(edge);
            }
        }
        foreach (HashSet<Edge> genEdges in vertex.LabeledOutgoingEdges.Values) {
            HashSet<Edge> genEdgesCopy = new HashSet<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                RemoveEdge(edge);
            }
        }
        vertices.Remove(vertex);
        vertex.Destroy();
    }

    public void RemoveEdge(Edge edge) {
        edges[edge.Label].Remove(edge);
        edge.Destroy();
    }

    public void RemoveEdges(char label) {
        if (!edges.ContainsKey(label)) return;
        foreach (Edge edge in edges[label]) edge.Destroy();
        edges.Remove(label);
    }


    public List<Edge> GetEdges(char label) {
        return edges[label];
    }

    public List<Edge> GetEdges() {
        List<Edge> result = new();
        foreach (List<Edge> edgeList in this.edges.Values) {
            result.AddRange(edgeList);
        }
        return result;
    }

    public int getDim() => vertices.Count > 0 ? vertices[0].Position.Size() : 0;

    public int GetVertexCount() => vertices.Count;

    internal void DeleteSubgraph() {
        throw new NotImplementedException();
    }
}