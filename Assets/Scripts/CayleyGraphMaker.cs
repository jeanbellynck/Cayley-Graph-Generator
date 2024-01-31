using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class CayleyGraphMaker : MonoBehaviour {
    private GraphManager graphManager;
    private MeshManager meshManager;
    private Physik physik; // I wonder whether the reference to Physics is necessary? 


    
    protected char[] generators;// = new char[]{'a', 'b', 'c'};
    protected char[] operators; // Like generators but with upper and lower case letters
    protected string[] relators;// = new string[]{"abAB"};


    // Konfigurationen
    public int vertexNumber; // Describes the number of vertices the graph should have. It would be better to have a config file with all the data.
    public float drawingSpeed = 1; // Describes the speed at which new vertices should be drawn in vertices per second 

    public GameObject meshPrefab;


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    List<List<Vertex>> randKnoten = new List<List<Vertex>>();
    // Contains the references of al vertices which need to be checked for relator application.
    HashSet<Vertex> relatorCandidates = new HashSet<Vertex>();
    HashSet<Vertex> edgeMergeCandidates = new HashSet<Vertex>();

    public void InitializeCGMaker(GraphManager graphManager, MeshManager meshManager, char[] generators, string[] relators) {
        this.graphManager = graphManager;
        this.meshManager = meshManager;
        this.generators = generators;
        this.relators = relators;
        this.operators = new char[2 * generators.Length];
        for (int i = 0; i < generators.Length; i++) {
            operators[i] = char.ToLower(generators[i]);
            operators[i + generators.Length] = char.ToUpper(generators[i]);
        }
        InitializeCGMaker();
    }

    public void setPhysics(Physik physik) {
        this.physik = physik;
    }

    // Start is called before the first frame update
    public void InitializeCGMaker() {
        StopAllCoroutines();
        
        randKnoten = new List<List<Vertex>>();
        relatorCandidates = new HashSet<Vertex>();
        edgeMergeCandidates = new HashSet<Vertex>();

        AddBorderVertex(graphManager.getNeutral());
        StartCoroutine(createNewElementsAndApplyRelators());
    }


    //public List<Vertex> borderVertices = new List<Vertex>();

    IEnumerator createNewElementsAndApplyRelators() {
        // ToDo: So zählen, dass bis tatsächliche Anzahl an Teilchen erreicht wird und aufhören, wenn kein Rand mehr übrig blebt. 
        while (vertexNumber > graphManager.getVertex().Count) {
            yield return new WaitForSeconds(1 / drawingSpeed);

            Vertex borderVertex = GetNextBorderVertex();
            if (borderVertex == null) {
                print("No vertices remaining. Stopping.");
                break;
            }

            foreach (char gen in generators) {
                if (!borderVertex.GetEdges().ContainsKey(gen) || borderVertex.GetEdges()[gen].Count == 0){
                    Vertex newVertex = CreateVertex(borderVertex, gen);
                    relatorCandidates.Add(newVertex);
                }
                if (!borderVertex.GetEdges().ContainsKey(char.ToUpper(gen)) || borderVertex.GetEdges()[char.ToUpper(gen)].Count == 0) {
                    Vertex newVertex = CreateVertex(borderVertex, char.ToUpper(gen));
                    relatorCandidates.Add(newVertex);
                }
            }
            //MergeAll();
            MergeAll();
        }

        DrawMesh();
        physik.shutDown();
    }


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    */
    private Vertex CreateVertex(Vertex predecessor, char gen) {
        // Zufallsverschiebung
        System.Random r = new System.Random();
        Vector3 elementPosition = predecessor.transform.position + 0.01f * new Vector3(r.Next(-100, 100), r.Next(-100, 100), r.Next(-100, 100));

        // Vertex is not the neutral element and an edge need to be created
        Vertex neuerKnoten = graphManager.CreateVertex(elementPosition);
        neuerKnoten.name = predecessor.name + gen;
        neuerKnoten.distanceToNeutralElement = predecessor.distanceToNeutralElement + 1;
        AddBorderVertex(neuerKnoten);
        createEdge(predecessor, neuerKnoten, gen);
        

        return neuerKnoten;
    }

    public char ToggleCase(char c) {
        if (char.IsUpper(c)) {
            return char.ToLower(c);
        }
        else {
            return char.ToUpper(c);
        }
    }


    void createEdge(Vertex startvertex, Vertex endvertex, char op) {
        // Kante erstellen
        Edge newEdge = graphManager.CreateEdge(startvertex, endvertex, op);
    }

    void AddBorderVertex(Vertex vertex) {
        if (randKnoten.Count <= vertex.distanceToNeutralElement) {
            randKnoten.Add(new List<Vertex>());
        }

        randKnoten[vertex.distanceToNeutralElement].Add(vertex);
    }

    public Vertex GetNextBorderVertex() {
        Vertex nextVertex = null;
        foreach (List<Vertex> borderVertices in randKnoten) {
            borderVertices.RemoveAll(item => item == null);
            if (borderVertices.Count > 0) {
                nextVertex = borderVertices.First();
                borderVertices.RemoveAt(0);
                return nextVertex;
            }
        }
        return null;
    }

    /**
    * Applies the relators to all groupElements in the mergeCandidates list.
    */
    private void MergeAll() {
        while (edgeMergeCandidates.Count > 0 || relatorCandidates.Count > 0) {
            Vertex mergeCandidate;
            // If two edges of the same generator lead to the different vertices, they need to be merged as fast as possible. Otherwise following generators is yucky.
            if (edgeMergeCandidates.Count > 0) {
                mergeCandidate = edgeMergeCandidates.First();
                edgeMergeCandidates.Remove(mergeCandidate);
                if (mergeCandidate != null && mergeCandidate.isActive) { // Might not be necessary
                    mergeEdges(mergeCandidate);
                }
            }
            else {
                mergeCandidate = relatorCandidates.First();
                relatorCandidates.Remove(mergeCandidate);
                if (mergeCandidate != null && mergeCandidate.isActive) {
                    MergeByRelator(mergeCandidate);
                }
            }
            print("Merged all vertices. Adding new Vertex.");
        }
    }

    void mergeEdges(Vertex vertex) {
        foreach (KeyValuePair<char, List<Edge>> entry in vertex.GetEdges()) {
            List<Edge> generatorsEdges = entry.Value;
            if (generatorsEdges.Count > 0) {
                Edge primaryEdge = generatorsEdges[0];
                for (int i = 1; i < generatorsEdges.Count; i++) {
                    //edgeMergeCandidates.Add(vertex); // After an edge merge a vertex might be merged with its neighbor meaning its edges can be merged again.
                    MergesVertices(primaryEdge.getOpposite(vertex), generatorsEdges[i].getOpposite(vertex));
                }
            }

        }
    }


    /**
    * Applies the relator to the given groupElement. For that the relator is followed, starting at a different string index each time. 
    * If the relator leads to an other group element, the two groupElements are merged.
    */
    void MergeByRelator(Vertex startingElement) {
        // Der Code ist ein wenig unoptimiert. Nach dem Anwenden eines Relators versucht er wieder alle anzuwenden. 
        // Dadurch steigt die Komplexität im Worst-Case zu n^2 falls alle Relatoren genutzt werden. (Was natürlichunwahrscheinlich ist)
        foreach (string relator in relators) {
            for (int i = 0; i < relator.Length; i++) {
                string path = relator[i..] + relator[..i];

                Vertex currentElement = startingElement;
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
    * Merges vertex2 with vertex1. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void MergesVertices(Vertex vertex1, Vertex vertex2) {
        if (vertex1.Equals(vertex2)) {
            return;
        }

        // New vertex should hav ethe shortest distance and carry the shorter name
        int distanceToNeutralElement = Mathf.Min(vertex1.distanceToNeutralElement, vertex2.distanceToNeutralElement);
        string newName;
        if (vertex1.name.Length <= vertex2.name.Length) {
            newName = vertex1.name;
        }
        else {
            newName = vertex2.name;
        }

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        //Dictionary<char, List<Edge>> vertex2edges = vertex2.GetEdges();
        foreach (char op in vertex2.GetEdges().Keys) {
            List<Edge> generatorEdgesCopy = new List<Edge>(vertex2.GetEdges(op));
            
            foreach (Edge edge in generatorEdgesCopy) {
                graphManager.CreateEdge(vertex1, edge.getOpposite(vertex2), op);
                graphManager.RemoveEdge(edge);
            }
        }

        // Update data of vertex1
        vertex1.name = newName;
        vertex1.distanceToNeutralElement = distanceToNeutralElement;

        // Delete vertex2
        graphManager.RemoveVertex(vertex2);

        // Aktuellen Knoten sicherheitshalber nochmal prüfen
        edgeMergeCandidates.Add(vertex1);
        relatorCandidates.Add(vertex1);
    }


    void DrawMesh() {
        foreach (Vertex vertex in graphManager.getVertex()) {
            foreach (string relator in relators) {
                Vertex[] vertices = new Vertex[relator.Length];
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
}
