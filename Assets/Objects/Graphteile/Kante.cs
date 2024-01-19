using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kante : MonoBehaviour
{
    public Knoten startPoint;
    public Knoten endPoint;

    public float lineWidth = 0.1f;
    public float arrowWidth = 0.2f;
    public float PercentHead = 0.1f;
    public float vertexRadius = 0.1f;

    // Start is called before the first frame update
    void Start(){}

    public void SetStartpunkt(Knoten startpunkt) {
        startPoint = startpunkt;
        startpunkt.addEdge(this);
    }

    public void SetEndpunkt(Knoten endpunkt) {
        endPoint = endpunkt;
        endpunkt.addEdge(this);
    }
    
    public void SetEndpunkte(Knoten startpunkt, Knoten endpunkt) {
        SetStartpunkt(startpunkt);
        SetEndpunkt(endpunkt);
        endPoint = endpunkt;
    }

    public void SetFarbe(Color farbe1, Color farbe2) {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.startColor = farbe1;
        //lr.endColor = farbe2;
    }

    // Update is called once per frame
    public void Update() {
        if(startPoint != null && endPoint != null) {
            Vector3 linienRichtung = (endPoint.transform.position - startPoint.transform.position).normalized;
            Vector3 startPointWithSpacing = startPoint.transform.position + linienRichtung * vertexRadius;
            Vector3 endPointWithSpacing = endPoint.transform.position - linienRichtung * vertexRadius;

            LineRenderer lr = GetComponent<LineRenderer>();
            
            lr.widthCurve = new AnimationCurve(
             new Keyframe(0, lineWidth)
             , new Keyframe(0.999f - PercentHead, lineWidth)  // neck of arrow
             , new Keyframe(1 - PercentHead, arrowWidth)  // max width of arrow head
             , new Keyframe(1, 0f));  // tip of arrow
            
            lr.SetPositions(new Vector3[] {
               startPointWithSpacing
               , Vector3.Lerp(startPointWithSpacing, endPointWithSpacing, 0.999f - PercentHead)
               , Vector3.Lerp(startPointWithSpacing, endPointWithSpacing, 1 - PercentHead)
               , endPointWithSpacing });
        }
    }

    public char getGenerator() {
        return name.ToCharArray()[0];
    }
}
