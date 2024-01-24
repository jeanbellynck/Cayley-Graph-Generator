using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Group : MonoBehaviour
{
    
    public string name;
    public string description;
    public string generators;
    public string relators;

    // Start is called before the first frame update
    void Start()
    {
        // Set button (TMP) text to group name
        transform.GetChild(0).GetComponent<TMP_Text>().text = name; 
        transform.GetChild(1).GetChild(0).GetComponent<Text>().text = description;
        transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "〈" + generators + " : " + relators + "〉";
        // Update Layout using LayoutRebuilder (copied from https://stackoverflow.com/questions/60201481/unity-3d-vertical-layout-group-not-placing-elements-where-they-should-be)
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        
    }
}
