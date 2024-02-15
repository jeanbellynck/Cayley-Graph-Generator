using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RelatorInputFieldSettingsSetter : MonoBehaviour
{
    [SerializeField] RelatorMenu relatorMenu;
    // Start is called before the first frame update
    void Start() {
        var inputField = GetComponent<TMP_InputField>();
        if (relatorMenu == null) {
            relatorMenu = FindFirstObjectByType<RelatorMenu>();
            if (relatorMenu == null) return;
        }
        inputField.onSubmit.AddListener(relatorMenu.AddRelatorString);
        inputField.restoreOriginalTextOnEscape = false;
    }

}
