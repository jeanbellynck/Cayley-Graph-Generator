using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CayleyGraphMaker : MonoBehaviour {
    private GraphManager graphManager;
    private MeshManager meshManager;
    private Physik physik; // I wonder whether the reference to Physics is necessary? 

    protected char[] generators;// = new char[]{'a', 'b', 'c'};
    protected char[] operators; // Like generators but with upper and lower case letters
    protected string[] relators;// = new string[]{"abAB"};

    [SerializeField] float hyperbolicity = 1;
    readonly Dictionary<(char, char), float> hyperbolicityMatrix = new();


    // Konfigurationen
    [SerializeField] int vertexNumber; // Describes the number of vertices the graph should have. It would be better to have a config file with all the data.
    [SerializeField] float drawingSpeed = 1; // Describes the speed at which new vertices should be drawn in vertices per second 

    [SerializeField] GameObject vertexPrefab;
    [SerializeField] GameObject edgePrefab;
    [SerializeField] GameObject meshPrefab;
    [SerializeField] GroupColorPanel groupColorPanel;

    [SerializeField] Kamera kamera;

    private int simulationDimensionality = 3;


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    List<List<GroupVertex>> randKnoten = new();
    // Contains the references of al vertices which need to be checked for relator application.
    HashSet<GroupVertex> relatorCandidates = new();
    HashSet<GroupVertex> edgeMergeCandidates = new();


    public void StartVisualization(GraphManager graphManager, char[] generators, string[] relators, int dimension) {
        this.graphManager = graphManager;
        this.meshManager = new(meshPrefab);
        this.generators = generators;
        this.relators = relators;
        this.simulationDimensionality = dimension;
        operators = new char[2 * generators.Length];

        for (int i = 0; i < generators.Length; i++) {
            operators[i] = char.ToLower(generators[i]);
            operators[i + generators.Length] = char.ToUpper(generators[i]);
        }
        
        groupColorPanel.updateView(graphManager.labelColors);

        //int simulationDimensionality = 2*generators.Length + 1;

        GroupVertex neutralElement = CreateVertex(null, default);
        kamera.center = neutralElement.transform;
        neutralElement.transform.localScale *= 1.6f;

        StartCoroutine(createNewElementsAndApplyRelators());
    }


    public void setPhysics(Physik physik) {
        this.physik = physik;
    }


    public void StopVisualization() {
        StopAllCoroutines();
        randKnoten = new();
        relatorCandidates = new();
        edgeMergeCandidates = new();
        if (graphManager != null) graphManager.ResetGraph();
        meshManager?.resetMeshes();
    }


    IEnumerator createNewElementsAndApplyRelators() {
        bool firstIteration = true;

        while (vertexNumber > graphManager.getVertices().Count) {
            // Speed is proportional to the number of vertices on the border. This makes knotting less likely
            float waitTime = 1 / (drawingSpeed * Mathf.Max(1, GetBorderVertexCount()));
            if (!firstIteration) {
                yield return new WaitForSeconds(waitTime);
            }
            else {
                firstIteration = false;
            }

            GroupVertex borderVertex = GetNextBorderVertex();
            if (borderVertex == null) {
                print("No vertices remaining. Stopping.");
                break;
            }

            foreach (char gen in generators) {
                if (borderVertex.FollowEdge(gen) == null) {
                    GroupVertex newVertex = CreateVertex(borderVertex, gen);
                    relatorCandidates.Add(newVertex);
                }
                if (borderVertex.FollowEdge(char.ToUpper(gen)) == null) {
                    GroupVertex newVertex = CreateVertex(borderVertex, char.ToUpper(gen));
                    relatorCandidates.Add(newVertex);
                }
            }
            MergeAll();
        }

        DrawMesh();
        StartCoroutine(physik.decayAlpha());
    }


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    */
    private GroupVertex CreateVertex(GroupVertex predecessor, char op) {
        GroupVertex newVertex = Instantiate(vertexPrefab, transform).GetComponent<GroupVertex>();
        if (predecessor == null)
            newVertex.Initialize(VectorN.Zero(simulationDimensionality), graphManager, "1", new List<string>(){""});
        else
            newVertex.InitializeFromPredecessor(predecessor, op, hyperbolicity);
        graphManager.AddVertex(newVertex);

        AddBorderVertex(newVertex);
        // Vertex is not the neutral element and an edge need to be created
        if (predecessor != null) 
            CreateEdge(predecessor, newVertex, op);

        return newVertex;
    }


    public GroupEdge CreateEdge(GroupVertex startvertex, GroupVertex endvertex, char op) {
        // If the edge already exists, no edge is created and the existing edge is returned
        foreach (GroupEdge edge in startvertex.GetEdges(op)) {
            if (edge.GetOpposite(startvertex).Equals(endvertex)) {
                return edge;
            }
        }

        GroupEdge newEdge = Instantiate(edgePrefab, transform).GetComponent<GroupEdge>();
        newEdge.Initialize(startvertex, endvertex, op, hyperbolicity, graphManager);
        graphManager.AddEdge(newEdge);

        return newEdge;
    }


    void AddBorderVertex(GroupVertex vertex) {
        if (randKnoten.Count <= vertex.DistanceToNeutralElement) {
            randKnoten.Add(new());
        }

        randKnoten[vertex.DistanceToNeutralElement].Add(vertex);
    }

    public GroupVertex GetNextBorderVertex() {
        GroupVertex nextVertex = null;
        foreach (List<GroupVertex> borderVertices in randKnoten) {
            borderVertices.RemoveAll(item => item == null);
            if (borderVertices.Count > 0) {
                nextVertex = borderVertices.First();
                borderVertices.RemoveAt(0);
                return nextVertex;
            }
        }
        return null;
    }

    public int GetBorderVertexCount() {
        int count = 0;
        foreach (List<GroupVertex> borderVertices in randKnoten) {
            count += borderVertices.Count;
        }
        return count;
    }

    /**
    * Applies the relators to all groupElements in the mergeCandidates list.
    */
    private void MergeAll() {
        while (edgeMergeCandidates.Count > 0 || relatorCandidates.Count > 0) {
            GroupVertex mergeCandidate;
            // If two edges of the same generator lead to the different vertices, they need to be merged as fast as possible. Otherwise following generators is yucky.
            if (edgeMergeCandidates.Count > 0) {
                mergeCandidate = edgeMergeCandidates.First();
                edgeMergeCandidates.Remove(mergeCandidate);
                if (mergeCandidate != null) { // Might not be necessary
                    MergeEdges(mergeCandidate);
                }
            }
            else {
                mergeCandidate = relatorCandidates.First();
                relatorCandidates.Remove(mergeCandidate);
                if (mergeCandidate != null) {
                    MergeByRelator(mergeCandidate);
                }
            }
            print("Merged all vertices. Adding new Vertex.");
        }
    }

    void MergeEdges(GroupVertex vertex) {
        foreach (char op in operators) {
            List<GroupEdge> generatorEdges = vertex.GetEdges(op);
            if (generatorEdges.Count > 1) {
                GroupEdge primaryEdge = generatorEdges[0];
                for (int i = 1; i < generatorEdges.Count; i++) {
                    //edgeMergeCandidates.Add(vertex); // After an edge merge a vertex might be merged with its neighbor meaning its edges can be merged again.
                    MergesVertices(primaryEdge.GetOpposite(vertex), generatorEdges[i].GetOpposite(vertex));
                }
            }
        }
    }


    /**
    * Applies the relator to the given groupElement. For that the relator is followed, starting at a different string index each time. 
    * If the relator leads to an other group element, the two groupElements are merged.
    */
    void MergeByRelator(GroupVertex startingElement) {
        // Der Code ist ein wenig unoptimiert. Nach dem Anwenden eines Relators versucht er wieder alle anzuwenden. 
        // Dadurch steigt die Komplexität im Worst-Case zu n^2 falls alle Relatoren genutzt werden. (Was natürlichunwahrscheinlich ist)
        foreach (string relator in relators) {
            List<string> relatorVariants = generateRelatorVariants(relator);
            foreach (string relatorVariant in relatorVariants) {
                GroupVertex currentElement = startingElement;
                bool relatorLeadToOtherElement = true;
                foreach (char op in relatorVariant) {
                    currentElement = currentElement.FollowEdge(op);
                    if (currentElement == null) {
                        relatorLeadToOtherElement = false;
                        break;
                    }
                }
                if (relatorLeadToOtherElement && !currentElement.Equals(startingElement)) {
                    MergesVertices(startingElement, currentElement);
                    return;
                }
            }
        }
    }

    /**
     * Generates all possible variants of the given relator.
     * Variants are generated by rotating or inverting the relator string.
     **/
    public List<string> generateRelatorVariants(string relator) {
        string relatorInverse = RelatorDecoder.invertSymbol(relator);
        List<string> variants = new();
        for (int i = 0; i < relator.Length; i++) {
            variants.Add(relator[i..] + relator[..i]);
            variants.Add(relatorInverse[i..] + relatorInverse[..i]);
        }
        return variants;
    }

    /**
    * Merges vertex2 and vertex1. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void MergesVertices(GroupVertex vertex1, GroupVertex vertex2) {
        if (vertex1.Equals(vertex2)) return;
        // The vertex with the longer name will be deleted. (We don't want to delete the neutral element.)
        if (vertex2.name.Length < vertex1.name.Length) {
            (vertex1, vertex2) = (vertex2, vertex1);
        }

        vertex1.Merge(vertex2, hyperbolicity);

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach (char op in vertex2.GetEdges().Keys) {
            List<GroupEdge> generatorEdgesCopy = new(vertex2.GetEdges(op));
            foreach (GroupEdge edge in generatorEdgesCopy) {
                CreateEdge(vertex1, edge.GetOpposite(vertex2), op);
            }
        }

        // Delete vertex2
        graphManager.RemoveVertex(vertex2);
        vertex2.Destroy();

        // Neuen Knoten nochmal prüfen
        edgeMergeCandidates.Add(vertex1);
        relatorCandidates.Add(vertex1);
    }


    /**
     * This is a method that would better fit into a "group element" class.
     * Taking in paths to identity and using the hyperbolicityMatrix it calculates the scaling of a generator. This give the desired length of an edge.
     **/
    float CalculateScalingForGenerator(char generator, string path) {
        float scaling = 1;
        foreach (char op in path) {
            scaling *= hyperbolicityMatrix[(op, generator)];
        }
        return scaling;
    }


    void DrawMesh() {
        foreach (var vertex in graphManager.getVertices()) {
            foreach (string relator in relators) {
                var vertices = new Vertex[relator.Length];
                vertices[0] = vertex;

                bool doInitialize = true;
                for (int i = 0; i < relator.Length - 1; i++) {
                    vertices[i + 1] = vertices[i].FollowEdge(relator[i]);
                    if (vertices[i + 1] == null) { doInitialize = false; break; }
                }

                if (doInitialize) meshManager.AddMesh(vertices, transform);
            }

            //if(vertex.name != "") {break;}
        }

    }

    public void setVertexNumber(int v) {
        vertexNumber = v;
    }

    public void setHyperbolicity(float hyperbolicity) {
        this.hyperbolicity = hyperbolicity;
        // Set all values of the hyperbolicity matrix to the new hyperbolicity
        float[,] matrix = new float[generators.Length, generators.Length];
        for (int i = 0; i < generators.Length; i++) {
            for (int j = 0; j < generators.Length; j++) {
                matrix[i, j] = hyperbolicity;
            }
        }
        SetHyperbolicityMatrix(matrix);
    }

    public void SetHyperbolicityMatrix(float[,] matrix) {
        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                float matrixValue = matrix[i, j];
                if (matrixValue < 0) {
                    // For negative Values the hyperbolic scaling is done in one direction
                    hyperbolicityMatrix[(generators[i], generators[j])] = -matrix[i, j];
                    hyperbolicityMatrix[(char.ToUpper(generators[i]), generators[j])] = -1 / matrix[i, j];
                }
                else {
                    // For positive Values the hyperbolic scaling is done in both directions
                    hyperbolicityMatrix[(generators[i], generators[j])] = matrix[i, j];
                    hyperbolicityMatrix[(char.ToUpper(generators[i]), generators[j])] = matrix[i, j];
                }
            }
        }
        recalculateHyperbolicity();
    }

    /**
     * Recalculates the length of all edges according to the hyperbolicity.
     */
    void recalculateHyperbolicity() {
        if (graphManager == null)
            return;

        foreach (var edge in graphManager.GetEdges())
            if (edge is GroupEdge groupEdge)
                groupEdge.calculateEdgeLength(hyperbolicity);

        foreach (var vertex in graphManager.getVertices())
            if (vertex is GroupVertex groupVertex) 
                groupVertex.calculateVertexMass(hyperbolicity);
    }


    public void setGenerators(char[] generators) {
        this.generators = generators;
    }
}
