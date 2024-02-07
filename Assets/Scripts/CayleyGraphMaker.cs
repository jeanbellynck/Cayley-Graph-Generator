using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CayleyGraphMaker : MonoBehaviour {
    private GraphManager graphManager;
    private MeshManager meshManager;
    private Physik physik; // I wonder whether the reference to Physics is necessary? 

    public GameObject neutralElementGameObject;


    protected char[] generators;// = new char[]{'a', 'b', 'c'};
    protected char[] operators; // Like generators but with upper and lower case letters
    protected string[] relators;// = new string[]{"abAB"};

    private float hyperbolicity = 1;
    private Dictionary<char, Dictionary<char, float>> hyperbolicityMatrix = new Dictionary<char, Dictionary<char, float>>();


    // Konfigurationen
    public int vertexNumber; // Describes the number of vertices the graph should have. It would be better to have a config file with all the data.
    public float drawingSpeed = 1; // Describes the speed at which new vertices should be drawn in vertices per second 

    public GameObject meshPrefab;
    public GameObject vertexPrefab;
    public GameObject edgePrefab;
    public Color[] colourList = new Color[] { new Color(255, 0, 0), new Color(0, 0, 255), new Color(0, 255, 0), new Color(255, 255, 0) };

    private int simulationDimensionality = 3;


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    List<List<GroupVertex>> randKnoten = new List<List<GroupVertex>>();
    // Contains the references of al vertices which need to be checked for relator application.
    HashSet<GroupVertex> relatorCandidates = new HashSet<GroupVertex>();
    HashSet<GroupVertex> edgeMergeCandidates = new HashSet<GroupVertex>();

    public void StartVisualization(GraphManager graphManager, MeshManager meshManager, char[] generators, string[] relators, int dimension) {
        this.graphManager = graphManager;
        this.meshManager = meshManager;
        this.generators = generators;
        this.relators = relators;
        this.simulationDimensionality = dimension;
        operators = new char[2 * generators.Length];

        for (int i = 0; i < generators.Length; i++) {
            operators[i] = char.ToLower(generators[i]);
            operators[i + generators.Length] = char.ToUpper(generators[i]);
        }
        GroupEdge.generatorColours = new Dictionary<char, Color>();
        for (int i = 0; i < generators.Length; i++) {
            if (i < colourList.Length) {
                GroupEdge.generatorColours.Add(generators[i], colourList[i]);
            }
            else {
                GroupEdge.generatorColours.Add(generators[i], new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255)));
            }

        }

        //int simulationDimensionality = 2*generators.Length + 1;
        GroupVertex neutralElement = neutralElementGameObject.GetComponent<GroupVertex>();
        neutralElement.Position = VectorN.Zero(simulationDimensionality);
        neutralElement.Velocity = VectorN.Zero(simulationDimensionality);
        graphManager.AddVertex(neutralElement);
        AddBorderVertex(neutralElement);

        StartCoroutine(createNewElementsAndApplyRelators());
    }


    public void setPhysics(Physik physik) {
        this.physik = physik;
    }


    public void StopVisualization() {
        StopAllCoroutines();
        randKnoten = new List<List<GroupVertex>>();
        relatorCandidates = new HashSet<GroupVertex>();
        edgeMergeCandidates = new HashSet<GroupVertex>();
        if (graphManager != null) {
            List<Vertex> vertices = new List<Vertex>(graphManager.getVertex());
            graphManager.RemoveVertex(neutralElementGameObject.GetComponent<GroupVertex>());
            vertices.Remove(neutralElementGameObject.GetComponent<GroupVertex>());
            foreach (Vertex vertex in vertices) {
                graphManager.RemoveVertex(vertex);
                vertex.Destroy();
            }
            //graphManager.ResetGraph();
        }
    }


    IEnumerator createNewElementsAndApplyRelators() {
        bool firstIteration = true;
        // ToDo: So zählen, dass bis tatsächliche Anzahl an Teilchen erreicht wird und aufhören, wenn kein Rand mehr übrig blebt. 
        while (vertexNumber > graphManager.getVertex().Count) {
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





        // Vertex is not the neutral element and an edge need to be created
        GroupVertex newVertex = Instantiate(vertexPrefab, transform).GetComponent<GroupVertex>();
        newVertex.InitializeFromPredecessor(predecessor, op, hyperbolicity);
        graphManager.AddVertex(newVertex);

        AddBorderVertex(newVertex);
        CreateEdge(predecessor, newVertex, op);

        return newVertex;
    }


    public GroupEdge CreateEdge(GroupVertex startvertex, GroupVertex endvertex, char op) {
        // If the edge already exists, no edge is created and the existing edge is returned
        foreach (GroupEdge edge in startvertex.GetEdges(op)) {
            if (edge.getOpposite(startvertex).Equals(endvertex)) {
                return edge;
            }
        }

        GroupEdge newEdge = Instantiate(edgePrefab, transform).GetComponent<GroupEdge>();
        newEdge.Initialize(startvertex, endvertex, op);

        graphManager.AddEdge(newEdge);
        return newEdge;
    }


    void AddBorderVertex(GroupVertex vertex) {
        if (randKnoten.Count <= vertex.DistanceToNeutralElement) {
            randKnoten.Add(new List<GroupVertex>());
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
                    mergeEdges(mergeCandidate);
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

    void mergeEdges(GroupVertex vertex) {
        foreach (char op in operators) {
            List<GroupEdge> generatorEdges = vertex.GetEdges(op);
            if (generatorEdges.Count > 1) {
                GroupEdge primaryEdge = generatorEdges[0];
                for (int i = 1; i < generatorEdges.Count; i++) {
                    //edgeMergeCandidates.Add(vertex); // After an edge merge a vertex might be merged with its neighbor meaning its edges can be merged again.
                    MergesVertices(primaryEdge.getOpposite(vertex), generatorEdges[i].getOpposite(vertex));
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
            for (int i = 0; i < relator.Length; i++) {
                string path = relator[i..] + relator[..i];

                GroupVertex currentElement = startingElement;
                bool relatorLeadToOtherElement = true;
                foreach (char op in path) {
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
    * Merges vertex2 and vertex1. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void MergesVertices(GroupVertex vertex1, GroupVertex vertex2) {
        if (vertex1.Equals(vertex2)) return;
        // The vertex with the longer name will be deleted. (We dont want to delete the neutral element.)
        if (vertex2.name.Length < vertex1.name.Length) {
            GroupVertex temp = vertex1;
            vertex1 = vertex2;
            vertex2 = temp;
        }

        vertex1.Merge(vertex2);

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach (char op in vertex2.GetEdges().Keys) {
            List<GroupEdge> generatorEdgesCopy = new List<GroupEdge>(vertex2.GetEdges(op));
            foreach (GroupEdge edge in generatorEdgesCopy) {
                CreateEdge(vertex1, edge.getOpposite(vertex2), op);
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
     * Taking in paths to identity and using the hyperbolicityMatrix it calculates the mass of a vertex. 
     * The mass is calculated using the geometric mean 
     **/
    public float calculateVertexMass(List<string> pathsToIdentity) {
        /**float mass = 1;
        int rootExponent = 0;
        foreach (string path in pathsToIdentity) {
            foreach (char gen in generators) {
                mass *= calculateScalingForGenerator(gen, path);
                rootExponent++;
            }
        }
        mass = Mathf.Pow(mass, 1f / rootExponent);
        if(mass == 0) {
            throw new System.Exception("The mass of the vertex is 0. This is not allowed.");
        }
        return mass;**/
        float mass = float.MaxValue;
        foreach (string path in pathsToIdentity) {
            foreach (char gen in generators) {
                float massCandidate = calculateScalingForGenerator(gen, path);
                if (massCandidate < mass) {
                    mass = massCandidate;
                }
            }
        }
        return mass;
    }

    /**
     * This is a method that would better fit into a "group generator" class.
     * Taking in paths to identity and using the hyperbolicityMatrix it calculates the length of a path. 
     * The mass is taken to be equal to the smalles branch of the vertex.
     **/
    public float calculateEdgeLength(GroupVertex v1, GroupVertex v2, char generator) {
        List<string> pathsToIdentity1 = v1.PathsToNeutralElement;
        List<string> pathsToIdentity2 = v2.PathsToNeutralElement;
        float length = float.MaxValue;
        foreach (string path in pathsToIdentity1) {
            float lengthCandidate = calculateScalingForGenerator(generator, path);
            if (lengthCandidate < length) {
                length = lengthCandidate;
            }
        }
        foreach (string path in pathsToIdentity2) {
            float lengthCandidate = calculateScalingForGenerator(generator, path);
            if (lengthCandidate < length) {
                length = lengthCandidate;
            }
        }
        return length;
    }

    /**
     * This is a method that would better fit into a "group element" class.
     * Taking in paths to identity and using the hyperbolicityMatrix it calculates the scaling of a generator. This give the desired length of an edge.
     **/
    private float calculateScalingForGenerator(char generator, string path) {
        float scaling = 1;
        foreach (char op in path) {
            scaling *= hyperbolicityMatrix[op][generator];
        }
        return scaling;
    }


    void DrawMesh() {
        foreach (GroupVertex vertex in graphManager.getVertex()) {
            foreach (string relator in relators) {
                GroupVertex[] vertices = new GroupVertex[relator.Length];
                vertices[0] = vertex;

                bool doInitialize = true;
                for (int i = 0; i < relator.Length - 1; i++) {
                    vertices[i + 1] = vertices[i].FollowEdge(relator[i]);
                    if (vertices[i + 1] == null) { doInitialize = false; break; }
                }

                if (doInitialize) {
                    MeshGenerator meshGen = Instantiate(meshPrefab, transform.position, Quaternion.identity, transform).GetComponent<MeshGenerator>();
                    meshGen.Initialize(vertices);
                    meshManager.AddMesh(meshGen);
                }
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
            hyperbolicityMatrix[generators[i]] = new Dictionary<char, float>();
            hyperbolicityMatrix[char.ToUpper(generators[i])] = new Dictionary<char, float>();
            for (int j = 0; j < matrix.GetLength(1); j++) {
                float matrixValue = matrix[i, j];
                if (matrixValue < 0) {
                    // For negative Values the hyperbolic scaling is done in one direction
                    hyperbolicityMatrix[generators[i]][generators[j]] = -matrix[i, j];
                    hyperbolicityMatrix[char.ToUpper(generators[i])][generators[j]] = -1 / matrix[i, j];
                }
                else {
                    // For positive Values the hyperbolic scaling is done in both directions
                    hyperbolicityMatrix[generators[i]][generators[j]] = matrix[i, j];
                    hyperbolicityMatrix[char.ToUpper(generators[i])][generators[j]] = matrix[i, j];
                }
            }
        }
        recalculateHyperbolicity();
    }

    /**
     * Recalculates the length of all edges according to the hyperbolicity.
     */
    private void recalculateHyperbolicity() {
        if (graphManager == null) {
            return;
        }
        /**
        List<GroupEdge> edges = graphManager.GetEdges();
        foreach (GroupEdge edge in edges) {
            edge.Length = calculateEdgeLength(edge.StartPoint, edge.EndPoint, edge.getGenerator());
        }
        foreach (GroupVertex vertex in graphManager.getVertex()) {
            vertex.Mass = calculateVertexMass(vertex.PathsToNeutralElement);
        }**/
    }


    public void setGenerators(char[] generators) {
        this.generators = generators;
    }
}
