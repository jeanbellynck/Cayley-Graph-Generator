using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupOption : MonoBehaviour
{
    public Group group;

    public GameObject groupParameterPrefab;


    // Start is called before the first frame update
    void Start()
    {
        // Set button (TMP) text to group name
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = group.name; 
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = group.description;
        transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = "〈" + string.Join(", ", group.generators) + " : " + string.Join(", ", group.relators) + "〉";
        
        // For each parameter create a new parameter object and set it as a child of the groupOption object.
        for(int i = 0; i < group.parameters.Length; i++)
        {
            string[] parameter = group.parameters[i];
            GameObject newParameter = Instantiate(groupParameterPrefab, transform);
            newParameter.transform.GetChild(0).GetComponent<TMP_Text>().text = parameter[0] + "=";
            newParameter.transform.GetChild(1).GetComponent<TMP_InputField>().text = parameter[1];
            newParameter.transform.GetChild(2).GetComponent<Text>().text = parameter[2];
            
            // For each parameter set updateParameter() as the on Value Changed method
            int iTemp = i;
            newParameter.transform.GetChild(1).GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate {updateParameter(iTemp, newParameter.transform.GetChild(1).GetComponent<TMP_InputField>().text);});
            newParameter.transform.SetParent(transform.GetChild(2));
        }
        // Update Layout using LayoutRebuilder (copied from https://stackoverflow.com/questions/60201481/unity-3d-vertical-layout-group-not-placing-elements-where-they-should-be)
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    /**
    * This method is called when a parameter is changed.
    * It updates the parameter in the group and recalculates the presentation.
    * The presentation is then updated in the UI.
    **/
    public void updateParameter(int parameterIndex, string parameterValue) {
        group.parameters[parameterIndex][1] = parameterValue;  
        group.updatePresentation();
        transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = "〈" + string.Join(", ", group.generators) + " : " + string.Join(", ", group.relators) + "〉";
        // Update Layout using LayoutRebuilder (copied from https://stackoverflow.com/questions/60201481/unity-3d-vertical-layout-group-not-placing-elements-where-they-should-be)
        //LayoutRebuilder.MarkLayoutForRebuild(transform.parent.GetComponent<RectTransform>());
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetChild(1).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }
}