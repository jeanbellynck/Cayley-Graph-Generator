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
    [SerializeField] float timeout = 5;
    [SerializeField] bool currentlyFetching;

    void Start() {
        gapClient = new();
        panel.SetActive(false);
    }

    public async void OnClick() {
        if (currentlyFetching) {
            Display("Please wait for the last request to finish");
        }
        currentlyFetching = true;
        //Display("");
        var generators = generatorMenu.Generators.Select(c => c.ToString()).ToArray();
        var relators = relatorMenu.Relators.ToArray();
        var (worked, optimizedGenerators, optimizedRelators, generatorMap) = await gapClient.OptimizePresentation(
            generators,
            relators
        );
        // TODO: Timeouts and not allowing concurrent requests (cancelling)
        currentlyFetching = false;

        if (!worked) {
            Display("Server for simplification couldn't be reached.");
            return;
        }

        lastOptimizedGenerators = optimizedGenerators;
        lastOptimizedRelators = optimizedRelators;

        var text = $@"Optimized presentation for your group using GAP:
<{string.Join(", ", generators)} | {string.Join(", ", relators)}>
  \u2245
<{string.Join(", ", optimizedGenerators)} | {string.Join(", ", optimizedRelators)}>
with the following isomorphism: 
{string.Join(", ", generatorMap.Select(kv => $"{kv.Key} \u2192 {kv.Value}"))}
Right click to use this presentation, middle click to copy"; // \u21A6 is \mapsto. For some reason the more complex arrows cannot be shown in TMP_Text, even though they are in the font (but not in the TMP_FontAsset). We use \u2192 instead, which is \rightarrow.
        Display(text);
    }

    void Display(string text) {
        textField.text = text;

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        StartCoroutine(WaitAndDeactivate());
        return;

        IEnumerator WaitAndDeactivate() {
            yield return new WaitForSeconds(timeout);
            panel.SetActive(false);
            yield return null;
        }
    }

    public void OnPanelClick(int button) {
        switch (button)
        {
            case 0:
                panel.SetActive(false);
                break;
            case 1:
                generatorMenu.Generators = lastOptimizedGenerators.Where(s => !string.IsNullOrWhiteSpace(s)).Select(c => c[0]).ToArray();
                relatorMenu.Relators = lastOptimizedRelators;
                break;
            case 2: 
                GUIUtility.systemCopyBuffer = relatorMenu.CopyableString(lastOptimizedRelators, lastOptimizedGenerators);
                // TODO: fix this for mobile or WebGL
                break;
        }
    }
}
