using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Methods referenced from UI
public class CayleySubgraphUI : MonoBehaviour
{
    [SerializeField] RelatorMenu subgroupGenerators;
    [SerializeField] CayleyGraphMaker cayleyGraphMaker;
    [SerializeField] CayleySubGraphMaker cayleySubGraphMaker;
    [SerializeField] GraphVisualizer graphVisualizer;
    [SerializeField] Slider strengthRatioSlider;


    public void ResetSubgroup() {
        SetStrengthRatio(0);// reactivates physics
        cayleySubGraphMaker.ResetSubgraph();
    }

    public void SetStrengthRatio(float r) {
        if (!cayleySubGraphMaker.Running) return;
        //if (r < 0.1f) r = 0.1f;
        //if (r > 0.9f) r = 0.9f;
        graphVisualizer.AmbientEdgeStrength = 1-r;
        graphVisualizer.SubgroupEdgeStrength = r;
    }

    public void SetSubgroupDrawingPreference(float r) {
        cayleyGraphMaker.UpdateSubgroupPreference(Mathf.Exp(-r));
    }

    /**
     * This method draws a subgroup inside the graph.
     * It also sets the strength of the subgroup edges and the ambient edges.
     */
    public void DrawSubgroup() {
        SetStrengthRatio(strengthRatioSlider.value);

        var generators = subgroupGenerators.GetRelators().ToArray();
        var neutralElement = cayleyGraphMaker.NeutralElement;
        cayleySubGraphMaker.RegenerateSubgroup(generators, neutralElement);
        cayleyGraphMaker.OnStopVisualization += () => cayleySubGraphMaker.FixSubgroup(generators, neutralElement);
    }

    public void GreyOutComplement(bool greyedOut) {
        graphVisualizer.GreyOut(greyedOut, v => !v.IsHighlighted(HighlightType.Subgroup), e => !e.IsHighlighted(HighlightType.Subgroup));
        graphVisualizer.baseImportance = greyedOut ? 0.3f : 1f;
    }

}
