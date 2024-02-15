using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] char[] generators = new char[0];
    [SerializeField] string[] relators = new string[0];


    public float Activity => Math.Clamp(physik.alpha, 0f, 1f);

    // Start is called before the first frame update
    void Start() {
        cayleyGraphMaker.setPhysics(physik);
    }


    // Update is called once per frame
    void Update() {
    }

    void setGenerators(IEnumerable<char> newGenerators) {
        if (newGenerators.SequenceEqual(generators)) return;
        generators = newGenerators.ToArray();
        cayleyGraphMaker.setGenerators(generators);
        hyperbolicityMatrix.SetMatrixSize(generators.Length);
    }

    void setRelators(IEnumerable<string> relators) => this.relators = relators.ToArray();

    public void setVertexNumber(string vertexNumber) {
        cayleyGraphMaker.setVertexNumber(int.Parse(vertexNumber));
    }

    public GeneratorMenu generatorMenu;
    public RelatorMenu relatorMenu;
    public InputField vertexNumberInputField;
    public InputField hyperbolicityInputField;
    public HyperbolicityMatrix hyperbolicityMatrix;
    public TMP_Dropdown dimensionInputDD;


    public void generateButton() {
        StopVisualization();
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
        setGenerators(generatorMenu.GetGenerators());
        setRelators(relatorMenu.GetRelators());
        graphManager.Initialize(generators);
        int projectionDimension = dimensionInputDD.value + 2;
        physik.startUp(graphManager, projectionDimension);
        int actualDimension = projectionDimension + 0;
        cayleyGraphMaker.StartVisualization(graphManager, meshManager, generators, relators, actualDimension);
    }


    public void SelectGroupOption(string name, string generatorString, string relatorString) {
        Debug.Log("Set Group: " + name);
        // Set the generators and relators for the input fields and for the program
        generatorMenu.SetGenerators(generatorString.Where(char.IsLetter));
        relatorMenu.SetRelatorString(relatorString);
        generateButton();
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
