using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CayleyGraphMakerV2 : CayleyGraphMaker
{
    IDictionary<char, Color> operationColors;

    // Konfigurationen
    public int vertexNumber; // Describes the number of vertices the graph should have
    public float drawingSpeed = 1; // Describes the speed at which new vertices should be drawn in vertices per second 

    public GameObject meshPrefab;    
    

    // Start is called before the first frame update
    public override void InitializeCGMaker()
    {
        StopAllCoroutines();
        operationColors = new Dictionary<char, Color>();
        for(int i = 0; i<generators.Length; i++) {
            if(i < colourList.Length) {
                operationColors.Add(generators[i], colourList[i]);
            }else{
                operationColors.Add(generators[i], new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255)));
            }
            
        }

        drawNeutralElement();
        StartCoroutine(createNewElementsAndApplyRelators()); 
    }


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    List<List<Vertex>> randKnoten = new List<List<Vertex>>();
    // Contains the references of al vertices which need to be checked for relator application.
    List<Vertex> relatorCandidates = new List<Vertex>();
    List<Vertex> edgeMergeCandidates = new List<Vertex>();
    

    void drawNeutralElement() {
        // Erzeugen des symmetrischen Alphabets
        Vertex neutral = createVertex(null, ' ');
        Camera.main.GetComponent<Kamera>().target = neutral.transform;
    }

    public List<Vertex> borderVertices = new List<Vertex>();

    IEnumerator createNewElementsAndApplyRelators() {
        // ToDo: So zählen, dass bis tatsächliche Anzahl an Teilchen erreicht wird und aufhören, wenn kein Rand mehr übrig blebt. 
        while(vertexNumber > vertexManager.getVertex().Count) {
            yield return new WaitForSeconds(1/drawingSpeed);

            Vertex borderVertex = GetNextBorderVertex();
            if(borderVertex == null) {
                print("No vertices remaining. Stopping.");
                break;
            }

            foreach(char gen in generators) {
                if(!borderVertex.edges.ContainsKey(gen)) {
                    Vertex newVertex = createVertex(borderVertex, gen);
                    relatorCandidates.Add(newVertex);
                }
                if(!borderVertex.edges.ContainsKey(char.ToUpper(gen))) {
                    Vertex newVertex = createVertex(borderVertex, char.ToUpper(gen));
                    relatorCandidates.Add(newVertex);
                }
            }
            //MergeAll();
            yield return MergeAll();
        }

        DrawMesh();
        physik.shutDown();
    }


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    */
    private Vertex createVertex(Vertex predecessor, char gen) {
        // Zufallsverschiebung
        Vector3 elementPosition = Vector3.zero;
        Vertex neuerKnoten;
        if(predecessor == null) {
            // Vertex is most likely the neutral element
            neuerKnoten = Instantiate(knotenPrefab, transform.position+elementPosition, UnityEngine.Quaternion.identity, transform).GetComponent<Vertex>();
            neuerKnoten.name = "";
            neuerKnoten.distanceToNeutralElement = 0;
            neuerKnoten.transform.position = Vector3.zero;
            AddBorderVertex(neuerKnoten, 0);
        } else {
            // Vertex is not the neutral element and an edge need to be created
            System.Random r = new System.Random();
            elementPosition = predecessor.transform.position + 0.01f * new Vector3(r.Next(-100, 100), r.Next(-100, 100), r.Next(-100, 100));
            neuerKnoten = Instantiate(knotenPrefab, transform.position+elementPosition, UnityEngine.Quaternion.identity, transform).GetComponent<Vertex>();
            neuerKnoten.name = predecessor.name + gen;
            neuerKnoten.distanceToNeutralElement = predecessor.distanceToNeutralElement + 1;
            AddBorderVertex(neuerKnoten, neuerKnoten.distanceToNeutralElement);
            createEdge(predecessor, neuerKnoten, gen);
        }
        
        vertexManager.AddKnoten(neuerKnoten);
        return neuerKnoten;
    }

    void createEdge(Vertex startvertex, Vertex endvertex, char op) {
        // Kante erstellen
        Kante neueKante = Instantiate(kantenPrefab, transform).GetComponent<Kante>();
        neueKante.SetFarbe(operationColors[char.ToLower(op)], new Color(100,100,100));
        neueKante.name = char.ToLower(op) + "";
 
        if(char.IsLower(op)) {
            neueKante.SetEndpoints(startvertex, endvertex);
        }else{
            neueKante.SetEndpoints(endvertex, startvertex);
        }
        edgeManager.AddEdge(neueKante);
    }
    
    void AddBorderVertex(Vertex vertex, int distance) {
        if(randKnoten.Count <= distance) {
            randKnoten.Add(new List<Vertex>());
        }
        vertex.distanceToNeutralElement = distance;
        randKnoten[distance].Add(vertex);
    }
    
    public Vertex GetNextBorderVertex() {
        Vertex nextVertex = null;
        foreach(List<Vertex> borderVertices in randKnoten) {
            borderVertices.RemoveAll(item => item == null);
            if(borderVertices.Count > 0) {
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
    IEnumerator MergeAll() {
        while(edgeMergeCandidates.Count > 0 || relatorCandidates.Count > 0) {
            yield return new WaitForSeconds(1/drawingSpeed);
            Vertex mergeCandidate;
            // If two edges of the same generator lead to the different vertices, they need to be merged as fast as possible. Otherwise following generators is yucky.
            if(edgeMergeCandidates.Count > 0) {
                mergeCandidate = edgeMergeCandidates[0];
                edgeMergeCandidates.RemoveAt(0);
                if(mergeCandidate != null && vertexManager.ContainsVertex(mergeCandidate)) {
                    mergeEdges(mergeCandidate);
                }
            } else {
                mergeCandidate = relatorCandidates[0];
                relatorCandidates.RemoveAt(0);
                if(mergeCandidate != null && vertexManager.ContainsVertex(mergeCandidate)) {
                    MergeByRelator(mergeCandidate);
                }
            }
            print("Merged all vertices. Adding new Vertex.");
        }
    }

    void mergeEdges(Vertex vertex) {
        // Can be optimzed to use edge information inside the vertices.
        Dictionary<char, Vertex> checkedEdges = new Dictionary<char, Vertex>();
        foreach(Vertex outgoingVertex in edgeManager.GetOutgoingVertices(vertex)) {
            char aktuelleOp = edgeManager.GetEdge(vertex.name, outgoingVertex.name).name[0];
            
            if(checkedEdges.ContainsKey(aktuelleOp)) {
                edgeMergeCandidates.Add(vertex);
                MergesVertices(checkedEdges[aktuelleOp], outgoingVertex);
            } else {
                checkedEdges.Add(aktuelleOp, outgoingVertex);
            }
        }
        checkedEdges = new Dictionary<char, Vertex>();
        foreach(Vertex ingoingVertex in edgeManager.GetIngoingVertices(vertex)) {
            char aktuelleOp = edgeManager.GetEdge(ingoingVertex.name, vertex.name).name[0];
            
            if(checkedEdges.ContainsKey(aktuelleOp)) {
                edgeMergeCandidates.Add(vertex);
                MergesVertices(checkedEdges[aktuelleOp], ingoingVertex);
            } else {
                checkedEdges.Add(aktuelleOp, ingoingVertex);
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
        foreach(string relator in relators) {
            for(int i = 0; i<relator.Length; i++) {
                string path = relator[i..] + relator[..i];

                Vertex currentElement = startingElement;
                bool relatorLeadToOtherElement = true;
                foreach(char op in path) {
                    currentElement = edgeManager.followEdge(currentElement, op);
                    if(currentElement == null) {
                        relatorLeadToOtherElement = false;
                        break;
                    }
                }
                if(relatorLeadToOtherElement) {
                    MergesVertices(startingElement, currentElement);
                    return;
                }
            }      
        }
        
    }

    /**
    * Merges the two given groupElements. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void MergesVertices(Vertex branch1, Vertex branch2) {
        if(branch1.Equals(branch2)) {
            return;
        }
        // Neuen Knoten erstellen, soll den Namen des kürzeren Wegs tragen
        Vertex shortWord;
        Vertex longWord;
        if(branch1.name.Length <= branch2.name.Length) {
            shortWord = branch1;
            longWord = branch2;
        } else {
            shortWord = branch2;
            longWord = branch1;
        }

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach(Vertex outgoingVertex in edgeManager.GetOutgoingVertices(longWord)) {
            Kante edge = edgeManager.GetEdge(longWord.name, outgoingVertex.name);
            if(edgeManager.ContainsEdge(shortWord.name, outgoingVertex.name)) {
                Debug.Assert(edgeManager.GetEdge(shortWord.name, outgoingVertex.name).name == edge.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                // Duplikat löschen
                Destroy(edge.gameObject);
            } else {
                edge.setStart(shortWord);
                edgeManager.AddEdge(edge);
            }
            edgeManager.RemoveEdge(longWord.name, outgoingVertex.name);
        }
        foreach(Vertex ingoingVertex in edgeManager.GetIngoingVertices(longWord)) {
            Kante edge = edgeManager.GetEdge(ingoingVertex.name, longWord.name);
            if(edgeManager.ContainsEdge(ingoingVertex.name, shortWord.name)) {
                Debug.Assert(edgeManager.GetEdge(ingoingVertex.name, shortWord.name).name == edge.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                Destroy(edge.gameObject);
            } else {
                edge.SetEnd(shortWord);
                edgeManager.AddEdge(edge);
            }
            edgeManager.RemoveEdge(ingoingVertex.name, longWord.name);
        }
        
        // Anderen Knoten löschen
        vertexManager.RemoveVertex(longWord);
        Destroy(longWord.gameObject);

        // Aktuellen Knoten sicherheitshalber nochmal prüfen
        AddBorderVertex(shortWord, shortWord.distanceToNeutralElement);
        edgeMergeCandidates.Add(shortWord);
        relatorCandidates.Add(shortWord);
    }

    
    /**
    * Finds all cycles of the given length in the graph which begin at the neutral element.
    */
    /**
    string[] FindCyclesOfLength(int length, Knoten vertex, string takenPath) {
        List<string> cycles = new List<string>();
        if(length == 0) {
            if(vertex.id == 0 && takenPath.Length > 0) {
                cycles.Add(takenPath);
            }
            return cycles.ToArray();
        }
        
        foreach(char gen in generators) {
            if(takenPath == "" || char.ToUpper(gen) != takenPath[takenPath.Length-1]) {
                Knoten nextVertex = kantenverwalter.followEdge(vertex, gen);
                if(nextVertex != null) {
                    cycles.AddRange(FindCyclesOfLength(length-1, nextVertex, takenPath + gen));
                }
            }
            if(takenPath == "" || gen != takenPath[takenPath.Length-1]) {
                Knoten nextVertex = kantenverwalter.followEdge(vertex, char.ToUpper(gen));
                if(nextVertex != null) {
                    cycles.AddRange(FindCyclesOfLength(length-1, nextVertex, takenPath + char.ToUpper(gen)));
                }
            }
        }
        
        return cycles.ToArray();
    }**/

    void DrawMesh() {
        foreach(Vertex vertex in vertexManager.getVertex()) {
            foreach(string relator in relators) {
                Vertex[] vertices = new Vertex[relator.Length];
                vertices[0] = vertex;

                bool doInitialize = true;
                for(int i = 0; i < relator.Length-1; i++) {
                    vertices[i+1] = edgeManager.followEdge(vertices[i], relator[i]);
                    if(vertices[i+1] == null) {doInitialize = false;break;}	
                }
                
                if(doInitialize) {
                    MeshGenerator meshGen = Instantiate(meshPrefab, transform.position, UnityEngine.Quaternion.identity, transform).GetComponent<MeshGenerator>();
                    meshGen.Initialize(vertices);
                    meshManager.AddMesh(meshGen);
                }
            }

            //if(vertex.name != "") {break;}
        }
        
    }

    internal override void setVertexNumber(int v)
    {
        vertexNumber = v;
    }
}
