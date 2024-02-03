using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HyperbolicityMatrix : MonoBehaviour {
    public GameObject textFieldPrefab;
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
        for (int i = 0; i < newMatrixSize; i++) {
            for (int j = 0; j < newMatrixSize; j++) {
                // Instantiate new text fields and put them as children of this object
                GameObject textField = Instantiate(textFieldPrefab, transform);
                if(i < oldMatrixSize && j < oldMatrixSize) {
                    textField.GetComponent<TMP_InputField>().text = matrix[i, j];
                } else {
                    textField.GetComponent<TMP_InputField>().text = "1";
                }
                textFields[i, j] = textField;
            }
        }
        // Set Constraint of GridLayoutGroup to matrixSize
        gameObject.GetComponent<GridLayoutGroup>().constraintCount = newMatrixSize;
    }


    /**
     * Reads the matrix out of the text fields.
     */
    public void UpdateMatrix() {
        int matrixSize = textFields.GetLength(0);
        float[,] matrix = new float[matrixSize, matrixSize];

        for (int i = 0; i < matrixSize; i++) {
            for (int j = 0; j < matrixSize; j++) {
                if(float.TryParse(textFields[i, j].GetComponent<TMP_InputField>().text, out float result) && result != 0) {
                    matrix[i, j] = float.Parse(textFields[i, j].GetComponent<TMP_InputField>().text);
                } else {
                    matrix[i, j] = 1;
                }
            }
        }
    }
}
