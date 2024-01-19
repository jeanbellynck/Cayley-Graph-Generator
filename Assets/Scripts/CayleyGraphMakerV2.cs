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
        operationColors = new Dictionary<char, Color>();
        for(int i = 0; i<generators.Length; i++) {
            operationColors.Add(generators[i], colourList[i]);
        }

        drawNeutralElement();
        StartCoroutine(createNewElementsAndApplyRelators()); 
    }


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    List<List<Knoten>> randKnoten = new List<List<Knoten>>();
    // Contains the references of al vertices which need to be checked for relator application.
    List<Knoten> relatorCandidates = new List<Knoten>();
    List<Knoten> edgeMergeCandidates = new List<Knoten>();
    

    void drawNeutralElement() {
        // Erzeugen des symmetrischen Alphabets
        Knoten neutral = createVertex(null, ' ');
        Camera.main.GetComponent<Kamera>().target = neutral.transform;
    }

    public List<Knoten> borderVertices = new List<Knoten>();

    IEnumerator createNewElementsAndApplyRelators() {
        // ToDo: So zählen, dass bis tatsächliche Anzahl an Teilchen erreicht wird und aufhören, wenn kein Rand mehr übrig blebt. 
        while(vertexNumber > knotenverwalter.GetKnoten().Count) {
            yield return new WaitForSeconds(1/drawingSpeed);

            Knoten borderVertex = GetNextBorderVertex();
            if(borderVertex == null) {
                print("No vertices remaining. Stopping.");
                break;
            }

            foreach(char gen in generators) {
                if(!borderVertex.edges.ContainsKey(gen)) {
                    Knoten newVertex = createVertex(borderVertex, gen);
                    relatorCandidates.Add(newVertex);
                }
                if(!borderVertex.edges.ContainsKey(char.ToUpper(gen))) {
                    Knoten newVertex = createVertex(borderVertex, char.ToUpper(gen));
                    relatorCandidates.Add(newVertex);
                }
            }
            //MergeAll();
            yield return MergeAll();
        }

        DrawMesh();
    }


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    */
    private Knoten createVertex(Knoten predecessor, char gen) {
        // Zufallsverschiebung
        Vector3 elementPosition = Vector3.zero;
        Knoten neuerKnoten;
        if(predecessor == null) {
            // Vertex is most likely the neutral element
            neuerKnoten = Instantiate(knotenPrefab, transform.position+elementPosition, UnityEngine.Quaternion.identity, transform).GetComponent<Knoten>();
            neuerKnoten.name = "";
            neuerKnoten.distanceToNeutralElement = 0;
            neuerKnoten.transform.position = Vector3.zero;
            addBorderVertex(neuerKnoten, 0);
        } else {
            // Vertex is not the neutral element and an edge need to be created
            System.Random r = new System.Random();
            elementPosition = predecessor.transform.position + 0.01f * new Vector3(r.Next(-100, 100), r.Next(-100, 100), r.Next(-100, 100));
            neuerKnoten = Instantiate(knotenPrefab, transform.position+elementPosition, UnityEngine.Quaternion.identity, transform).GetComponent<Knoten>();
            neuerKnoten.name = predecessor.name + gen;
            neuerKnoten.distanceToNeutralElement = predecessor.distanceToNeutralElement + 1;
            addBorderVertex(neuerKnoten, neuerKnoten.distanceToNeutralElement);
            createEdge(predecessor, neuerKnoten, gen);
        }
        
        knotenverwalter.AddKnoten(neuerKnoten);
        return neuerKnoten;
    }

    void createEdge(Knoten startvertex, Knoten endvertex, char op) {
        // Kante erstellen
        Kante neueKante = Instantiate(kantenPrefab, transform).GetComponent<Kante>();
        neueKante.SetFarbe(operationColors[char.ToLower(op)], new Color(100,100,100));
        neueKante.name = char.ToLower(op) + "";
 
        if(char.IsLower(op)) {
            neueKante.SetEndpunkte(startvertex, endvertex);
        }else{
            neueKante.SetEndpunkte(endvertex, startvertex);
        }
        kantenverwalter.AddKante(neueKante);
    }
    
    void addBorderVertex(Knoten knoten, int distance) {
        if(randKnoten.Count <= distance) {
            randKnoten.Add(new List<Knoten>());
        }
        knoten.distanceToNeutralElement = distance;
        randKnoten[distance].Add(knoten);
    }
    
    public Knoten GetNextBorderVertex() {
        Knoten nextVertex = null;
        foreach(List<Knoten> borderVertices in randKnoten) {
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
            Knoten mergeCandidate;
            // If two edges of the same generator lead to the different vertices, they need to be merged as fast as possible. Otherwise following generators is yucky.
            if(edgeMergeCandidates.Count > 0) {
                mergeCandidate = edgeMergeCandidates[0];
                edgeMergeCandidates.RemoveAt(0);
                if(mergeCandidate != null && knotenverwalter.ContainsKnoten(mergeCandidate)) {
                    mergeEdges(mergeCandidate);
                }
            } else {
                mergeCandidate = relatorCandidates[0];
                relatorCandidates.RemoveAt(0);
                if(mergeCandidate != null && knotenverwalter.ContainsKnoten(mergeCandidate)) {
                    mergeByRelator(mergeCandidate);
                }
            }
            print("Merged all vertices. Adding new Vertex.");
        }
    }

    void mergeEdges(Knoten knoten) {
        // Can be optimzed to use edge information inside the vertices.
        Dictionary<char, Knoten> geprüfteKanten = new Dictionary<char, Knoten>();
        foreach(Knoten ausgehenderKnoten in kantenverwalter.GetAusgehendeKnoten(knoten)) {
            char aktuelleOp = kantenverwalter.GetKante(knoten.name, ausgehenderKnoten.name).name[0];
            
            if(geprüfteKanten.ContainsKey(aktuelleOp)) {
                edgeMergeCandidates.Add(knoten);
                elementeVereinen(geprüfteKanten[aktuelleOp], ausgehenderKnoten);
            } else {
                geprüfteKanten.Add(aktuelleOp, ausgehenderKnoten);
            }
        }
        geprüfteKanten = new Dictionary<char, Knoten>();
        foreach(Knoten eingehenderKnoten in kantenverwalter.GetEingehendeKnoten(knoten)) {
            char aktuelleOp = kantenverwalter.GetKante(eingehenderKnoten.name, knoten.name).name[0];
            
            if(geprüfteKanten.ContainsKey(aktuelleOp)) {
                edgeMergeCandidates.Add(knoten);
                elementeVereinen(geprüfteKanten[aktuelleOp], eingehenderKnoten);
            } else {
                geprüfteKanten.Add(aktuelleOp, eingehenderKnoten);
            }
        }
    }


    /**
    * Applies the relator to the given groupElement. For that the relator is followed, starting at a different string index each time. 
    * If the relator leads to an other group element, the two groupElements are merged.
    */
    void mergeByRelator(Knoten anfangselement) {
        // Der Code ist ein wenig unoptimiert. Nach dem Anwenden eines Relators versucht er wieder alle anzuwenden. 
        // Dadurch steigt die Komplexität im Worst-Case zu n^2 falls alle Relatoren genutzt werden. (Was natürlichunwahrscheinlich ist)
        foreach(string relator in relators) {
            for(int i = 0; i<relator.Length; i++) {
                string path = relator[i..] + relator[..i];

                Knoten aktuellesElement = anfangselement;
                bool relatorLeadToOtherElement = true;
                foreach(char op in path) {
                    aktuellesElement = kantenverwalter.kanteFolgen(aktuellesElement, op);
                    if(aktuellesElement == null) {
                        relatorLeadToOtherElement = false;
                        break;
                    }
                }
                if(relatorLeadToOtherElement) {
                    elementeVereinen(anfangselement, aktuellesElement);
                    return;
                }
            }      
        }
        
    }

    /**
    * Merges the two given groupElements. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void elementeVereinen(Knoten zweig1, Knoten zweig2) {
        if(zweig1.Equals(zweig2)) {
            return;
        }
        // Neuen Knoten erstellen, soll den Namen des kürzeren Wegs tragen
        Knoten kurzesWort;
        Knoten langesWort;
        if(zweig1.name.Length <= zweig2.name.Length) {
            kurzesWort = zweig1;
            langesWort = zweig2;
        } else {
            kurzesWort = zweig2;
            langesWort = zweig1;
        }

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach(Knoten ausgehenderKnoten in kantenverwalter.GetAusgehendeKnoten(langesWort)) {
            Kante kante = kantenverwalter.GetKante(langesWort.name, ausgehenderKnoten.name);
            if(kantenverwalter.ContainsKante(kurzesWort.name, ausgehenderKnoten.name)) {
                Debug.Assert(kantenverwalter.GetKante(kurzesWort.name, ausgehenderKnoten.name).name == kante.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                // Duplikat löschen
                Destroy(kante.gameObject);
            } else {
                kante.SetStartpunkt(kurzesWort);
                kantenverwalter.AddKante(kante);
            }
            kantenverwalter.RemoveKante(langesWort.name, ausgehenderKnoten.name);
        }
        foreach(Knoten eingehenderKnoten in kantenverwalter.GetEingehendeKnoten(langesWort)) {
            Kante kante = kantenverwalter.GetKante(eingehenderKnoten.name, langesWort.name);
            if(kantenverwalter.ContainsKante(eingehenderKnoten.name, kurzesWort.name)) {
                Debug.Assert(kantenverwalter.GetKante(eingehenderKnoten.name, kurzesWort.name).name == kante.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                Destroy(kante.gameObject);
            } else {
                kante.SetEndpunkt(kurzesWort);
                kantenverwalter.AddKante(kante);
            }
            kantenverwalter.RemoveKante(eingehenderKnoten.name, langesWort.name);
        }
        
        // Anderen Knoten löschen
        knotenverwalter.RemoveKnoten(langesWort);
        Destroy(langesWort.gameObject);

        // Aktuellen Knoten sicherheitshalber nochmal prüfen
        addBorderVertex(kurzesWort, kurzesWort.distanceToNeutralElement);
        edgeMergeCandidates.Add(kurzesWort);
        relatorCandidates.Add(kurzesWort);
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
                Knoten nextVertex = kantenverwalter.kanteFolgen(vertex, gen);
                if(nextVertex != null) {
                    cycles.AddRange(FindCyclesOfLength(length-1, nextVertex, takenPath + gen));
                }
            }
            if(takenPath == "" || gen != takenPath[takenPath.Length-1]) {
                Knoten nextVertex = kantenverwalter.kanteFolgen(vertex, char.ToUpper(gen));
                if(nextVertex != null) {
                    cycles.AddRange(FindCyclesOfLength(length-1, nextVertex, takenPath + char.ToUpper(gen)));
                }
            }
        }
        
        return cycles.ToArray();
    }**/

    void DrawMesh() {
        foreach(Knoten knoten in knotenverwalter.GetKnoten()) {
            foreach(string relator in relators) {
                Knoten[] vertices = new Knoten[relator.Length];
                vertices[0] = knoten;

                bool doInitialize = true;
                for(int i = 0; i < relator.Length-1; i++) {
                    vertices[i+1] = kantenverwalter.kanteFolgen(vertices[i], relator[i]);
                    if(vertices[i+1] == null) {doInitialize = false;break;}	
                }
                
                if(doInitialize) {
                    MeshGenerator meshGen = Instantiate(meshPrefab, transform.position, UnityEngine.Quaternion.identity, transform).GetComponent<MeshGenerator>();
                    meshGen.Initialize(vertices);
                    meshManager.AddMesh(meshGen);
                }
            }

            //if(knoten.name != "") {break;}
        }
        
    }

    internal override void setVertexNumber(int v)
    {
        vertexNumber = v;
    }
}
