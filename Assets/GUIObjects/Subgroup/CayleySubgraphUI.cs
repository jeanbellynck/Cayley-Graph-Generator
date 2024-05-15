using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Methods referenced from UI
public class CayleySubgraphUI : MonoBehaviour
{
    public RelatorMenu subgroupGenerators;
    public CayleyGraphMain cayleyGraphMain;

    /**
     * This method draws a subgroup inside the graph. The subgroup edges will not affect the original graph.
     */
    public void DrawSubgroup() {
        IEnumerable<string> generators = subgroupGenerators.GetRelators();
        cayleyGraphMain.DrawSubgroup(generators, 1f, 0.1f);
    }

    /**
     * This method draws a subgroup inside the graph. The graph is clustered by the subgroups and their cosets. 
     * This is done by reducing the strength of all ambient-group edges.
     */
    public void ClusterBySubgroup() {
        IEnumerable<string> generators = subgroupGenerators.GetRelators();
        cayleyGraphMain.DrawSubgroup(generators, 0.1f, 1f);
    }

    public void GreyOutComplement(bool greyedOut) {
        cayleyGraphMain.GreyOutComplement(greyedOut);
    }
}
