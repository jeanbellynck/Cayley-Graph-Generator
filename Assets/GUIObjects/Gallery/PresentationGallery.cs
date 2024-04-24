using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PresentationGallery : MonoBehaviour
{
    [SerializeField] GameObject examplePrefab;
    [SerializeField] GameObject labelPrefab;
    [SerializeField] Transform parent;
    [SerializeField] CayleyGraphMain cayleyGraph;
    [SerializeField] string labelText;
    [SerializeField] PresentationExample[] presentationExamples;

    protected PresentationGallery (string labelText, PresentationExample[] presentationExamples)
    {
        this.labelText = labelText;
        this.presentationExamples = presentationExamples;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Add a finite group label
        GameObject finiteGroupLabel = Instantiate(labelPrefab, parent);
        finiteGroupLabel.GetComponent<TMP_Text>().text = labelText;


        // For each Group create a new group object and set it as a child of the gallery.
        foreach (PresentationExample presentationExample in presentationExamples)
        {
            GameObject newGroup = Instantiate(examplePrefab, parent);
            newGroup.GetComponent<GroupOption>().group = presentationExample;

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.SelectGroupOption(presentationExample.name, string.Join(',', presentationExample.generators), string.Join(',', presentationExample.relators), presentationExample.groupMode));
        }
    }

}
