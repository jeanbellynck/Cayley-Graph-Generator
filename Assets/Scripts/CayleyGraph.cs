using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Video;

public class CayleyGraph : MonoBehaviour
{
    
    public Physik physik;// = new Physik(10, 100);


    public Knotenverwalter knotenVerwalter = new Knotenverwalter();
    public Kantenverwalter kantenVerwalter = new Kantenverwalter();
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
            physik.UpdatePh(knotenVerwalter, kantenVerwalter, präzisionsfaktor*Time.deltaTime);
        }
    }

    public void setGenerators(string generators) {
        Debug.Log("Generators: " + generators);
        generators = generators.Replace(" ", string.Empty).Replace(",", string.Empty);
        this.generators = generators.ToCharArray();
    }

    public void setRelators(string relators) {
        Debug.Log("Relators: " + relators);
        relators = relators.Replace(" ", string.Empty);
        this.relators = relators.Split(',');
    }

    public void setVertexNumber(string vertexNumber) {
        cayleyGraphMaker.setVertexNumber(int.Parse(vertexNumber));
    }

    public GameObject generatorInputField;
    public GameObject relatorInputField;
    public GameObject complexInputField;
    public GameObject vertexNumberInputField;

    public void startVisualization() {
        setGenerators(generatorInputField.GetComponent<UnityEngine.UI.InputField>().text);        
        setRelators(relatorInputField.GetComponent<UnityEngine.UI.InputField>().text);
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
        ICollection<Kante> edges = kantenVerwalter.GetKanten();
        foreach(Kante edge in edges) {
            Destroy(edge.gameObject);
        }
        kantenVerwalter.resetKanten();
        // Destroy vertices Objects
        ICollection<Knoten> nodes = knotenVerwalter.GetKnoten();
        foreach(Knoten node in nodes) {
            Destroy(node.gameObject);
        }
        knotenVerwalter.resetKnoten();
        physik.startUp();

        
        Debug.Log("Start Visualization");
        physik.setGenerators(generators);
        cayleyGraphMaker.InitializeCGMaker(knotenVerwalter, kantenVerwalter, meshManager, knotenPrefab, kantenPrefab, colourList, generators, relators, complexSize);
        hatPhysik = true;
    }


    public void setGroupAndStartVisualisation(string name, string generators, string relators) {
        Debug.Log("Set Group: " + name);
        // Set the generators and relators for the input fields and for the program
        generatorInputField.GetComponent<UnityEngine.UI.InputField>().text = generators;
        relatorInputField.GetComponent<UnityEngine.UI.InputField>().text = relators;
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
