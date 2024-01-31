using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Video;

public class CayleyGraph : MonoBehaviour
{
    
    public Physik physik;// = new Physik(10, 100);


    public GraphManager graphManager;
    public MeshManager meshManager = new MeshManager();

    public CayleyGraphMaker cayleyGraphMaker;

    // Konfigurationen
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
            physik.UpdatePh(graphManager, präzisionsfaktor*Time.deltaTime);
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
    public GameObject vertexNumberInputField;

    public void startVisualization() {
        setGenerators(generatorInputField.GetComponent<UnityEngine.UI.InputField>().text.Replace(" ", "").Split(';'));        
        setRelators(relatorInputField.GetComponent<UnityEngine.UI.InputField>().text.Replace(" ", "").Split(';'));
        setVertexNumber(vertexNumberInputField.GetComponent<UnityEngine.UI.InputField>().text);
        cayleyGraphMaker.setPhysics(physik);

        // Destroy Mesh Objects
        ICollection<MeshGenerator> meshes = meshManager.GetMeshes();
        foreach(MeshGenerator mesh in meshes) {
            Destroy(mesh.gameObject);
        }
        meshManager.resetMeshes();
        graphManager.ResetGraph();
        physik.startUp();

        
        Debug.Log("Start Visualization");
        physik.setGenerators(generators);
        cayleyGraphMaker.InitializeCGMaker(graphManager, meshManager, generators, relators);
        graphManager.Initialize(generators);
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
