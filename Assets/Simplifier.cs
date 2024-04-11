using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Simplifier : MonoBehaviour {
    [SerializeField] RelatorMenu relatorMenu;
    [SerializeField] GeneratorMenu generatorMenu;
    [SerializeField] TMP_Text textField;
    [SerializeField] GameObject panel;
    GAPClient gapClient;
    [SerializeField] string[] lastOptimizedGenerators;
    [SerializeField] string[] lastOptimizedRelators;

    void Start() => gapClient = new();

    public async void OnClick() {
        var (worked, optimizedGenerators, optimizedRelators, generatorMap) = await gapClient.OptimizePresentation(
            generatorMenu.Generators.Select(c => c.ToString()).ToArray(),
            relatorMenu.Relators.ToArray()
        );
        // TODO: Timeouts and not allowing concurrent requests (cancelling)

        if (!worked) return;

        lastOptimizedGenerators = optimizedGenerators;
        lastOptimizedRelators = optimizedRelators;

        textField.text = $@"This optimized presentation describes the same group: 
<{string.Join(", ", optimizedGenerators)} | {string.Join(", ", optimizedRelators)}>
with the following mapping: 
{string.Join(", ", generatorMap.Select(kv => $"{kv.Key} -> {kv.Value}"))}";

        panel.SetActive(true);

        StartCoroutine(WaitAndDeactivate());
        return;

        IEnumerator WaitAndDeactivate() {
            yield return new WaitForSeconds(5);
            panel.SetActive(false);
            yield return null;
        }
    }
}
