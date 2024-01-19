using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGallery : MonoBehaviour
{
    // List of groups to be displayed, parameters are name, generators and relators.
    string[][] groupList = {
        new string[] {"C5 x C12 (Torus)", "a, b", "abAB, aaaaa, bbbbbbbbbbbb"},
        new string[] {"Z^2 with two generators", "a, b", "abAB"},
        new string[] {"Z^2 with three generators", "a, b, c", "abAB, abC"},
        new string[] {"Simple Hyperbolic Group", "a, b", "abab, aaaaa, bbbbb"},
        new string[] {"Free Group with two Generators", "a, b", ""}
    };
    public GameObject groupPrefab;
    public GameObject groupGallery;
    public GameObject cayleyGraph;


    // Start is called before the first frame update
    void Start()
    {
        // For each Group create a new group object and set it as a child of the gallery.
        foreach (string[] group in groupList)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<Group>().name = group[0];
            newGroup.GetComponent<Group>().generators = group[1];
            newGroup.GetComponent<Group>().relators = group[2];
            newGroup.transform.SetParent(groupGallery.transform);
            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().setGroupAndStartVisualisation(group[0], group[1], group[2]));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
