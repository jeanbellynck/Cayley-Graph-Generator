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

    [SerializeField] GraphVisualizer graphVisualizer;
    [SerializeField] CayleyGraphMaker cayleyGraphMaker;
    [SerializeField] CayleySubGraphMaker cayleySubGraphMaker;


    // These values are probably better moved to CayleyGraphMaker entirely
    [SerializeField] char[] generators = new char[0];
    [SerializeField] string[] relators = new string[0];

    [SerializeField] GeneratorMenu generatorMenu;
    [SerializeField] RelatorMenu relatorMenu;
    [SerializeField] InputField vertexNumberInputField;
    [SerializeField] InputField hyperbolicityInputField;
    [SerializeField] HyperbolicityMatrix hyperbolicityMatrix;
    [SerializeField] TMP_Dropdown dimensionInputDD;
    [SerializeField] TMP_Dropdown groupModeDropdown;

    public float Activity => physik.Activity;

    void SetGenerators(IEnumerable<char> newGenerators) {
        if (newGenerators.SequenceEqual(generators)) return;
        generators = newGenerators.ToArray();
        cayleyGraphMaker.SetGenerators(generators);
        hyperbolicityMatrix.SetMatrixSize(generators.Length);
    }

    // referenced from UI
    public void SetVertexNumber(string vertexNumber) {
        if (int.TryParse(vertexNumber, out int num))
            cayleyGraphMaker.SetVertexNumber(num);
        // TODO: also continue subgroup visualization
    }

    // referenced from UI
    public void ToggleActiveState() {
        cayleyGraphMaker.ToggleActiveState();
        // TODO: also toggle subgroup visualization
    }


    // referenced from UI
    public void OnGenerateButton() {
        //StopVisualization();
        StartVisualization();
    }

    // referenced from UI
    public void StopVisualization() {
        physik.BeginShutDown();
        cayleyGraphMaker.StopVisualization();
        // TODO: Subgroup
    }

    public void StartVisualization() {
        SetVertexNumber(vertexNumberInputField.text);
        relatorMenu.FixGeneratorMenu();
        SetGenerators(generatorMenu.GetGenerators());
        this.relators = relatorMenu.GetRelators().ToHashSet().ToArray();
        graphVisualizer.Initialize(generators, activityProvider: this);
        int projectionDimension = dimensionInputDD.value + 2;
        physik.Initialize(graphVisualizer.graphManager, projectionDimension, generators.Length); // calls physik.Abort()
        int actualDimension = projectionDimension + 0;
        graphVisualizer.SetDimension(actualDimension);
        cayleyGraphMaker.Initialize(generators, relators, physik, graphVisualizer, (GroupMode) groupModeDropdown.value); // calls AbortVisualization()
        cayleyGraphMaker.StartVisualization(); // calls physik.Run() 
    }


    public void SelectGroupOption(string name, string generatorString, string relatorString,
        GroupMode groupMode) {
        generatorMenu.SetGenerators(generatorString.Where(char.IsLetter));
        relatorMenu.SetRelatorString(relatorString);
        groupModeDropdown.value = (int) groupMode;
        OnGenerateButton();
    }

    // referenced from UI
    public void SetHyperbolicity(string hyperbolicityString) {
        print("scaling changed to:" + hyperbolicityString);
        if (float.TryParse(hyperbolicityString.FixDecimalPoint(), out float hyperbolicity) && hyperbolicity != 0) {
            cayleyGraphMaker.SetHyperbolicity(hyperbolicity);
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
        cayleySubGraphMaker.RegenerateSubgroup(generators, cayleyGraphMaker.neutralElement);
    }
}
