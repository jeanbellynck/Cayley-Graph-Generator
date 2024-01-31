using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public float age = 0;
    public Vertex startPoint;
    public Vertex endPoint;
    public char generator;

    public float lineWidth = 0.1f;
    public float arrowWidth = 0.2f;
    public float PercentHead = 0.1f;
    public float vertexRadius = 0.1f;

    // Start is called before the first frame update
    void Start(){}


    /**
     * This method is used to get the start Vertex of the edge.
     * The result is dependent on whether op is upper or lower case
     */
    public Vertex getStartPoint(char op) {
        if (char.IsLower(op)) {
            return startPoint;
        }
        else {
            return endPoint;
        }
    }

    
    public void SetEndpoints(Vertex startPoint, Vertex endPoint, char generator) {
        if(this.startPoint != null || this.endPoint != null) {
            throw new Exception("The Endpoints of an Edge are final and should not be changed. Create a new Edge instead");startPoint.removeEdge(this);
        }
        if(char.IsLower(generator)) {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        } else {
            this.startPoint = endPoint;
            this.endPoint = startPoint;
        }
        this.generator = char.ToLower(generator);
        
        name = this.startPoint.name + " --" + this.generator + "-> " + this.endPoint.name;
        startPoint.addEdge(this);
        endPoint.addEdge(this);
        Update();
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
        
        age += Time.deltaTime;
    }

    public char getGenerator() {
        return generator;
    }

    public Vertex getOpposite(Vertex vertex) {
        if(vertex.Equals(startPoint)) {
            return endPoint;
        }
        else if(vertex.Equals(endPoint)) {
            return startPoint;
        }
        else {
            throw new Exception("Vertex is not part of this edge.");
        }
    }

    /**
    public void SetOpposite(Vertex referenceVertex, Vertex oppositeVertex) {
        if(referenceVertex.Equals(startPoint)) {
            SetEnd(oppositeVertex);
        }
        else if(referenceVertex.Equals(endPoint)) {
            SetStart(oppositeVertex);
        }
        else {
            throw new Exception("Vertex is not part of this edge.");
        }
    }**/

    public void Destroy() {
        startPoint.removeEdge(this);
        endPoint.removeEdge(this);
        startPoint = null;
        endPoint = null;
        Destroy(gameObject);
    }

    public bool Equals(Edge other) {
        return startPoint.Equals(other.startPoint) && endPoint.Equals(other.endPoint) && generator == other.generator;
    }
}
