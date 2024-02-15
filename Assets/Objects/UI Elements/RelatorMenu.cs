using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class RelatorMenu : MonoBehaviour {
    

    [SerializeField] GameObject relatorItemPrefab;
    [SerializeField] GeneratorMenu generatorMenu;
    readonly Dictionary<string, TMP_InputField> relatorInputs = new();
    readonly Dictionary<string, GameObject> relatorGameObjects = new();

    public IEnumerable<string> GetRelators() {
        return from input in relatorInputs.Values
            select input.text;
    }

    public void Awake() {
        if (generatorMenu == null)
            generatorMenu = FindObjectOfType<GeneratorMenu>();
        SetRelatorString("[a,b], a^5");
    }

    public void AddRelatorString([CanBeNull] string relatorString) {
        AddRelatorString(relatorString, null);
    }

    void AddRelatorString([CanBeNull] string relatorString, [CanBeNull] string oldIndex) {
        var newRelators = RelatorDecoder.decodeRelators(relatorString ?? "");
        AddRelators(newRelators, oldIndex);
    }

    void AddRelators(IEnumerable<string> relators, [CanBeNull] string oldIndex) {
        var knownRelators = GetRelators().ToList();
        relators = relators.Where(s => !string.IsNullOrWhiteSpace(s) && !knownRelators.Contains(s));
        string firstRelator = relators.FirstOrDefault();
        if (oldIndex != null && relatorInputs.ContainsKey(oldIndex)) {
            if (firstRelator == default) {
                Destroy(relatorGameObjects[oldIndex]);
                relatorInputs.Remove(oldIndex);
                return;
            }

            relatorInputs[oldIndex].text = firstRelator;
            relators = relators.Skip(1);
        }
        var index = string.IsNullOrWhiteSpace(oldIndex) ? "" : oldIndex + '.';
        var i = 1;
        List<char> generators = generatorMenu.GetGenerators().ToList();
        foreach (var relator in relators) {
            while (relatorInputs.ContainsKey(index + i))
                i++;
            var newIndex = index + i;

            var newRelatorItemGameObject = relatorGameObjects[newIndex] = Instantiate(relatorItemPrefab, transform);
            newRelatorItemGameObject.name = "Relator Item " + newIndex;
            
            var newInputField = relatorInputs[newIndex] = newRelatorItemGameObject.GetComponentInChildren<TMP_InputField>();

            newInputField.text = relator;
            newInputField.onEndEdit.AddListener((s) => { AddRelatorString(s, newIndex); });

            foreach (var c in
                     from a in relator
                     where char.IsLetter(a) // should not happen, but currently, if you enter e.g. "5", RelatorDecoder will return "5" (which is nonsensical)
                     let b = char.ToLower(a)
                     where !generators.Contains(b)
                     select b) {
                generatorMenu.AddGeneratorInput(c);
                generators.Add(c);
            }
        }
    }

    public void SetRelatorString([CanBeNull] string relatorString) {
        ResetRelators();
        AddRelatorString(relatorString, null);
    }
    public void SetRelators([CanBeNull] IEnumerable<string> relators = null) {
        ResetRelators();
        if (relators != null)
            AddRelators(relators, null);
    }

    void ResetRelators()
    {
        foreach (var relator in relatorGameObjects.Values)
            Destroy(relator);

        relatorInputs.Clear();
        relatorGameObjects.Clear();
    }
}
