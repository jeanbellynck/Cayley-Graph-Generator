using System;
using System.Collections.Generic;
using System.Linq;
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
    readonly List<Edge> edges = new();
    public int LabelCount { get; set; }

    public delegate void OnEdgeAdded(Edge edge);
    public event OnEdgeAdded onEdgeAdded;

    public List<Vertex> getVertices() {
        return vertices;
    }


    public void AddVertex(Vertex vertex) {
        vertex.Id = idCounter;
        idCounter++;
        vertices.Add(vertex);
        vertex.graphManager = this;
    }

    public void ResetGraph() {
        foreach (Vertex vertex in new List<Vertex>(vertices)) vertex.Destroy();
        vertices.Clear();
        edges.Clear();
        idCounter = 1;
    }

    public void AddEdge(Edge edge) {
        edges.Add(edge);
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
    }

    public void RemoveEdge(Edge edge) {
        edges.Remove(edge);
    }


    public List<Edge> GetEdges() {
        return edges;
    }

    public int getDim() => vertices.Count > 0 ? vertices[0].Position.Size() : 0;

    public int GetVertexCount() => vertices.Count;
}