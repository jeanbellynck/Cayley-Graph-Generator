using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using TaggedGraph = QuikGraph.UndirectedGraph<string, QuikGraph.TaggedEdge<string, EdgeData>>;

public class ExtraGraph : MonoBehaviour
{
    [SerializeField] GraphManager graphManager;
    [SerializeField] Physik physik;
    
    [SerializeField] GameObject vertexPrefab;
    [SerializeField] GameObject edgePrefab;
    [SerializeField] GeneratorMenu generatorMenu;
    [SerializeField] TMP_InputField vertexCountInput;
    [SerializeField] TMP_InputField edgeCountInput;
    [SerializeField] TMP_InputField proportionOfGeneratorsInput;
    [SerializeField] List<Kamera> cameras;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        foreach (var camera in cameras)
            camera.center = graphManager.getVertices().FirstOrDefault()?.transform;

        physik.startUp(graphManager, 3, generatorStrings.Length);
        physik.shutDown();
    }

    void DrawGraph(IEnumerable<char> generators, TaggedGraph graph) {
        graphManager.Initialize(generators.ToArray(), physik);
        graphManager.SetSplinificationMode((int)Edge.SplinificationType.Always);
        graphManager.ResetGraph();
        var vertexDict = new Dictionary<string, Vertex>();
        foreach (var vertexName in graph.Vertices) {
            var newVertex = vertexDict[vertexName] = Instantiate(vertexPrefab, transform).GetComponent<Vertex>();
            newVertex.Initialize(VectorN.Random(3, 10), graphManager);
            newVertex.name = "Node " + vertexName;
            graphManager.AddVertex(newVertex);
        }
        foreach (var edge in graph.Edges) {
            var newEdge = Instantiate(edgePrefab, transform).GetComponent<Edge>();
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
    
            newEdge.Initialize(startPoint, endPoint, label, graphManager);
            graphManager.AddEdge(newEdge);
        }
    }
}
