using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This class was split from LabelledGraphManager to separate the visual stuff from the logic.
 */
public class GraphVisualizer : MonoBehaviour, IActivityProvider {
    public LabelledGraphManager graphManager = new LabelledGraphManager(); 
    [SerializeField] public Edge.SplinificationType splinificationType { get; protected set; } = Edge.SplinificationType.WhenSimulationHasStopped;
    [SerializeField] Color[] ColorList = { new(1, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) };
    public List<char> generatorLabels = new();
    public List<char> subgroupLabels = new();

    public Dictionary<char, Color> labelColors = new();

    [SerializeField] GroupColorPanel groupColorPanel;
    [SerializeField] List<Kamera> kameras;
    
    [SerializeField] int simulationDimensionality = 3;
    
    [SerializeField] GameObject vertexPrefab;
    [SerializeField] GameObject edgePrefab;
    [SerializeField] IActivityProvider activityProvider;


    public float Activity => activityProvider.Activity;

    public int LabelCount => graphManager.LabelCount;
    //public Kamera kamera { get; protected set; }

    public void Initialize(IEnumerable<char> generators, IActivityProvider activityProvider) { 
        this.activityProvider = activityProvider;

        graphManager.onEdgeAdded += UpdateLabel;
        graphManager.OnCenterChanged += (vertex, activeKamera) => {
            if (activeKamera != null) 
                activeKamera.centerPointer = vertex.centerPointer;
            else
                foreach (var kamera in kameras) 
                    kamera.centerPointer = vertex.centerPointer;
        };
        UpdateGeneratorLabels(generators);
    }

    public void UpdateGeneratorLabels(IEnumerable<char> generators) {
        generatorLabels = generators.ToList();
        UpdateLabels();
    }


    public void UpdateLabels()
    { 
        labelColors = new(Enumerable.Zip(
            generatorLabels, 
            ColorList.Extend(RandomColor), (generator, color) => new KeyValuePair<char, Color>(generator, color)
        ));

        graphManager.GetEdges().ForEach(edge => UpdateLabel(edge));
        graphManager.LabelCount = 2 * labelColors.Count;
        groupColorPanel.updateView(labelColors);
    }

    private Color RandomColor() {
        return Random.ColorHSV(0, 1, 0.9f, 1);
    }

    public void UpdateLabel(Edge edge) => edge.SetColors(labelColors[edge.Label]);


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    * I moved this to the visualizer because the createEdge method is also here.
    */
    public GroupVertex CreateVertex(GroupVertex predecessor, char op, float hyperbolicity) {
        GroupVertex newVertex = Instantiate(vertexPrefab, transform).GetComponent<GroupVertex>();
        if (predecessor == null)
            newVertex.Initialize(VectorN.Zero(simulationDimensionality), this, "1", new List<string>(){""});
        else
            newVertex.InitializeFromPredecessor(predecessor, op, hyperbolicity);
        graphManager.AddVertex(newVertex);

        return newVertex;
    }

    public GroupEdge CreateEdge(GroupVertex startvertex, GroupVertex endvertex, char op) {
        return CreateEdge(startvertex, endvertex, op, 0);
    }

    public GroupEdge CreateSubgroupEdge(GroupVertex startvertex, GroupVertex endvertex, char op) {
        // Create edge
        GroupEdge newEdge = CreateEdge(startvertex, endvertex, op, 0);
        // Set layer to subgroup
        newEdge.gameObject.layer = LayerMask.NameToLayer("SubgroupOnly");
        return newEdge;
    }

    /**
     * Creates a new edge and adds it to the graph. If the edge already exists, the existing edge is returned.
     * I moved it to the visualizer because the cayley graph maker and subgraph maker need this method.
     **/    
    public GroupEdge CreateEdge(GroupVertex startvertex, GroupVertex endvertex, char op, float hyperbolicity) {
        // Check if the edge label has a colour assigned
        if (!labelColors.ContainsKey(op)) {
            labelColors[op] = RandomColor();
        }

        // If the edge already exists, no edge is created and the existing edge is returned
        foreach (GroupEdge edge in startvertex.GetEdges(op)) {
            if (edge.GetOpposite(startvertex).Equals(endvertex)) {
                return edge;
            }
        }

        GroupEdge newEdge = Instantiate(edgePrefab, transform).GetComponent<GroupEdge>();
        newEdge.Initialize(startvertex, endvertex, op, hyperbolicity, this);
        graphManager.AddEdge(newEdge);

        return newEdge;
    }

    
    // referred to from event (UI)
    public void SetSplinificationMode(int t) {
        splinificationType = (Edge.SplinificationType) t;
    }

    public void SetDimension(int dim) {
        simulationDimensionality = dim;
    }
}