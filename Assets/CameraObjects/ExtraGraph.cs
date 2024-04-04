using System.Collections.Generic;
using System.Linq;
using DanielLochner.Assets.SimpleSideMenu;
using TMPro;
using UnityEngine;
using TaggedGraph = QuikGraph.UndirectedGraph<string, QuikGraph.TaggedEdge<string, EdgeData>>;

public class ExtraGraph : MonoBehaviour
{
    [SerializeField] GraphVisualizer graphVisualizer;
    [SerializeField] Physik physik;
    
    [SerializeField] GameObject vertexPrefab;
    [SerializeField] GameObject edgePrefab;
    [SerializeField] GeneratorMenu generatorMenu;
    [SerializeField] TMP_InputField vertexCountInput;
    [SerializeField] TMP_InputField edgeCountInput;
    [SerializeField] TMP_InputField proportionOfGeneratorsInput;
    [SerializeField] List<Kamera> cameras;
    [SerializeField] SimpleSideMenu normalSideMenu;
    [SerializeField] RelatorMenu normalRelatorMenu;
    [SerializeField] GeneratorMenu normalGeneratorMenu;
    [SerializeField] TaggedGraph graph;

    void Start() {
        generatorMenu.OnGeneratorsChanged += () => graphVisualizer.UpdateLabels(generatorMenu.Generators.ToArray());
    }

    public void StartVisualization() {
        if (!int.TryParse(vertexCountInput.text, out int vertexCount) || vertexCount < 0 || vertexCount > 150) return;
        if (!int.TryParse(edgeCountInput.text, out int edgeCount) || edgeCount < 0 || edgeCount > 500) return;
        if (!double.TryParse(proportionOfGeneratorsInput.text.FixDecimalPoint(), out double proportionOfGenerators) || proportionOfGenerators < 0 || proportionOfGenerators > 3) return;
        VisualizeRandomGraph(vertexCount, edgeCount, proportionOfGenerators);
    }

    public void VisualizeRandomGraph(int vertexCount, int edgeCount, double proportionOfGenerators) {
        var (generatorStrings, graph) = RandomGroups.RandomGraphWithEdgeWords(vertexCount, edgeCount, proportionOfGenerators);
        var generators = generatorMenu.Generators = 
            from generator in generatorStrings select generator.DefaultIfEmpty('?').First();
        DrawGraph(generators, graph);
        physik.startUp(graphVisualizer.graphManager, 3, generatorStrings.Length);
        physik.shutDown();
    }

    void DrawGraph(IEnumerable<char> generators, TaggedGraph graph) {
        graphVisualizer.Initialize(generators.ToArray(), physik);
        graphVisualizer.SetSplinificationMode((int)Edge.SplinificationType.Always);
        TaggedGraphToGraphManager(graph);
        Debug.Log(graph.ToStringF());
    }

    void TaggedGraphToGraphManager(TaggedGraph graph)
    {
        this.graph = graph;
        graphVisualizer.graphManager.ResetGraph();
        var vertexDict = new Dictionary<string, Vertex>();
        foreach (var vertexName in graph.Vertices) {
            var newVertex = vertexDict[vertexName] = Instantiate(vertexPrefab, transform).GetComponent<Vertex>();
            newVertex.Initialize(VectorN.Random(3, 10), graphVisualizer.graphManager);
            newVertex.name = "Node " + vertexName;
            graphVisualizer.graphManager.AddVertex(newVertex);
        }
        foreach (var edge in graph.Edges) {
            var label = edge.Tag?.generator.DefaultIfEmpty('?').First() ?? '?';
            Vertex startPoint = vertexDict[edge.Source];
            Vertex endPoint = vertexDict[edge.Target];
            bool reverseOrientation = (edge.Tag?.start == edge.Target);
            bool reverseLabel = char.IsUpper(label);
            if ( reverseLabel )
                label = RelatorDecoder.invertGenerator(label);

            if ( reverseOrientation != reverseLabel )
                // xor (minus mal minus)
                (startPoint, endPoint) = (endPoint, startPoint);
    
            var newEdge = Instantiate(edgePrefab, transform).GetComponent<Edge>();
            newEdge.Initialize(startPoint, endPoint, label);
            graphVisualizer.graphManager.AddEdge(newEdge);
        }
    }

    TaggedGraph GraphManagerToTaggedGraph() {
        graph = new TaggedGraph();
        foreach (var vertex in graphVisualizer.graphManager.getVertices()) {
            graph.AddVertex(vertex.name);
        }

        foreach (var edge in graphVisualizer.graphManager.GetEdges()) {
            graph.AddEdge(new(edge.StartPoint.name, edge.EndPoint.name,
                new() { generator = edge.Label.ToString(), start = edge.StartPoint.name }));
        }
        return graph;
    }

    public void GetRelators() {
        TaggedGraph graph = GraphManagerToTaggedGraph();
        var relatorsFromGraph = RandomGroups.RelatorsFromGraph(graph);
        normalRelatorMenu.SetRelators(relatorsFromGraph);
        normalGeneratorMenu.SetGenerators(generatorMenu.Generators);
        normalSideMenu.Open();
    }
}
