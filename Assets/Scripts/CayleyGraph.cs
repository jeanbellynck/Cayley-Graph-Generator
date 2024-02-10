using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/**
 * This class is both the main class for the graph and the interface for the UI.
 */
public class CayleyGraph : MonoBehaviour {

    public Physik physik;// = new Physik(10, 100);


    public GraphManager graphManager;
    public MeshManager meshManager = new MeshManager();

    public CayleyGraphMaker cayleyGraphMaker;


    // These values are probably better moved to CayleyGraphMaker entirely
    public char[] generators = new char[0];
    public string[] relators = new string[0];


    public float Activity => Math.Clamp(physik.alpha, 0f, 1f);

    // Start is called before the first frame update
    void Start() {
        setGenerators("a, b");
        setRelators("[a, b], a^5");
        cayleyGraphMaker.setPhysics(physik);
    }


    // Update is called once per frame
    void Update() {
    }

    public void setGenerators(string generatorString) {
        generatorString = generatorString.Replace(" ", "").Replace(";", "").Replace(",", "");
        char[] newGenerators = string.Join("", generatorString).ToCharArray();
        if(!newGenerators.Equals(generators)) {
            generators = newGenerators;
            cayleyGraphMaker.setGenerators(generators);
            hyperbolicityMatrix.SetMatrixSize(generators.Length);
        }

    }

    public void setRelators(string relators) {
        if (relators.Equals("")) {
            this.relators = new string[0];
            return;
        }

        // Apply the Relotor Decoder
        this.relators = RelatorDecoder.decodeRelators(relators);

    }

    public void setVertexNumber(string vertexNumber) {
        cayleyGraphMaker.setVertexNumber(int.Parse(vertexNumber));
    }

    public InputField generatorInputField;
    public InputField relatorInputField;
    public InputField vertexNumberInputField;
    public InputField hyperbolicityInputField;
    public HyperbolicityMatrix hyperbolicityMatrix;
    public InputField dimensionInputField;


    public void generateButton() {
        StopVisualization();
        setRelators(relatorInputField.text);
        StartVisualization();
    }

    public void StopVisualization() {
        physik.StopAllCoroutines();
        cayleyGraphMaker.StopVisualization();
        // Destroy Mesh Objects
        ICollection<MeshGenerator> meshes = meshManager.GetMeshes();
        foreach (MeshGenerator mesh in meshes) {
            Destroy(mesh.gameObject);
        }
        meshManager.resetMeshes();
        graphManager.ResetGraph();
    }

    public void StartVisualization() {
        setVertexNumber(vertexNumberInputField.text);
        graphManager.Initialize(generators);
        physik.startUp(graphManager);
        string dimensionString = dimensionInputField.text;
        int dimension = 3;
        if(int.TryParse(dimensionString, out int dimensionValue) && dimension > 0) {
            dimension = dimensionValue;
        } 
        cayleyGraphMaker.StartVisualization(graphManager, meshManager, generators, relators, dimension);
    }


    public void SelectGroupOption(string name, string generatorString, string relatorString) {
        StopVisualization();
        Debug.Log("Set Group: " + name);
        // Set the generators and relators for the input fields and for the program
        generatorInputField.text = string.Join(", ", generatorString);
        relatorInputField.text = string.Join(", ", relatorString);
        // Generator is automatically updated on value change
        //setGenerators(generatorString); 
        setRelators(relatorString);
        StartVisualization();
    }

    public void openHelpPage() {
        Application.OpenURL("https://jeanbellynck.github.io/");
    }

    public void openGitHub() {
        Application.OpenURL("https://github.com/jeanbellynck/Cayley-Graph-Generator");
    }


    public void SetHyperbolicity(string hyperbolicityString) {
        print("scaling changed to:" + hyperbolicityString);
        if (float.TryParse(hyperbolicityString, out float hyperbolicity) && hyperbolicity != 0) {
            cayleyGraphMaker.setHyperbolicity(hyperbolicity);
        }
    }


    public void SetHyperbolicityMatrix(float[,] matrix) {
        cayleyGraphMaker.SetHyperbolicityMatrix(matrix);
    }
}
