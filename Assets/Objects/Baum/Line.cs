using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public GameObject startPoint;
    public GameObject endPoint;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void LateUpdate()
    {
        print("Kantenkoordinaten werden versucht zu setzen, 1");
        if(startPoint != null && endPoint != null) {
            print("Kantenkoordinaten werden gesetzt, 1");
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.SetPosition(0, startPoint.transform.position);
            lr.SetPosition(1, endPoint.transform.position);
        }
    }
}
