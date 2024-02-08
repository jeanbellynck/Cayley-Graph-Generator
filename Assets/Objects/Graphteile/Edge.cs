using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;


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

    public void SetFarbe(Color farbe1, Color farbe2) {
        //LineRenderer lr = GetComponent<LineRenderer>();
        splineRenderer ??= GetComponent<SplineRenderer>();
        splineRenderer.color = farbe1;
        //lr.endColor = farbe2;
    }
    SplineComputer splineComputer;
    SplineRenderer splineRenderer;

    public float midDisplacementFactor = 0.18f;
    public float midDirectionFactor = 0.3f;
    //Vector3 vectorForOldRandomMidDisplacement = Vector3.zero;
    //Vector3 oldRandomMidDisplacement = Vector3.zero;

    const float scalingC = 1.324717957244f; // scaling(0) = 1/3
    readonly float scalingB = 1/Mathf.Log(1+scalingC); // scaling(1) = 1
    float MidDisplacementScaling(float x) => scalingB * Mathf.Log( scalingC + x);

    void Update() {
        Vector3 vector = endPoint.transform.position - startPoint.transform.position;
        startPoint.CalculateSplineDirection(generator, vector);
        endPoint.CalculateSplineDirection(RelatorDecoder.invertGenerator(generator), vector);
    }

    void LateUpdate() {
        if (startPoint == null || endPoint == null) return;
        age += Time.deltaTime; // what for?

        splineComputer ??= GetComponent<SplineComputer>();
        // https://forum.unity.com/threads/why-does-unity-override-the-null-comparison-for-unity-objects.1294593/ Doesn't matter here

        Vector3 startPosition = startPoint.transform.position;
        Vector3 endPosition = endPoint.transform.position;
        Vector3 vector = endPosition - startPosition;
        //Vector3 vectorNormalized = vector.normalized;
        Vector3 startDirection = startPoint.CalculateSplineDirection(generator, vector);
        Vector3 endDirection = endPoint.CalculateSplineDirection(RelatorDecoder.invertGenerator(generator), vector);

        Vector3 midDisplacementDirectionNonOrthogonal = startDirection - endDirection;
        Vector3 midDisplacementDirection = Vector3.ProjectOnPlane( midDisplacementDirectionNonOrthogonal, vector.normalized);
        float l = midDisplacementDirection.magnitude;
        float lambda = MathF.Sqrt(startDirection.sqrMagnitude + endDirection.sqrMagnitude);
        // overly complicated (and non-working) way to get a random vector orthogonal to vector and not needed bc. the midDisplacement doesn't have to be large.
        //while (l < 0.1 * lambda) {
        //    // replace by random vector (take old random vector if nothing changed)
        //    if ((vectorForOldRandomMidDisplacement - midDisplacementDirectionNonOrthogonal).sqrMagnitude > 0.01) {
        //        oldRandomMidDisplacement = Vector3.ProjectOnPlane(Random.insideUnitSphere, vector.normalized);
        //        vectorForOldRandomMidDisplacement = vector;
        //    }
        //    midDisplacementDirection += oldRandomMidDisplacement;
        //    l = midDisplacementDirection.magnitude;
        //}
        Vector3 midDisplacementVector = l > 0.001f ? midDisplacementFactor * MidDisplacementScaling(l/lambda) / l * midDisplacementDirection : Vector3.zero;
        Vector3 midPosition = startPosition + 0.5f * vector + midDisplacementVector;
        Vector3 midDirection = (vector - endDirection - startDirection) * midDirectionFactor; 
        // this is approximately the vector from the "corner" of the spline after the start to the "corner" before the end


        splineComputer.SetPoints(new [] {
            new SplinePoint(startPosition, startPosition - startDirection),
            new SplinePoint(midPosition, midPosition - midDirection),
            new SplinePoint(endPosition, endPosition - endDirection) 
            // Why do we have to subtract the direction? and why from the position?
        });

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
