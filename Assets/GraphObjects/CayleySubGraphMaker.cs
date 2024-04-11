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
    public List<string> generatorList = new List<string>();
    public GraphVisualizer graphVisualizer; // In the best case this would actually be a graphManager, but that would require a lot of changes.

    // Method for drawing/changing the subgraph
    /**
    - Takes in generators
    - Deletes previous subgraph if necessary
    - Adds an edge to each vertex by following generator paths
    - The rest is handled by the GraphVisualizer.
    **/
    public void RegenerateSubgroup(IEnumerable<string> generators) {
        // Delete previous subgraph
        ResetSubgraph(generators);
        GenerateSubgroup();
    }

    public void GenerateSubgroup() {
        // Add edges to the subgraph
        for (int genIndex = 0; genIndex < generatorList.Count; genIndex++) {
            foreach(GroupVertex startVertex in graphVisualizer.graphManager.GetVertices()) {
                GroupVertex endVertex = FollowGeneratorPath(startVertex, generatorList[genIndex]);
                if (endVertex != null) {
                    graphVisualizer.CreateEdge(startVertex, endVertex, (char)(genIndex + '0'));
                }
            }
        }
    }

    /**
     * Deletes the subgraph from the graphManager.
     * This is done by deleting all edges with a number as label.
     **/
    private void ResetSubgraph(IEnumerable<string> generators) {
        // Delete all edges from the previous subgraph
        for(int i = 0; i < generatorList.Count; i++) {
            graphVisualizer.graphManager.RemoveEdges((char)(i + '0'));
        }

        // Set the generatorList to the new generators
        generatorList = new List<string>(generators);
    }

    /**
     * Takes in a vertex and a string and follows the path as given by the generator string.
     * Returns the vertex at the end of the path.
     * Returns null if the path is not valid or leaves the ambient graph.
     **/
    private GroupVertex FollowGeneratorPath(GroupVertex startVertex, string generator) {
        // For each lettern in the string
        GroupVertex currentVertex = startVertex;
        foreach (char op in generator) {
            currentVertex = currentVertex.FollowEdge(op);
            if (currentVertex == null) {
                return null;
            } 
        }
        return currentVertex; 
    }

}
