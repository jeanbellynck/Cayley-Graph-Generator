using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Vertex : MonoBehaviour, ITooltipOnHover {


    public int Id { get; set; }

    [SerializeField] float mass = 1;
    public float Mass { get => mass; protected set => mass = value; }

    [field: SerializeField] public VectorN Velocity { get; set; }
    [field: SerializeField] public VectorN Force { get; set; }

    public Dictionary<char, HashSet<Edge>> LabeledOutgoingEdges { get; } = new();
    public Dictionary<char, HashSet<Edge>> LabeledIncomingEdges { get; } = new();
    protected Renderer Mr { get; private set; }
    [SerializeField] protected TooltipContent tooltipContent = new();

    public float CreationTime { get; protected set; } // = Time.time;
    [field: SerializeField] public float Radius { get; protected set; }
    public float Age => Time.time - CreationTime;

    protected readonly HashSet<HighlightType> activeHighlightTypes = new();

    public virtual float Importance => IsHighlighted() ? 1 : MathF.Min(1, baseImportance);
    public float baseImportance = 1;


    public event Action OnEdgeChange;

    [field: SerializeField] public VectorN Position { get; set; }

    public readonly Dictionary<char, Vector3> splineDirections = new();
    [SerializeField] float splineDirectionFactor = 0.2f; // actually I would like to see these in the inspector AND have them be static
    [SerializeField] float orthogonalSplineDirectionFactor = 0.1f;
    [SerializeField] bool preferEights = false;
    [SerializeField] float equalDirectionDisplacementFactor = 0.3f;


    Dictionary<char, Vector3> preferredRandomDirections = new ();
    Dictionary<char, Vector3> fixedPreferredRandomDirections = new ();
    readonly Dictionary<char, Vector3> fallbackRandomDirections = new ();

    [SerializeField] protected GraphVisualizer graphVisualizer;
    [SerializeField] IActivityProvider activityProvider;
    [SerializeField] float maxSpeed;
    protected float Activity => activityProvider.Activity;

    CenterPointerToTransform _centerPointer;
    // this is a workaround for the subclasses where a vertex might get merged with another vertex and the outside would still point to the old vertex (often destroyed at that point)
    public CenterPointerToTransform centerPointer => _centerPointer ??= new() { transform = transform }; // lazy initialization

    protected virtual void Start() {
        if (Mass == 0) Mass = 0.1f;
        Mr = GetComponent<Renderer>();
    }

    public virtual void Initialize(VectorN position, GraphVisualizer graphVisualizer) {
        Start();
        CreationTime = Time.time;

        this.graphVisualizer = graphVisualizer;
        this.activityProvider = graphVisualizer;

        VectorN zero = VectorN.Zero(position.Size());
        Position = position;
        Force = zero;
        Velocity = zero;

        splineDirections.Clear();
        preferredRandomDirections.Clear();
        fallbackRandomDirections.Clear();
        
        //StartCoroutine(CalculateSplineDirections()); // weird: in node n, this is at some point never called again // moved to edge
        //OnEdgeChange += RecalculateSplineDirections; // would be ok, but not necessary, as the edges call this method themselves

        OnEdgeChange?.Invoke();
    }

    protected virtual void Update() {
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + VectorN.ToVector3(Force));
        // Draw a line pointing in the direction of the force
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + VectorN.ToVector3(Velocity));
        // Draw a sphere at the current position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(VectorN.ToVector3(Position), 0.1f);
    }
    
    public void Destroy(bool simply = false) {
        // Destroy all edges too
        foreach (HashSet<Edge> genEdges in LabeledIncomingEdges.Values) {
            List<Edge> genEdgesCopy = new(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                edge.Destroy(simply);
            }
        }
        foreach (HashSet<Edge> genEdges in LabeledOutgoingEdges.Values) {
            List<Edge> genEdgesCopy = new(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                edge.Destroy(simply);
            }
        }
        StopAllCoroutines();
        GameObject.Destroy(gameObject);
    }

    public bool Equals(Vertex other) {
        return other != null && Id == other.Id;
    }


    //IEnumerator<int> CalculateSplineDirections() {
    //    var r = Random.Range(0, SplineDirectionUpdateFrameInterval);
    //    while (true) {
    //        if (Time.frameCount % SplineDirectionUpdateFrameInterval != r) {
    //            yield return 0;
    //            continue;
    //        }
    //        RecalculateSplineDirections();
    //        yield return 1;
    //    }
    //}

    public void RecalculateSplineDirections(){
        var labels = LabeledIncomingEdges.Keys.Union(LabeledOutgoingEdges.Keys).ToList();
        labels.Sort();
        List<char> randomLabels = new();

        var incomingAverages = new Dictionary<char, Vector3>();
        var outgoingAverages = new Dictionary<char, Vector3>();


        foreach (var label in labels) {
            var inAvg = incomingAverages[label] = Helpers.Average(from edge in GetIncomingEdges(label)
                select edge.Direction);
            var outAvg = outgoingAverages[label] = Helpers.Average(from edge in GetOutgoingEdges(label)
                select edge.Direction);
            var res = 0.5f * splineDirectionFactor * (inAvg + outAvg);
            if (res.sqrMagnitude < 0.005f * splineDirectionFactor * splineDirectionFactor *
                (inAvg.sqrMagnitude + outAvg.sqrMagnitude)) {
                randomLabels.Add(label);
            }

            splineDirections[label] = res;
        }

        // assert: randomLabels.Sort() is unnecessary

        var preferredRandomDirectionsKeys = preferredRandomDirections.Keys.ToList();
        preferredRandomDirectionsKeys.Sort(); // should be unnecessary as the dict is never changed (overwritten), thus the ordering 'should' be preserved
        if (!randomLabels.SequenceEqual(preferredRandomDirectionsKeys)) {
            var l = randomLabels.Count;
            preferredRandomDirections = new(
                from i in Enumerable.Range(0, l)
                select new KeyValuePair<char, Vector3>(
                    randomLabels[i], 
                    Helpers.distributedVectors[l][i]
                )
            );
        }

        var fixedPreferredRandomDirectionsKeys = fixedPreferredRandomDirections.Keys.ToList();
        fixedPreferredRandomDirectionsKeys.Sort(); // should be unnecessary as the dict is never changed (overwritten), thus the ordering 'should' be preserved
        if (!labels.SequenceEqual(fixedPreferredRandomDirectionsKeys)) {
            var l = labels.Count;
            fixedPreferredRandomDirections = new(
                from i in Enumerable.Range(0, l)
                select new KeyValuePair<char, Vector3>(
                    labels[i],
                    Helpers.distributedVectors[l][i]
                )
            );
        }

        foreach (var label in randomLabels) 
            splineDirections[label] = RandomOrthogonalDirection(label, incomingAverages[label] - outgoingAverages[label], false ) * orthogonalSplineDirectionFactor; // actually, this is just 2*incomingAverages[label]

        foreach (List<char> similarLabels in Helpers.GroupVectorsByAngle(splineDirections)) {
            int similarLabelsCount = similarLabels.Count;
            if (similarLabelsCount <= 1) continue;
            var factor = equalDirectionDisplacementFactor / Mathf.Sqrt(  similarLabelsCount );
            var middle = (similarLabelsCount - 1) / 2f;

            similarLabels.Sort(); // the order of splineDirections' keys is not guaranteed to be preserved
            var label = similarLabels[Mathf.RoundToInt(middle)];
            var displacementDirection = RandomOrthogonalDirection(label, incomingAverages[label] - outgoingAverages[label], true);

            //similarLabels.Sort();

            for (int i = 0; i < similarLabelsCount; i++) {
                var localFactor = factor * (i - middle);
                if (!InPositiveHalfSpace(splineDirections[similarLabels[i]]))
                    localFactor = -localFactor;
                splineDirections[similarLabels[i]] += localFactor * displacementDirection;
            }
        }

        return;

        bool InPositiveHalfSpace(Vector3 v) {
            if (v.x > 0) return true;
            if (v.x < 0) return false;
            if (v.y > 0) return true;
            if (v.y < 0) return false;
            if (v.z > 0) return true;
            return false;
        }

        Vector3 RandomOrthogonalDirection(char label, Vector3 direction, bool @fixed) {
            var localPreferredRandomDirections = @fixed ? fixedPreferredRandomDirections : preferredRandomDirections;
            if (!localPreferredRandomDirections.TryGetValue(label, out var preferredRandomDirection))
            {
                // should not happen anymore!
               preferredRandomDirection = localPreferredRandomDirections[label] = Random.onUnitSphere;
                // TODOne: choose this so that it preferredRandomDirections contains vectors that are maximally spread (in RP2), i.e. the dot product of any two is minimal
            }
            var randomDirection = Vector3.ProjectOnPlane(preferredRandomDirection, direction.normalized);
            //assert: preferredRandomDirection has norm 1
            const float minimalLengthOfProjection = 0.01f;

            if (randomDirection.sqrMagnitude < minimalLengthOfProjection) {
                if (!fallbackRandomDirections.TryGetValue(label, out var fallbackRandomDirection))
                    while(true){
                        fallbackRandomDirection = Vector3.Cross(Random.onUnitSphere, preferredRandomDirection);
                        if( fallbackRandomDirection.sqrMagnitude < minimalLengthOfProjection) continue;
                        fallbackRandomDirections[label] = fallbackRandomDirection = fallbackRandomDirection.normalized;
                        break;
                    }

                randomDirection = Vector3.ProjectOnPlane(fallbackRandomDirection, direction.normalized);
            }


            if (!preferEights && !InPositiveHalfSpace(direction)) {
                randomDirection = -randomDirection;
            }

            return direction.magnitude * randomDirection.normalized;
        }
    }

    public IEnumerable<Edge> GetIncomingEdges(char op) {
        return LabeledIncomingEdges.TryGetValue(op, out var edges) ? edges : new();
    }


    public IEnumerable<Edge> GetOutgoingEdges(char op) {
        return LabeledOutgoingEdges.TryGetValue(op, out var edges) ? edges : new();
    }


    /**
     * This method is used to add an edge to the list of edges of this vertex. 
     * It dynamically checks whether this vertex is the start or the end. The vertex therefore need to already be set as start or end.
     * If an edge with the same generator and the same endpoints already exists, it is not added.
     */
    public void AddEdge(Edge edge) {
        // Determine whether this the edge points to this vertex or away from it
        if (edge.StartPoint.Equals(this)) 
            AddOutgoingEdge(edge);
        else if (edge.EndPoint.Equals(this)) 
            AddIncomingEdge(edge);
        else
            throw new("The edge does not point to this vertex.");
    }

    public void RemoveEdge(Edge edge) {
        if (edge.StartPoint.Equals(this)) {
            RemoveOutgoingEdge(edge);
        }
        if (edge.EndPoint.Equals(this)) {
            RemoveIncomingEdge(edge);
        }
    }

    /**
     * To add an Edge, use the method addEdge instead
     **/
    void AddOutgoingEdge(Edge edge) {
        char generator = edge.Label;
        if (!LabeledOutgoingEdges.ContainsKey(generator)) {
            LabeledOutgoingEdges.Add(generator, new());
        }
        LabeledOutgoingEdges[generator].Add(edge);
        OnEdgeChange?.Invoke();
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    void RemoveOutgoingEdge(Edge edge) {
        char label = edge.Label;
        if (LabeledOutgoingEdges.ContainsKey(label)) {
            LabeledOutgoingEdges[label].Remove(edge);
        }
        OnEdgeChange?.Invoke();
    }

    /**
     * To add an Edge, use the method addEdge instead
     **/
    void AddIncomingEdge(Edge edge) {
        char label = edge.Label;
        LabeledIncomingEdges.TryAdd(label, new());
        LabeledIncomingEdges[label].Add(edge);
        OnEdgeChange?.Invoke();
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    void RemoveIncomingEdge(Edge edge) {
        if (LabeledIncomingEdges.TryGetValue(edge.Label, out HashSet<Edge> incomingEdges)) 
            incomingEdges.Remove(edge);
        OnEdgeChange?.Invoke();
    }
    
    // ITooltipOnHover
    public TooltipContent GetTooltip() => tooltipContent;

    public virtual void OnClick(Kamera activeKamera) { } // => OnCenter?.Invoke(activeKamera);
    public virtual void OnHover(Kamera activeKamera) { }

    public virtual void OnHoverEnd() { }

    public void Center() => centerPointer.Center(); // this is a workaround for the fact that events can only be called from the class they are defined in (not even subclasses)

    public virtual void Highlight(HighlightType mode, Func<string, (IEnumerable<char>, IEnumerable<char>)> followEdges,
        bool removeHighlight, bool keepGoingWhenAlreadyDone)
    => Highlight(
        mode, followEdges, "", removeHighlight,
        keepGoingWhenAlreadyDone, mode switch {
            HighlightType.Subgroup when removeHighlight => "-SG",
            HighlightType.Subgroup => "+SG",
            HighlightType.Path when removeHighlight => "~" + name,
            HighlightType.PrimaryPath when removeHighlight => "~" + name,
            HighlightType.Path => "#" + name,
            HighlightType.PrimaryPath => "#" + name,
            HighlightType.Selected => "@" + name,
            _ => "?" + name
        });

    protected virtual void Highlight(HighlightType mode, Func<string, (IEnumerable<char>, IEnumerable<char>)> followEdges, string path, bool removeHighlight, bool keepGoingWhenAlreadyDone, string id) {
        if (this == null) return;
        var wasAlreadyDone = removeHighlight ? !UnHighlight(mode) : !Highlight(mode);
        if (wasAlreadyDone && !keepGoingWhenAlreadyDone) 
            return;

        // I only split the in and out labels in followEdges bc. I want to implement this for Vertices, not GroupVertices AND we don't have the "toUpper" for inverses of the generators of the subgroup!
        var (outLabels, inLabels) = followEdges(path);
        foreach (var label in outLabels) {
            foreach (var edge in GetOutgoingEdges(label).Take(1)) { 
                // todo: only highlight the first edge? in a monoid there might actually be more than one here, but only one leads back to the identity!
                edge.Highlight(mode, removeHighlight);
                edge.EndPoint.PlanHighlight(mode, followEdges, path + label, removeHighlight, keepGoingWhenAlreadyDone, id);
            }
        }

        foreach (var label in inLabels) {
            foreach (var edge in GetIncomingEdges(label).Take(1)) {
                edge.Highlight(mode, removeHighlight);
                edge.StartPoint.PlanHighlight(mode, followEdges, path + char.ToUpper(label), removeHighlight, keepGoingWhenAlreadyDone, id); 
                // here, we call ToUpper also for the generators of the subgroup (0,1,2,...) but it doesn't matter, since in this case the input to FollowEdges is ignored anyway
            }
        }
    }

    protected virtual void PlanHighlight(HighlightType mode, Func<string, (IEnumerable<char>, IEnumerable<char>)> followEdges,
        string path, bool removeHighlight, bool keepGoingWhenAlreadyDone, string id) {
        graphVisualizer.PlanAction((
            () => Highlight(mode, followEdges, path, removeHighlight, keepGoingWhenAlreadyDone, id),
            id,
            1
        ));
    }


    Color unhighlightedColor = Color.clear;
    float unhighlightedRadius = 0f;
    [SerializeField] Mesh selectedMesh;
    Mesh unselectedMesh;

    void SetUnhighlightedValues(HighlightType mode) {
        switch (mode) {
            case HighlightType.Subgroup when unhighlightedColor == Color.clear:
                unhighlightedColor = Mr.material.color;
            break;
            case HighlightType.Selected:
                if (unhighlightedRadius == 0f)
                    unhighlightedRadius = Radius;
                if (unselectedMesh == null)
                    unselectedMesh = GetComponent<MeshFilter>().mesh;
            break;
        }
    }

    public virtual bool Highlight(HighlightType mode) {
        SetUnhighlightedValues(mode);
        if (IsHighlighted(mode))
            return false;
        activeHighlightTypes.Add(mode);
        
        switch (mode) { // todo
            case HighlightType.Subgroup:
                Mr.material.color = Color.red;
                break;
            case HighlightType.Selected:
                SetRadius(unhighlightedRadius * 1.6f);
                GetComponent<MeshFilter>().mesh = selectedMesh;
                break;
        }
        return true;
    }

    public virtual bool UnHighlight(HighlightType mode) {
        SetUnhighlightedValues(mode);
        if (!IsHighlighted(mode))
            return false;
        activeHighlightTypes.Remove(mode);

        switch(mode){
            case HighlightType.Subgroup:
                Mr.material.color = unhighlightedColor;
            break;
            case HighlightType.Selected:
                SetRadius(unhighlightedRadius);
                GetComponent<MeshFilter>().mesh = unselectedMesh;
            break;
        }
        return true;
    }

    public bool IsHighlighted(HighlightType mode) => activeHighlightTypes.Contains(mode);
    public bool IsHighlighted() => activeHighlightTypes.Count > 0;

    public virtual void SetRadius(float radius) {
        this.Radius = radius;
        transform.localScale = Vector3.one * radius * 2;
    }
    protected void SetRadius() => SetRadius(Radius);

    public void GreyOut(bool greyedOut) {
        baseImportance = greyedOut ? 0.3f : 1f;
        SetRadius();
    }

}
