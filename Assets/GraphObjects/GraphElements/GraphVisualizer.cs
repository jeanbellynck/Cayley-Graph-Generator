using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This class was split from LabelledGraphManager to separate the visual stuff from the logic.
 */
public class GraphVisualizer : MonoBehaviour {
    public LabelledGraphManager graphVisualizer = new LabelledGraphManager(); 
    [SerializeField] public Edge.SplinificationType splinificationType { get; protected set; } = Edge.SplinificationType.WhenSimulationHasStopped;
    [SerializeField] Color[] ColorList = { new(1, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) };
    IActivityProvider activityProvider;
    public List<char> generatorLabels = new();
    public List<char> subgroupLabels = new();

    public Dictionary<char, Color> labelColors = new();

    [SerializeField] GroupColorPanel groupColorPanel;
    [SerializeField] List<Kamera> kameras;

    
    
    public float Activity => activityProvider.Activity;
    //public Kamera kamera { get; protected set; }

    public void Initialize(IEnumerable<char> generators, IActivityProvider activityProvider) { 
        this.activityProvider = activityProvider;

        graphVisualizer.onEdgeAdded += UpdateLabel;
        graphVisualizer.OnCenterChanged += (vertex, activeKamera) => {
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
            ColorList.Extend(
                () => Random.ColorHSV(0, 1, 0.9f, 1)
            ), (generator, color) => new KeyValuePair<char, Color>(generator, color)
        ));

        graphVisualizer.GetEdges().ForEach(edge => UpdateLabel(edge));
        graphVisualizer.LabelCount = 2 * labelColors.Count;
        groupColorPanel.updateView(labelColors);
    }

    public void UpdateLabel(Edge edge) => edge.SetColors(labelColors[edge.Label]);

    
    // referred to from event (UI)
    public void SetSplinificationMode(int t) {
        splinificationType = (Edge.SplinificationType) t;
    }
}