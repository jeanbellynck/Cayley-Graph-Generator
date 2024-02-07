using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
    


public class Edge : MonoBehaviour {
    public static GameObject edgePrefab;
    public float lineWidth = 0.1f;
    public float arrowWidth = 0.2f;
    public float PercentHead = 0.1f;
    public float vertexRadius = 0.1f;


    private char label;


    [SerializeField]
    private float length;
    public float age = 0;
    private Vertex startPoint;
    private Vertex endPoint;

    public char Label { get => label; set => label = value; }
    public Vertex StartPoint { get => startPoint; set => startPoint = value; }
    public Vertex EndPoint { get => endPoint; set => endPoint = value; }
    public float Length { get => length; set => length = value; }

    public void Initialize(Vertex startPoint, Vertex endPoint, char label) {
        this.StartPoint = startPoint;
        this.EndPoint = endPoint;
        this.Label = label;
        name = StartPoint.name + " --" + Label + "-> " + EndPoint.name;
        startPoint.AddEdge(this);
        endPoint.AddEdge(this);
        Update();
    }

    // Update is called once per frame
    public void Update() {
        if (StartPoint != null && EndPoint != null) {
            Vector3 linienRichtung = (EndPoint.transform.position - StartPoint.transform.position).normalized;
            Vector3 startPointWithSpacing = StartPoint.transform.position + linienRichtung * vertexRadius;
            Vector3 endPointWithSpacing = EndPoint.transform.position - linienRichtung * vertexRadius;

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

    public void Destroy() {
        StartPoint.RemoveEdge(this);
        EndPoint.RemoveEdge(this);
        StartPoint = null;
        EndPoint = null;
        Destroy(gameObject);
    }


    public virtual Vertex getOpposite(GroupVertex vertex) {
        if (vertex.Equals(StartPoint)) {
            return EndPoint;
        }
        else if (vertex.Equals(EndPoint)) {
            return StartPoint;
        }
        else {
            throw new Exception("Vertex is not part of this edge.");
        }
    }
}