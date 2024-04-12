using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * This class is both the main class for the graph and the interface for the UI.
 */
public class CayleyGraphMain : MonoBehaviour, IActivityProvider {

    [SerializeField] Physik physik;// = new Physik(10, 100);

    public GraphVisualizer graphVisualizer;
    public CayleyGraphMaker cayleyGraphMaker;
    public CayleySubGraphMaker cayleySubGraphMaker;


    // These values are probably better moved to CayleyGraphMaker entirely
    [SerializeField] char[] generators = new char[0];
    [SerializeField] string[] relators = new string[0];


    public float Activity => physik.Activity;

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

    void setRelators(IEnumerable<string> relators) => this.relators = relators.ToHashSet().ToArray();

    public void setVertexNumber(string vertexNumber) {
        if (int.TryParse(vertexNumber, out int num))
            cayleyGraphMaker.setVertexNumber(num);
    }

    [SerializeField] GeneratorMenu generatorMenu;
    [SerializeField] RelatorMenu relatorMenu;
    [SerializeField] InputField vertexNumberInputField;
    [SerializeField] InputField hyperbolicityInputField;
    [SerializeField] HyperbolicityMatrix hyperbolicityMatrix;
    [SerializeField] TMP_Dropdown dimensionInputDD;


    public void generateButton() {
        StopVisualization();
        StartVisualization();
    }

    public void StopVisualization() {
        physik.StopAllCoroutines();
        cayleyGraphMaker.StopVisualization();
    }

    public void StartVisualization() {
        setVertexNumber(vertexNumberInputField.text);
        relatorMenu.FixGeneratorMenu();
        setGenerators(generatorMenu.GetGenerators());
        setRelators(relatorMenu.GetRelators());
        graphVisualizer.Initialize(generators, this);
        int projectionDimension = dimensionInputDD.value + 2;
        physik.startUp(graphVisualizer.graphManager, projectionDimension, generators.Length);
        int actualDimension = projectionDimension + 0;
        graphVisualizer.SetDimension(actualDimension);
        cayleyGraphMaker.StartVisualization(generators, relators);
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
        if (float.TryParse(hyperbolicityString.FixDecimalPoint(), out float hyperbolicity) && hyperbolicity != 0) {
            cayleyGraphMaker.setHyperbolicity(hyperbolicity);
        }
    }


    public void SetHyperbolicityMatrix(float[,] matrix) {
        cayleyGraphMaker.SetHyperbolicityMatrix(matrix);
    }


    /**
     * This method draws a subgroup inside the graph. 
     * It also sets the strength of the subgroup edges and the ambient edges.
     */
    public void DrawSubgroup(IEnumerable<string> generators, float ambientEdgeStrength, float subgroupEdgeStrength) {
        graphVisualizer.AmbientEdgeStrength = ambientEdgeStrength;
        graphVisualizer.SubgroupEdgeStrength = subgroupEdgeStrength;
        cayleySubGraphMaker.RegenerateSubgroup(generators);
    }
}
