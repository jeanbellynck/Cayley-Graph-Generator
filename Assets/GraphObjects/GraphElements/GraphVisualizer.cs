using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/**
 * This class was split from LabelledGraphManager to separate the visual stuff from the logic.
 */
public class GraphVisualizer : MonoBehaviour, IActivityProvider {
    public readonly LabelledGraphManager graphManager = new(); 
    [SerializeField] Physik physik;
    [field: SerializeField] public Edge.SplinificationType splinificationType { get; protected set; } = Edge.SplinificationType.WhenSimulationHasStopped;
    [SerializeField] Color[] ColorList = { new(1, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) };
    [SerializeField] List<char> generatorLabels = new();

    [SerializeField] Dictionary<char, Color> labelColors = new();

    [SerializeField] GroupColorPanel groupColorPanel;
    [SerializeField] List<Kamera> kameras;
    
    [SerializeField] int simulationDimensionality = 3;
    
    [SerializeField] GameObject vertexPrefab;
    [SerializeField] GameObject edgePrefab;
    [SerializeField] IActivityProvider activityProvider;
    [SerializeField] ElementFinder centerProvider; // this should be of type ICenterProvider, but Unity doesn't show this.

    float _ambientEdgeStrength, _subgroupEdgeStrength;
    public float baseImportance = 1;

    public event Action<Vertex> OnVertexAdded;

    public float AmbientEdgeStrength {
        get => _ambientEdgeStrength;
        set {
            if (Math.Abs(_ambientEdgeStrength - value) > 1e-6) {
                _ambientEdgeStrength = value;
                foreach (char label in generatorLabels) {
                    foreach (Edge edge in graphManager.GetEdges(label)) {
                        edge.Strength = value;
                    }
                }
            }
            physik.RunShortly(20f);
        }
    }
    public float SubgroupEdgeStrength {
        get => _subgroupEdgeStrength;
        set {
            _subgroupEdgeStrength = value;
            physik.RunShortly(20f);
            for (int i = 0; i < 10; i++) {
                foreach (Edge edge in graphManager.GetEdges((char)(i + '0'))) {
                    edge.Strength = value;
                }
            }
        }
    }

    public float Activity => activityProvider.Activity;

    public int LabelCount => graphManager.LabelCount;

    GroupVertex selectedVertex;
    public GroupVertex SelectedVertex { get => selectedVertex; set { selectedVertex = value; SelectedVertexChanged.Invoke(value); } }
    public UnityEvent<GroupVertex> SelectedVertexChanged = new();

    //public Kamera kamera { get; protected set; }

    public void Initialize(IEnumerable<char> generators, IActivityProvider activityProvider) { 
        this.activityProvider = activityProvider;

        _ambientEdgeStrength = 1f;
        _subgroupEdgeStrength = 0f;
        graphManager.onEdgeAdded += UpdateLabel;
        graphManager.OnCenterChanged += (centerPointer, activeKamera) => {
            if (activeKamera != null) 
                activeKamera.centerPointer = centerPointer;
            else
                foreach (var kamera in kameras) 
                    kamera.centerPointer = centerPointer;
        }; // todo: Redundant?
        if (centerProvider is not null)
            centerProvider.OnCenterChanged += (centerPointer) => {
                    foreach (var kamera in kameras)
                        kamera.centerPointer = centerPointer;
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
            ColorList.Extend(RandomColor),
            (generator, color) => new KeyValuePair<char, Color>(generator, color)
        ));

