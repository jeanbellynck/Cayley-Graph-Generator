using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Globalization;
using System.Linq;

public class HyperbolicityMatrix : MonoBehaviour {
    public GameObject textFieldPrefab;
    public GameObject hyperbolicityTextField;
    public GameObject cayleyGraph;

    [SerializeField]
    private GameObject[,] textFields = new GameObject[0, 0];

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void SetMatrixSize(int newMatrixSize) {
        int matrixSize = textFields.GetLength(0);
        string[,] matrix = new string[matrixSize, matrixSize];
        // Destroy all text fields
        int oldMatrixSize = textFields.GetLength(0);
        for (int i = 0; i < oldMatrixSize; i++) {
            for (int j = 0; j < oldMatrixSize; j++) {
                matrix[i, j] = textFields[i, j].GetComponent<TMP_InputField>().text;
                Destroy(textFields[i, j]);
            }
        }
        textFields = new GameObject[newMatrixSize, newMatrixSize];
        for (int x = 0; x < newMatrixSize; x++) {
            for (int y = 0; y < newMatrixSize; y++) {
                // Instantiate new text fields and put them as children of this object
                GameObject textField = Instantiate(textFieldPrefab, transform);
                // Textfield should call ChangeValue when the text changes
                if(x < oldMatrixSize && y < oldMatrixSize) {
                    textField.GetComponent<TMP_InputField>().text = matrix[x, y];
                } else {
                    textField.GetComponent<TMP_InputField>().text = "1";
                }
                textFields[x, y] = textField;
                textField.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate { ValueChanged(textField.GetComponent<TMP_InputField>().text); });
            }
        }
        // Set Constraint of GridLayoutGroup to matrixSize
        gameObject.GetComponent<GridLayoutGroup>().constraintCount = newMatrixSize;
        UpdateMatrix();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    public void ValueChanged(string value) {
        char separator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.First();
        
        if(float.TryParse(value.Replace('.', separator).Replace(',', separator), out float result) && result != 0) {
            UpdateMatrix();
        }
    }

    /**
     * Reads the matrix out of the text fields.
     */
    public void UpdateMatrix() {
        // Temporary solution. Matrix is currently disabled
        cayleyGraph.GetComponent<CayleyGraph>().SetHyperbolicity(hyperbolicityTextField.GetComponent<InputField>().text);
        return;
        /**
        // Read Hyperbolicity out of hyperbolicityTextField
        float hyperbolicity;

        char separator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.First();
        if(float.TryParse(hyperbolicityTextField.GetComponent<InputField>().text.Replace('.', separator).Replace(',', separator), out hyperbolicity) && hyperbolicity > 0) {
            // continue
        } else {
            //exit
            return;
        }

        int matrixSize = textFields.GetLength(0);
        float[,] matrix = new float[matrixSize, matrixSize];

        for (int i = 0; i < matrixSize; i++) {
            for (int j = 0; j < matrixSize; j++) {
                // Matrix is currently deactivated
                matrix[i, j] = hyperbolicity;
                **/
                /**
                if(float.TryParse(textFields[i, j].GetComponent<TMP_InputField>().text, out float result) && result != 0) {
                    matrix[i, j] = float.Parse(textFields[i, j].GetComponent<TMP_InputField>().text);
                } else {
                    matrix[i, j] = 1;
                }
                **/
                /**
            }
        }**/
        //cayleyGraph.GetComponent<CayleyGraph>().SetHyperbolicityMatrix(matrix);
    }
}
