using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Video;

public class CayleyGraph : MonoBehaviour
{
    
    public Physik physik;// = new Physik(10, 100);


    public VertexManager vertexManagererwalter = new VertexManager();
    public EdgeManager edgeManagererwalter = new EdgeManager();
    public MeshManager meshManager = new MeshManager();

    public CayleyGraphMaker cayleyGraphMaker;

    // Konfigurationen
    public GameObject knotenPrefab;
    public GameObject kantenPrefab;
    public Color[] colourList = new Color[]{new Color(0,0,255), new Color(255, 0, 0), new Color(0, 255, 0), new Color(255, 255,0 )};
    bool hatPhysik = false;
    public float präzisionsfaktor = 10f;
    public char[] generators = new char[0];
    public string[] relators = new string[0];

    

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update() {
        if(hatPhysik) {
            physik.UpdatePh(vertexManagererwalter, edgeManagererwalter, präzisionsfaktor*Time.deltaTime);
        }
    }

    public void setGenerators(string[] generators) {
        Debug.Log("Generators: " + generators);
        this.generators = string.Join("", generators).ToCharArray();
    }

    public void setRelators(string[] relators) {
        // Apply the Relotor Decoder
        for(int i = 0; i < relators.Length; i++) {
            relators[i] = RelatorDecoder.decodeRelator(relators[i]);
        }
        this.relators = relators;
    }

    public void setVertexNumber(string vertexNumber) {
        cayleyGraphMaker.setVertexNumber(int.Parse(vertexNumber));
    }

    public GameObject generatorInputField;
    public GameObject relatorInputField;
    public GameObject complexInputField;
    public GameObject vertexNumberInputField;

    public void startVisualization() {
        setGenerators(generatorInputField.GetComponent<UnityEngine.UI.InputField>().text.Replace(" ", "").Split(';'));        
        setRelators(relatorInputField.GetComponent<UnityEngine.UI.InputField>().text.Replace(" ", "").Split(';'));
        setVertexNumber(vertexNumberInputField.GetComponent<UnityEngine.UI.InputField>().text);
        cayleyGraphMaker.setPhysics(physik);
        int complexSize = int.Parse(complexInputField.GetComponent<UnityEngine.UI.InputField>().text);

        // Destroy Mesh Objects
        ICollection<MeshGenerator> meshes = meshManager.GetMeshes();
        foreach(MeshGenerator mesh in meshes) {
            Destroy(mesh.gameObject);
        }
        meshManager.resetMeshes();
        // Destroy edges Objects
        ICollection<Kante> edges = edgeManagererwalter.GetKanten();
        foreach(Kante edge in edges) {
            Destroy(edge.gameObject);
        }
        edgeManagererwalter.resetKanten();
        // Destroy vertices Objects
        ICollection<Knoten> nodes = vertexManagererwalter.getVertex();
        foreach(Knoten node in nodes) {
            Destroy(node.gameObject);
        }
        vertexManagererwalter.resetKnoten();
        physik.startUp();

        
        Debug.Log("Start Visualization");
        physik.setGenerators(generators);
        cayleyGraphMaker.InitializeCGMaker(vertexManagererwalter, edgeManagererwalter, meshManager, knotenPrefab, kantenPrefab, colourList, generators, relators, complexSize);
        hatPhysik = true;
    }


    public void setGroupAndStartVisualisation(string name, string[] generators, string[] relators) {
        Debug.Log("Set Group: " + name);
        // Set the generators and relators for the input fields and for the program
        generatorInputField.GetComponent<UnityEngine.UI.InputField>().text = string.Join("; ", generators);
        relatorInputField.GetComponent<UnityEngine.UI.InputField>().text = string.Join("; ", relators);
        setGenerators(generators);
        setRelators(relators);
        startVisualization();

    }

    public void openHelpPage() {
        Application.OpenURL("https://jeanbellynck.github.io/");
    }

    public void openGitHub() {
        Application.OpenURL("https://github.com/jeanbellynck/Cayley-Graph-Generator");
    }
}