        graphManager.GetEdges().ForEach(edge => UpdateLabel(edge));
        // this throws errors since it wants to update all edges, but some or all of them are to be deleted and may thus have labels that are not in the labelColors dictionary anymore
        graphManager.LabelCount = 2 * labelColors.Count;
        groupColorPanel.updateView(labelColors);
    }

    static Color RandomColor() {
        return Random.ColorHSV(0, 1, 0.9f, 1);
    }

    public void UpdateLabel(Edge edge) {
        if (labelColors.TryGetValue(edge.Label, out var color))
            edge.SetColors(color);
    }


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
        newVertex.baseImportance = baseImportance;

        OnVertexAdded?.Invoke(newVertex);
        return newVertex;
    }

    public GroupEdge CreateSubgroupEdge([NotNull] GroupVertex startVertex, [NotNull] GroupVertex endVertex, char op) {
        //var edge = startVertex.GetOutgoingEdges(op).FirstOrDefault();
        //if (edge is GroupEdge) return null;

        var newEdge = CreateEdge(startVertex, endVertex, op, 1);
        newEdge.Strength = SubgroupEdgeStrength;
        newEdge.gameObject.layer = LayerMask.NameToLayer("SubgroupOnly");
        
        startVertex.DistanceToSubgroup = endVertex.DistanceToSubgroup = Math.Min(startVertex.DistanceToSubgroup, endVertex.DistanceToSubgroup); // distance to subgroup is constant in the coset.
        return newEdge;
    }

    /**
     * Creates a new edge and adds it to the graph. If the edge already exists, the existing edge is returned.
     * I moved it to the visualizer because the cayley graph maker and subgraph maker need this method.
     **/    
    public GroupEdge CreateEdge(GroupVertex startVertex, GroupVertex endVertex, char op, float hyperbolicity) {
        // Check if the edge label has a colour assigned
        if (!labelColors.ContainsKey(op)) {
            labelColors[op] = RandomColor();
        }

        foreach (var edgeList in startVertex.GetEdges().Values) {
            foreach (GroupEdge edge in edgeList) {     
                if(edge.EndPoint.Equals(endVertex)) {
                    if(edge.Label == op){
                        // If the edge already exists, no edge is created and the existing edge is returned
                        return edge;
                    }
                    // If an edge between start and end vertex already exists then the force of the new edge is descreased.
                    // This should reduce jiggling and reduces load on the physics engine.
                    edge.PhysicsEnabled = false;
                }
                
            }
        }

        GroupEdge newEdge = Instantiate(edgePrefab, transform).GetComponent<GroupEdge>();
        newEdge.Initialize(startVertex, endVertex, op, hyperbolicity, this);
        newEdge.Strength = AmbientEdgeStrength;
        graphManager.AddEdge(newEdge);


        if (startVertex.DistanceToSubgroup > endVertex.DistanceToSubgroup)
            startVertex.DistanceToSubgroup = endVertex.DistanceToSubgroup + 1;
        else if (endVertex.DistanceToSubgroup > startVertex.DistanceToSubgroup)
            endVertex.DistanceToSubgroup = startVertex.DistanceToSubgroup + 1;

        return newEdge;
    }

    
    // referred to from event (UI)
    public void SetSplinificationMode(int t) {
        splinificationType = (Edge.SplinificationType) t;
    }

    public void SetDimension(int dim) {
        simulationDimensionality = dim;
    }

    public void GreyOut(bool greyedOut, Func<Vertex, bool> vertexSelector, Func<Edge, bool> edgeSelector) {
        //foreach (var e in graphManager.GetEdges().Where(edgeSelector))
        //    e.GreyOut(greyedOut);
        foreach (var v in graphManager.GetVertices().Where(vertexSelector))
            v.GreyOut(greyedOut);
    }

    Queue<(Action, string, int)> plannedActions = new();
    readonly HashSet<string> executedActions = new();
    int steps;
    void Update() {
        executedActions.Clear();
        for (steps = 0; steps < highlightStepsPerFrame && plannedActions.TryDequeue(out var action); steps += action.Item3) {
            action.Item1.Invoke();
            executedActions.Add(action.Item2);
        }
    }
    void LateUpdate() {
        for (; steps < highlightStepsPerFrame && plannedActions.TryDequeue(out var action); steps += action.Item3) {
            action.Item1.Invoke();
            executedActions.Add(action.Item2);
        }
        if (steps > 0)
            Debug.Log($"This frame, we executed {steps} planned actions of types {string.Join(", ", executedActions)}, {plannedActions.Count} left");

    }

    public void CancelActions(string id) {
        plannedActions = new(plannedActions.Where(
            action => action.Item2 != id
        )); 
    }

    public void PlanAction((Action, string, int) plan)
    {
        plannedActions.Enqueue(plan);
        while (plannedActions.Count > highlightStepsPerFrame * 100) {
            var floodedRemovalAction = plannedActions.FirstOrDefault(a => a.Item2.StartsWith("~")).Item2;
            if (floodedRemovalAction != default) CancelActions(floodedRemovalAction);
            // this is very ad hoc, but sometimes the plannedActions queue gets flooded with highlight removal actions. Sometimes also with highlight adding actions. 
            // TODO: The highlighting of the paths to the identity for some reason is very weird as in it adds exponentially many actions to the queue. This should be fixed.
        }
    }

    const int highlightStepsPerFrame = 30;
}

public enum HighlightType {
    Selected,
    Subgroup,
    Path,
    PrimaryPath
}
public static class HighlightTypeExtensions {
    public static bool IsPath(this HighlightType type) => 
        type is HighlightType.Path or HighlightType.PrimaryPath;

    public static int ToActionType(this HighlightType type, bool remove = false) =>
        remove ? -1 :
        type switch {
            HighlightType.Subgroup => 0,
            _ => 1
        };

    public static int ToInt(this HighlightType type, bool remove = false) {
        return remove ? -1 : (int)type;
    }
}