using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
 * This class is responsible for creating the subgraph of a Cayley graph.
 * The subgraph is saved in the graphManager, thereby updating the visual representation of the graph. 
 * Differently to normal generators, the subgraph generators are enumerators by numbers, not letters.
 */
public class CayleySubGraphMaker : MonoBehaviour {
    List<string> generatorList = new();
    List<char> generatorNames = new();
    public GraphVisualizer graphVisualizer; // In the best case this would actually be a graphManager, but that would require a lot of changes.
    GroupVertex neutralElement;

    // Method for drawing/changing the subgraph
    /**
    - Takes in generators
    - Deletes previous subgraph if necessary
    - Adds an edge to each vertex by following generator paths
    - The rest is handled by the GraphVisualizer.
    **/
    public void RegenerateSubgroup(IEnumerable<string> generators, GroupVertex neutralElement) {
        // Delete previous subgraph
        ResetSubgraph(generators);
        GenerateSubgroup();
        this.neutralElement = neutralElement;
        SubgroupHighlight(false);
    }

    public void GenerateSubgroup() {
        generatorNames = (
                from genIndex in Enumerable.Range(0, generatorList.Count)
                select (char)(genIndex + '0')
            ).ToList();

        // Add edges to the subgraph
        for (int genIndex = 0; genIndex < generatorList.Count; genIndex++) {
            foreach(GroupVertex startVertex in graphVisualizer.graphManager.GetVertices()) {
                GroupVertex endVertex = startVertex.FollowGeneratorPath(generatorList[genIndex]);
                if (endVertex == null) continue;
                graphVisualizer.CreateSubgroupEdge(startVertex, endVertex, generatorNames[genIndex]);
                // todo: if any of the vertices are subgroup highlighted, subgroup highlight!
            }
        }
    }

    /**
     * Deletes the subgraph from the graphManager.
     * This is done by deleting all edges with a number as label.
     **/
    void ResetSubgraph(IEnumerable<string> generators) {
        SubgroupHighlight(true);
        // Delete all edges from the previous subgraph
        foreach (var label in generatorNames) 
            graphVisualizer.graphManager.DestroyEdges(label);
        

        // Set the generatorList to the new generators
        generatorList = new List<string>(generators);
    }

    public void SubgroupHighlight(bool removeHighlight) {
        HashSet<char> subgroupGenerators = generatorNames.ToHashSet();

        if (neutralElement != null) 
            neutralElement.Highlight(HighlightType.Subgroup, FollowEdges, "", removeHighlight, false);
        return;

        (IEnumerable<char>, IEnumerable<char>) FollowEdges (string path) => (subgroupGenerators, subgroupGenerators);
    }

}
