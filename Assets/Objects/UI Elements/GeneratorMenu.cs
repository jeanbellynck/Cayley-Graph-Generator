using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GeneratorMenu : MonoBehaviour
{
    [SerializeField] GameObject generatorInputPrefab;
    readonly List<TMP_InputField> generatorInputs = new();
    readonly List<GameObject> generatorGameObjects = new();

    public event Action OnGeneratorsChanged;

    public IEnumerable<char> Generators {
        get => GetGenerators();
        set => SetGenerators(value);
    }

    public IEnumerable<char> GetGenerators() {
        return from input in generatorInputs
            let a = input.text.FirstOrDefault()
            let c = a != default ? a : 
                (input.placeholder.GetComponent<TMP_Text>().text.Length > 0 ? input.placeholder.GetComponent<TMP_Text>().text[0] : 'a')
                //shouldn't be necessary as it should delete itself if the input is empty => I don't care about performance if it only happens in this case
            select char.ToLower(c);
    }

    public void SetGenerators(IEnumerable<char> generators) {
        foreach (var generatorGameObject in generatorGameObjects) {
            Destroy(generatorGameObject);
        } 
        generatorInputs.Clear();
        generatorGameObjects.Clear();

        foreach (var generator in generators) 
            AddGeneratorInput(generator);
    }

    public void AddGenerators(IEnumerable<char> generators) {
        var presentGenerators = GetGenerators().ToList();
        foreach (var generator in generators) {
            if (presentGenerators.Contains(generator)) continue;
            presentGenerators.Add(generator);
            AddGeneratorInput(generator);
        }
    }

    public void AddGeneratorInput() => AddGeneratorInput(default);

    public void AddGeneratorInput(char preferredGeneratorName) {
        var selectedGenerators = GetGenerators().Select(a => a - 'a').ToArray();
        int preferredGeneratorNumber = preferredGeneratorName - 'a';
        if (preferredGeneratorNumber is < 0 or >= 26) 
            preferredGeneratorNumber = selectedGenerators.DefaultIfEmpty(0).Max();
        var expectedGeneratorNumber = preferredGeneratorNumber;
        for (var i = preferredGeneratorNumber; i < 26 + preferredGeneratorNumber; i++) {
            if (selectedGenerators.Contains(i%26)) continue;
            expectedGeneratorNumber = i%26;
            break;
        }
        string expectedName = ((char)(expectedGeneratorNumber + 'a')).ToString();
        var newGeneratorInputGameObject = Instantiate(generatorInputPrefab, transform);
        generatorGameObjects.Add(newGeneratorInputGameObject);

        var newGeneratorInputField = newGeneratorInputGameObject.GetComponentInChildren<TMP_InputField>();
        generatorInputs.Add(newGeneratorInputField);
        newGeneratorInputField.text = expectedName;
        newGeneratorInputField.placeholder.GetComponent<TMP_Text>().text = expectedName;
        //newGeneratorInput.GetComponentInChildren<Button>().onClick.AddListener(() => Destroy(newGeneratorInput));
        newGeneratorInputField.onEndEdit.AddListener((s) => {
            if (string.IsNullOrWhiteSpace(s)) s = "";
            var c = char.ToLower(s.TrimStart().FirstOrDefault());
            var generators = GetGenerators();
            if (c is > 'z' or < 'a' || generators.ContainsTwice(c)) {
                Destroy(newGeneratorInputGameObject);
                generatorInputs.Remove(newGeneratorInputField);
                generatorGameObjects.Remove(newGeneratorInputGameObject);
                return;
            }
            newGeneratorInputField.text = c.ToString();
            OnGeneratorsChanged?.Invoke();
        });
        OnGeneratorsChanged?.Invoke();
    }

}
