using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    // referenced from UI
    public void openPage(string url) {
        Application.OpenURL(url);
    } 
}
