using Dreamteck.Splines;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Edge : MonoBehaviour {
    [SerializeField] protected float lineWidth = 0.1f;
    [SerializeField] protected float arrowWidth = 0.2f;
    [SerializeField] protected float PercentHead = 0.1f;
    [SerializeField] protected float vertexRadius = 0.1f;
    [SerializeField] bool useSplines = true;

    [SerializeField] float length = 1f;

    float creationTime; // = Time.time;
    public float Age => Time.time - creationTime;

    [SerializeField] Vertex startPoint;
    [SerializeField] Vertex endPoint;

    [SerializeField] char _label;
    public char Label { get => _label; protected set => _label = value; }
    public Vertex StartPoint { get => startPoint; protected set => startPoint = value; }
    public Vertex EndPoint { get => endPoint; protected set => endPoint = value; }
    public float Length { get => length; protected set => length = value; }
    public Vector3 Direction => EndPoint.transform.position - StartPoint.transform.position;
    
    [SerializeField] private float strength = 1f;
    public float Strength { get => strength; set => strength = value; }
    [SerializeField] private bool physicsEnabled = true; // With this method the pyhsics can be turned off independantly of the value strength
    public bool PhysicsEnabled { get => physicsEnabled; set => physicsEnabled = value; }
    
    public enum SplinificationType {
        Never,
        Always,
        WhenSimulationSlowsDown,
        WhenSimulationHasStopped
    }

    SplinificationType splinificationType => graphVisualizer.splinificationType;
    SplinificationType? lastSplinificationType = null;

    //protected IDictionary<char, Color> LabelColors => graphManager.labelColors;

    SplineComputer splineComputer;
    SplineRenderer splineRenderer;
    MeshRenderer meshRenderer;
    LineRenderer lineRenderer;

    public bool finished;
    public GraphVisualizer graphVisualizer;

    [SerializeField]
    protected float midDisplacementFactor = 0.18f;
    [SerializeField]
    protected float midDirectionFactor = 0.3f;
    //Vector3 vectorForOldRandomMidDisplacement = Vector3.zero;
    //Vector3 oldRandomMidDisplacement = Vector3.zero;
    public float Activity => graphVisualizer.Activity;

    [SerializeField] Color startColor, endColor;

    void Start() {
        creationTime = Time.time;
    }

    public void Initialize(Vertex startPoint, Vertex endPoint, char label, GraphVisualizer graphVisualizer) {
        this.StartPoint = startPoint;
        this.EndPoint = endPoint;
        this.Label = label;
        this.graphVisualizer = graphVisualizer;
        name = StartPoint.name + " --" + Label + "-> " + EndPoint.name;
        startPoint.AddEdge(this);
        endPoint.AddEdge(this);

        splineComputer = GetComponent<SplineComputer>();
        splineRenderer = GetComponent<SplineRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        SetColors(new Color(1, 0, 0));

        useSplines = splinificationType == SplinificationType.Always;

        LateUpdate();
    }

    public void Destroy() {
        StartPoint.RemoveEdge(this);
        EndPoint.RemoveEdge(this);
        StartPoint = null;
        EndPoint = null;
        Destroy(gameObject);
    }

    public void SetColors(Color startColor, Color? endColor = null) {
        this.startColor = startColor;
        this.endColor = endColor ?? startColor.Desaturate(0.2f);
    }

    const float scalingC = 1.324717957244f; // scaling(0) = 1/3
    readonly float scalingB = 1 / Mathf.Log(1 + scalingC); // scaling(1) = 1
    const float scalingD = 1.8f;
    readonly float scalingE = 1 / 3f - scalingD * Mathf.Sqrt(scalingC);


    //float MidDisplacementScaling(float x) => scalingB * Mathf.Log(scalingC + x);
    float MidDisplacementScaling(float x) => scalingD * Mathf.Sqrt(scalingC + x) + scalingE;


    protected virtual void LateUpdate() {
        if (startPoint == null || endPoint == null) return;
        if (splinificationType != lastSplinificationType || Activity > 0) {
            lastSplinificationType = splinificationType;
            finished = false;
        }
        if (finished) return;

        switch (splinificationType) {
            case SplinificationType.Never:
                useSplines = false;
                break;
            case SplinificationType.Always:
            case SplinificationType.WhenSimulationSlowsDown 
                when Activity < 1 && Mathf.SmoothStep(0, 1, Age) > Activity:
            case SplinificationType.WhenSimulationHasStopped 
                when Activity == 0:
                useSplines = true;
                break;
            case SplinificationType.WhenSimulationHasStopped:
            case SplinificationType.WhenSimulationSlowsDown:
                useSplines = false;
                break;
            default: break;
        }
        if (Activity == 0) finished = true;

        if (useSplines)
            UpdateSpline();
        else 
            UpdateLine();
    }


    void UpdateLine() {
        lineRenderer.enabled = true;
        meshRenderer.enabled = false;
        Vector3 startPosition = StartPoint.transform.position;
        Vector3 endPosition = EndPoint.transform.position;
        Vector3 lineDirection = (endPosition - startPosition).normalized;
        Vector3 startPointWithSpacing = startPosition + lineDirection * vertexRadius;
        Vector3 endPointWithSpacing = endPosition - lineDirection * vertexRadius;

        startColor.a = StartPoint.EdgeCompletion;
        endColor.a = EndPoint.EdgeCompletion;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        lineRenderer.widthCurve = new(
            new Keyframe(time: 0, value: lineWidth)
            , new Keyframe(time: 0.999f - PercentHead, value: lineWidth) // neck of arrow
            , new Keyframe(time: 1 - PercentHead, value: arrowWidth) // max width of arrow head
            , new Keyframe(time: 1, value: 0f) // tip of arrow
        ); 

        lineRenderer.SetPositions(new[] {
            startPointWithSpacing,
            Vector3.Lerp(a: startPointWithSpacing, b: endPointWithSpacing, t: 0.999f - PercentHead),
            Vector3.Lerp(a: startPointWithSpacing, b: endPointWithSpacing, t: 1 - PercentHead), endPointWithSpacing
        });
    }

    void UpdateSpline() {
        lineRenderer.enabled = false;
        meshRenderer.enabled = true;
        var startPoint = StartPoint;
        var endPoint = EndPoint; // in case we update how the property works

        startColor.a = StartPoint.EdgeCompletion;
        endColor.a = EndPoint.EdgeCompletion;
        splineRenderer.colorModifier.keys[0].color = startColor;
        splineRenderer.colorModifier.keys[1].color = endColor;
        

        Vector3 startPosition = startPoint.transform.position;
        Vector3 endPosition = endPoint.transform.position;
        Vector3 vector = endPosition - startPosition;

        //if (!startPoint.splineDirections.ContainsKey(Label))
            startPoint.RecalculateSplineDirections();
        //if (!endPoint.splineDirections.ContainsKey(Label))
            endPoint.RecalculateSplineDirections();
        Vector3 startDirection = startPoint.splineDirections[Label];
        Vector3 endDirection = endPoint.splineDirections[Label];

        Vector3 midDisplacementDirectionNonOrthogonal = startDirection - endDirection;
        Vector3 midDisplacementDirection = Vector3.ProjectOnPlane(midDisplacementDirectionNonOrthogonal, vector.normalized);
        float l = midDisplacementDirection.magnitude;
        float lambda = MathF.Sqrt(startDirection.sqrMagnitude + endDirection.sqrMagnitude);
        /* overly complicated (and non-working) way to get a random vector orthogonal to vector and not needed bc. the midDisplacement doesn't have to be large.
        while (l < 0.1 * lambda) {
            // replace by random vector (take old random vector if nothing changed)
            if ((vectorForOldRandomMidDisplacement - midDisplacementDirectionNonOrthogonal).sqrMagnitude > 0.01) {
                oldRandomMidDisplacement = Vector3.ProjectOnPlane(Random.insideUnitSphere, vector.normalized);
                vectorForOldRandomMidDisplacement = vector;
            }
            midDisplacementDirection += oldRandomMidDisplacement;
            l = midDisplacementDirection.magnitude;
        }
        */
        Vector3 midDisplacementVector = l > 0.001f ? midDisplacementFactor * MidDisplacementScaling(l / lambda) / l * midDisplacementDirection : Vector3.zero;
        Vector3 midPosition = startPosition + 0.5f * vector + midDisplacementVector;
        Vector3 midDirection = (vector - endDirection - startDirection) * midDirectionFactor;
        // this is approximately the vector from the "corner" of the spline after the start to the "corner" before the end


        splineComputer.SetPoints(new[] {
            new SplinePoint(startPosition, startPosition - startDirection),
            new SplinePoint(midPosition, midPosition - midDirection),
            new SplinePoint(endPosition, endPosition - endDirection) 
            // Why do we have to subtract the direction? and why from the position?
        });
    }

    public virtual Vertex GetOpposite(Vertex vertex) {
        if (vertex.Equals(StartPoint)) return EndPoint;
        if (vertex.Equals(EndPoint)) return StartPoint;
        throw new Exception("Vertex is not part of this edge.");
    }

    //public virtual char ReverseLabel => RelatorDecoder.invertGenerator(Label); // This should be in the subclass
}
