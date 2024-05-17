using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RelatorMenu : MonoBehaviour {
    

    [SerializeField] GameObject relatorItemPrefab;
    [SerializeField] GeneratorMenu generatorMenu;
    readonly Dictionary<string, TMP_InputField> relatorInputs = new();
    readonly Dictionary<string, GameObject> relatorGameObjects = new();
    [SerializeField] Button generateButton;
    [SerializeField] string startRelators = "[a,b], a^5";
    public UnityEvent<string, bool> OnRelatorDrawChange = new();

    public IEnumerable<string> Relators { // this is probably bad practice but fun (not used currently)
        get => GetRelators();
        set => SetRelators(value);
    }

    public IEnumerable<string> GetRelators() => GetRelatorStrings().SelectMany(input => RelatorDecoder.DecodeRelatorStrings(input, false)); // is already separated
    public IEnumerable<string> GetRelatorStrings() => from input in relatorInputs.Values select input.text;

    public void Start() {
        if (generatorMenu == null)
            generatorMenu = FindFirstObjectByType<GeneratorMenu>();
        SetRelatorString(startRelators);
    }

    public void AddRelatorString([CanBeNull] string relatorString) =>
        AddRelatorString(relatorString, null);

    void AddRelatorString([CanBeNull] string relatorString, [CanBeNull] string oldIndex) => 
        AddRelators(RelatorDecoder.SeparateRelators(relatorString ?? ""), oldIndex);

    void AddRelators(IEnumerable<string> relators, [CanBeNull] string oldIndex) {
        relators = relators.Where(s => !string.IsNullOrWhiteSpace(s));
        string firstRelator = relators.FirstOrDefault();
        if (oldIndex != null && relatorInputs.ContainsKey(oldIndex)) {
            if (firstRelator == default) {
                Destroy(relatorGameObjects[oldIndex]);
                relatorInputs.Remove(oldIndex);
                relatorGameObjects.Remove(oldIndex);
                return;
            }

            relatorInputs[oldIndex].text = firstRelator;
            relators = relators.Skip(1);
        }
        var index = string.IsNullOrWhiteSpace(oldIndex) ? "" : oldIndex + '.';
        var i = 1;
        List<char> generators = generatorMenu.GetGenerators().ToList();
        var knownRelators = GetRelatorStrings().ToList();

        foreach (var relator in relators) {
            if (knownRelators.Contains(relator)) continue;
            while (relatorInputs.ContainsKey(index + i))
                i++;
            var newIndex = index + i;

            var newRelatorItemGameObject = relatorGameObjects[newIndex] = Instantiate(relatorItemPrefab, transform);
            newRelatorItemGameObject.name = "Relator Item " + newIndex;
            
            var newInputField = relatorInputs[newIndex] = newRelatorItemGameObject.GetComponentInChildren<TMP_InputField>();

            newInputField.text = relator;
            newInputField.onEndEdit.AddListener((s) => { AddRelatorString(s, newIndex); generateButton.Select(); });
            newRelatorItemGameObject.GetComponentInChildren<Toggle>().onValueChanged.AddListener((b) => {
                var decodedRelators = RelatorDecoder.DecodeRelatorStrings(newInputField.text);
                foreach (var decodedRelator in decodedRelators) {
                    OnRelatorDrawChange.Invoke(decodedRelator, b); // this will be listened to by the Mesh Manager, which manages one type of mesh for each relator; we had to separate the relators in a relation like ab=ba=t, because the Mesh Manager tags are the completely decoded ones, i.e. BAba, BAt.
                }
            });

            // Add the generators that are used in the relator to the generator menu
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

    public void FixGeneratorMenu() {
        generatorMenu.AddGenerators(
            from relator in GetRelatorStrings()
            from a in relator
            where char.IsLetter(a)
            select char.ToLower(a)
        );
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

    public string CopyableString(IEnumerable<string> relators = null, IEnumerable<string> generators = null) {
        relators ??= GetRelatorStrings();
        generators ??= generatorMenu.GetGenerators().Select(c => c.ToString());
        var presentation = string.Join(", ", relators);
        presentation += (presentation.Length == 0 ? "" : ", ") +
                        string.Join(",",
                            from gen in generators
                            where !presentation.Contains(gen)
                            select gen + RelatorDecoder.InvertSymbol(gen)
                        );
        return presentation;
    }
}
