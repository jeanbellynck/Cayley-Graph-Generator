using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * This class is used to manage the graph. It is responsible for storing the vertices and edges and for keeping track of the idCounter.   
 * It does not contain any logic for the forces or the algebra of the graph.
 */
public class GraphManager : MonoBehaviour {
    private int idCounter = 0; // Starts by 1 as 0 is reserved for the neutral element
    readonly List<Vertex> vertices = new();
    readonly List<Edge> edges = new();
    public Edge.SplinificationType splinificationType { get; protected set; } = Edge.SplinificationType.WhenSimulationHasStopped;
    IActivityProvider activityProvider;
    public int LabelCount { get; private set; }

    public Dictionary<char, Color> labelColors = new();
    [SerializeField] Color[] ColorList = { new(1, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) };

    [SerializeField] GroupColorPanel groupColorPanel;
    [SerializeField] List<Kamera> kameras;


    public float Activity => activityProvider.Activity;
    //public Kamera kamera { get; protected set; }

    public void Initialize(IEnumerable<char> generators, IActivityProvider activityProvider) { 
        this.activityProvider = activityProvider;

        UpdateLabels(generators);
    }

    public void UpdateLabels(IEnumerable<char> generators)
    { 

        labelColors = new(Enumerable.Zip(
            generators, 
            ColorList.Extend(
                () => Random.ColorHSV(0, 1, 0.9f, 1)
            ), (generator, color) => new KeyValuePair<char, Color>(generator, color)
        ));
        LabelCount = 2 * labelColors.Count;
        groupColorPanel.updateView(labelColors);

    }

    public List<Vertex> getVertices() {
        return vertices;
    }

    // referred to from event (UI)
    public void SetSplinificationMode(int t) {
        splinificationType = (Edge.SplinificationType) t;
    }

    public void AddVertex(Vertex vertex) {
        if (vertices.IsEmpty())
            foreach (var kamera in kameras) 
                kamera.center = vertex.transform;

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
        edge.graphManager = this;
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