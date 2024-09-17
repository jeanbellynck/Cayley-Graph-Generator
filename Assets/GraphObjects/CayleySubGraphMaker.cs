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
    [SerializeField] GraphVisualizer graphVisualizer; // In the best case this would actually be a graphManager, but that would require a lot of changes.
    GroupVertex neutralElement;
    string currentCreationTaskLabel;
    public bool Running => !generatorList.IsEmpty();


    // Method for drawing/changing the subgraph
    /**
    - Takes in generators
    - Deletes previous subgraph if necessary
    - Adds an edge to each vertex by following generator paths
    - The rest is handled by the GraphVisualizer.
    **/
    public void RegenerateSubgroup(IEnumerable<string> generators, GroupVertex neutralElement) {
        // Delete previous subgraph
        ResetSubgraph();
        GenerateSubgroup(generators, neutralElement);
        SubgroupHighlight(false);
    }


    const int maxSubgroupEdgesPerFrame = 10;

    // This method is called when the graph is done generating; We have to check if the subgroup is still the same!
    public void FixSubgroup(IEnumerable<string> generators = null, GroupVertex neutralElement = null) {
        if (!generators.SequenceEqual(generatorList)) return;
        if (neutralElement != this.neutralElement) return;
        GenerateSubgroup();
    }

    void GenerateSubgroup(IEnumerable<string> generators = null, GroupVertex neutralElement = null) {
        if (neutralElement != null)
            this.neutralElement = neutralElement;
        if (generators != null) {
            generatorList = new (generators);
            generatorNames = (
                    from genIndex in Enumerable.Range(0, generatorList.Count)
                    select (char)(genIndex + '0')
                ).ToList();
            Debug.Log("Generating subgroup with generators " +string.Join(", ",generatorList));

        }

        var vertices = new List<Vertex>( graphVisualizer.graphManager.GetVertices() );
        var creationTaskLabel = "+SGE" + Time.frameCount;
        currentCreationTaskLabel = creationTaskLabel;

        // Add edges to the subgraph
        for (int genIndex = 0; genIndex < generatorList.Count; genIndex++) {
            var enumerator = vertices.GetEnumerator();
            var generator = generatorList[genIndex];
            var generatorName = generatorNames[genIndex];

            for (int i = 0; i < Math.Ceiling((double) maxSubgroupEdgesPerFrame / generatorList.Count); i++)
                PlanAddSubgroupEdge(enumerator, generator, generatorName, currentCreationTaskLabel);
            // maxSubgroupEdgesPerFrame runners run with the same enumerator
        }

        Action<Vertex> vertexAdded = null;
        vertexAdded = vertex => {
            if (currentCreationTaskLabel != creationTaskLabel) {
                graphVisualizer.OnVertexAdded -= vertexAdded;
                return;
            }

            if (vertex is not GroupVertex groupVertex) return;
            for (int genIndex = 0; genIndex < generatorList.Count; genIndex++) {
                var generator = generatorList[genIndex];
                var generatorName = generatorNames[genIndex];
                graphVisualizer.PlanAction((
                    () => {
                        AddSubgroupEdge(groupVertex, generator, generatorName, false);
                        AddSubgroupEdge(groupVertex, generator, generatorName, true);
                    },
                    currentCreationTaskLabel,
                    2
                )); 
            }
        };
        graphVisualizer.OnVertexAdded += vertexAdded; 
    }

    void PlanAddSubgroupEdge(IEnumerator<Vertex> enumerator, string generator, char generatorName, string label)
    {
        var startVertex = enumerator.Current;
        if (enumerator.MoveNext())
            graphVisualizer.PlanAction((
                () => PlanAddSubgroupEdge(enumerator, generator, generatorName, label),
                label,
                1
            ));
        else Debug.Log("No more vertices to add subgroup edges to.");
        AddSubgroupEdge(startVertex, generator, generatorName);
    }

    void AddSubgroupEdge(Vertex vertex, string generator, char generatorName, bool inverse = false) {
        if (vertex is not GroupVertex startVertex || startVertex == null) return; 
        // again, unity destruction doesn't imply C# nullness, only == null
        var edge = inverse
            ? vertex.GetIncomingEdges(generatorName).FirstOrDefault()
            : vertex.GetOutgoingEdges(generatorName).FirstOrDefault();
        if (edge is GroupEdge) return;

        if (inverse) generator = RelatorDecoder.InvertSymbol(generator);
        GroupVertex endVertex = startVertex.FollowGeneratorPath(generator);
        if (endVertex == null) return;
        // this will sadly add a lot of vertices twice to the plan, but sometimes vertices are added with no connection to the next subgroup element, but will later be connected from the other side
        // still doesn't work. Will just redraw the subgroup at the end of generation.
        // graphVisualizer.PlanAction((
        //    () => AddSubgroupEdge(endVertex, generator, generatorName, inverse),
        //    "+SGEr",
        //    1
        //));
        if (inverse) 
            graphVisualizer.CreateSubgroupEdge(endVertex, startVertex, generatorName);
        else 
            graphVisualizer.CreateSubgroupEdge(startVertex, endVertex, generatorName);


        if (startVertex.IsHighlighted(HighlightType.Subgroup))
            SubgroupHighlight(false, endVertex);
        else if (endVertex.IsHighlighted(HighlightType.Subgroup))
            SubgroupHighlight(false, startVertex);
    }


    /**
     * Deletes the subgraph from the graphManager.
     * This is done by deleting all edges with a number as label.
     **/
    public void ResetSubgraph() {
        graphVisualizer.CancelActions(currentCreationTaskLabel);
        currentCreationTaskLabel = null;
        SubgroupHighlight(true);
        // Delete all edges from the previous subgraph
        foreach (var label in generatorNames) 
            graphVisualizer.PlanAction((
                () => graphVisualizer.graphManager.DestroyEdges(label),
                "-SGE",
                100
            ));
        generatorList.Clear();
        generatorNames.Clear();
    }

    void SubgroupHighlight(bool removeHighlight, Vertex startVertex = null) {
        HashSet<char> subgroupGenerators = generatorNames.ToHashSet();

        graphVisualizer.CancelActions(removeHighlight ? "+SG" : "-SG");

        if (startVertex == null)
            startVertex = neutralElement;
        if (startVertex == null) 
            return;
        startVertex.Highlight(HighlightType.Subgroup, FollowAllSubgroupEdges, removeHighlight, false);
        return;

        (IEnumerable<char>, IEnumerable<char>) FollowAllSubgroupEdges (string path) => (subgroupGenerators, subgroupGenerators);
    }

}
