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
    public LabelledGraphManager graphVisualizer;

    
    // Method for drawing/changing the subgraph
    /**
    - Takes in generators
    - Deletes previous subgraph if necessary
    - Adds an edge to each vertex by following generator paths
    - The rest is handled by the GraphVisualizer.
    **/ 
}
