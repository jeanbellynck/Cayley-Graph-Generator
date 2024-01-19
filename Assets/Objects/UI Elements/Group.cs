using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    
    public string name;
    public string generators;
    public string relators;

    // Start is called before the first frame update
    void Start()
    {
        // Set button (legacy) text to group name
        transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = name;
    }
}
