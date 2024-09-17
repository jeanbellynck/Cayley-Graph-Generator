using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ElementFinder : MonoBehaviour, ICenterProvider
{
    [SerializeField] GroupVertex vertex;
    [SerializeField] CayleyGraphMaker cayleyMaker;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] List<string> lastWords = new();

    [SerializeField] int nextLastWordIndex = 0;
    [SerializeField] CenterPointer centerOfMassPointer;
    [field: SerializeField] public CenterPointer CenterPointer { get; private set; }

    public event Action<CenterPointer> OnCenterChanged;

    public GroupVertex Vertex {
        get => vertex;
        private set {
            if (vertex != null) {
                vertex.HighlightPathsFromIdentity(removeHighlight: true);
                vertex.UnHighlight(HighlightType.Selected);
            }
            vertex = value;

            if (vertex != null) {
                vertex.HighlightPathsFromIdentity(removeHighlight: false);
                vertex.Highlight(HighlightType.Selected);
            }
        }
    }


    // Event handler from GraphVisualizer
    public void SetVertex(GroupVertex vertex) {
        if (vertex == null) return;
        string newVertexWord = vertex.PathsFromNeutralElement.FirstOrDefault();
        inputField.text = newVertexWord;
        UpdateLastWords(newVertexWord);
        Vertex = vertex;
    }

    void Update() {

        if (inputField.isFocused) {
            if (Input.GetKeyDown(KeyCode.UpArrow) && lastWords.Count > nextLastWordIndex) {
                string word = lastWords[nextLastWordIndex];
                inputField.text = word;
                SetFromWord(word);
                nextLastWordIndex++;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && nextLastWordIndex > 0) {
                nextLastWordIndex--;
                string word = lastWords[nextLastWordIndex];
                inputField.text = word;
                SetFromWord(word);
            }
        }
    }

    void Start() => centerOfMassPointer = FindObjectsByType<Physik>(FindObjectsSortMode.InstanceID).FirstOrDefault()?.centerPointer;
    // todo? This is not really elegant

    // InputField handler
    public void OnEndEdit(string word) {
        SetFromWord(word);
        UpdateLastWords(word);
    }

    void SetFromWord(string word) {
        bool bad = false;
        GroupVertex newVertex = null;
        if (cayleyMaker == null || cayleyMaker.NeutralElement == null) bad = true;
        if (!bad) newVertex = cayleyMaker.NeutralElement.FollowGeneratorPath(word);
        if (newVertex == null) bad = true;
        if (bad) {
            inputField.textComponent.color = Color.red;
            Debug.Log($"Vertex {word} not found");
            return;
        }

        Vertex = newVertex;
    }

    void UpdateLastWords(string word) {
        lastWords.Remove(word);
        lastWords.Insert(0, word);
        nextLastWordIndex = 0;
    }

    // InputField handler
    public void OnChanged(string _) {
        inputField.textComponent.color = Color.black;
    }

    // Button handler
    public void Center() {
        if (Vertex == null) return;
        CenterPointer = Vertex.centerPointer;
        OnCenterChanged?.Invoke(CenterPointer);
        // Vertex.Center();
    }

    // Button handler
    public void SelectNeutral() {
        inputField.text = "";
        OnEndEdit("");
    }

    // Button handler
    public void CenterOnCenterOfMass() {
        CenterPointer = centerOfMassPointer;
        OnCenterChanged?.Invoke(CenterPointer);
    }
}

internal interface ICenterProvider {
    CenterPointer CenterPointer { get; }
    event Action<CenterPointer> OnCenterChanged;
}