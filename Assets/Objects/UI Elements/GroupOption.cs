using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupOption : MonoBehaviour
{
    public Group group;

    public GameObject groupParameterPrefab;
    public Tooltip tooltip;


    // Start is called before the first frame update
    void Start()
    {
        // Set button (TMP) text to group name
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = group.name; 
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = group.description;
        setPresentation();
        // Set tooltip information
        tooltip.text = group.tooltipInfo;
        tooltip.url = group.tooltipURL;
        
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
        setPresentation();
        // Update Layout using LayoutRebuilder (copied from https://stackoverflow.com/questions/60201481/unity-3d-vertical-layout-group-not-placing-elements-where-they-should-be)
        //LayoutRebuilder.MarkLayoutForRebuild(transform.parent.GetComponent<RectTransform>());
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetChild(1).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        //LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    public void setPresentation() {
        string presentation = "〈" + string.Join(", ", group.generators) + " : " + string.Join(", ", group.relators) + "〉";
        // If there is an exponent of the presentation surround the number with <sup></sup>
        int i = 0;
        while (i < presentation.Length)
        {
            if (presentation[i] == '^')
            {
                int power = findPowerValue(presentation, i);
                presentation = presentation.Substring(0, i) + "<sup>" + power.ToString() + "</sup>" + presentation.Substring(i + power.ToString().Length + 1);
                i += power.ToString().Length + 11;
            }
            i++;
        }

        transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = presentation;
    }

    private static int findPowerValue(string symbol, int powerIndex)
    {
        int i = 1;
        if(powerIndex+i < symbol.Length && symbol[powerIndex+i] == '-') {
            i++;
        }
        while (powerIndex+i < symbol.Length && char.IsDigit(symbol[powerIndex+i]))
        {
            i++;
        }
        return int.Parse(symbol.Substring(powerIndex + 1, --i));
    }
}
