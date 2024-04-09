using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This class was split from LabelledGraphManager to separate the visual stuff from the logic.
 */
public class GraphVisualizer : MonoBehaviour {
    public LabelledGraphManager graphManager = new LabelledGraphManager(); 
    [SerializeField] public Edge.SplinificationType splinificationType { get; protected set; } = Edge.SplinificationType.WhenSimulationHasStopped;
    [SerializeField] Color[] ColorList = { new(1, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) };
    IActivityProvider activityProvider;

    public Dictionary<char, Color> labelColors = new();

    [SerializeField] GroupColorPanel groupColorPanel;
    [SerializeField] List<Kamera> kameras;

    
    
    public float Activity => activityProvider.Activity;
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

        graphManager.GetEdges().ForEach(edge => UpdateLabel(edge));
        graphManager.LabelCount = 2 * labelColors.Count;
        groupColorPanel.updateView(labelColors);
    }

    public void UpdateLabel(Edge edge) => edge.SetColors(labelColors[edge.Label]);

    
    // referred to from event (UI)
    public void SetSplinificationMode(int t) {
        splinificationType = (Edge.SplinificationType) t;
    }
}