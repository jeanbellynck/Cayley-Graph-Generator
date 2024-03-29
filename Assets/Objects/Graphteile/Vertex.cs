using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Vertex : MonoBehaviour {



    public int Id { get; set; }

    [SerializeField] float mass = 1;
    public float Mass { get => mass; protected set => mass = value; }

    public VectorN Velocity { get; set; }
    public VectorN Force { get; set; }

    public Dictionary<char, HashSet<Edge>> LabeledOutgoingEdges { get; set; } = new();
    public Dictionary<char, HashSet<Edge>> LabeledIncomingEdges { get; set; } = new();
    protected Renderer Mr { get; private set; }

    [SerializeField] VectorN _position;
    VectorN previousPosition; // This is the previous position of the vertex. It is used for smooth lerp animations

    float creationTime; // = Time.time;
    public float Age => Time.time - creationTime;

    public virtual float EdgeCompletion {
        get => 1f;
        protected set => throw new NotImplementedException();
    }

    public event Action OnEdgeChange;
    
    public VectorN Position { get => _position; set => _position = value; }

    public readonly Dictionary<char, Vector3> splineDirections = new();
    [SerializeField] float splineDirectionFactor = 0.2f; // actually I would like to see these in the inspector AND have them be static
    [SerializeField] float orthogonalSplineDirectionFactor = 0.1f;
    [SerializeField] bool preferEights = false;
    [SerializeField] float equalDirectionDisplacementFactor = 0.3f;


    Dictionary<char, Vector3> preferredRandomDirections = new ();
    Dictionary<char, Vector3> fixedPreferredRandomDirections = new ();
    readonly Dictionary<char, Vector3> fallbackRandomDirections = new ();

    [SerializeField] public GraphManager graphManager;
    [SerializeField] float maxSpeed;
    public float Activity => graphManager.Activity;


    // Start is called before the first frame update
    protected virtual void Start() {
        if (Mass == 0) {
            Mass = 0.1f;
        }
        Mr = GetComponent<Renderer>();
    }

    public virtual void Initialize(VectorN position, GraphManager graphManager) {
        creationTime = Time.time;

        this.graphManager = graphManager;

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

    float velocityRescaling(float x, float v) => x < v ? 1f : (MathF.Sqrt(x - v + 0.25f) + v - 0.5f) / x;
    protected virtual void Update() {
        if (Activity == 0) {
            transform.position = VectorN.ToVector3(Position);
            foreach (var (gen, edges) in GetEdges()) {
                foreach (var edge in edges) {
                    edge.finished = false; 
                    // TODO! This is a stupid workaround since the edges (in LateUpdate) get to see Activity == 0 first and then stop updating.  (this way we get two full spline renders and expose the variable finished
                }
            }
        }
        else {
            var movingDirection = VectorN.ToVector3(Position) - transform.position;
            transform.position += Time.deltaTime * velocityRescaling(movingDirection.magnitude, maxSpeed / Activity) *
                                  movingDirection;
        }

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
    
    public void Destroy() {
        // Destroy all edges too
        foreach (HashSet<Edge> genEdges in LabeledIncomingEdges.Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                edge.Destroy();
            }
        }
        foreach (HashSet<Edge> genEdges in LabeledOutgoingEdges.Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                edge.Destroy();
            }
        }
        StopAllCoroutines();
        Destroy(gameObject);
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
            preferredRandomDirections = new Dictionary<char, Vector3>(
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
            fixedPreferredRandomDirections = new Dictionary<char, Vector3>(
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

    public HashSet<Edge> GetIncomingEdges(char op) {
        if (LabeledIncomingEdges.ContainsKey(op)) {
            return LabeledIncomingEdges[op];
        }
        else {
            return new HashSet<Edge>();
        }
    }


    public HashSet<Edge> GetOutgoingEdges(char op) {
        if (LabeledOutgoingEdges.ContainsKey(op)) {
            return LabeledOutgoingEdges[op];
        }
        else {
            return new HashSet<Edge>();
        }
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
            throw new Exception("The edge does not point to this vertex.");
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
            LabeledOutgoingEdges.Add(generator, new HashSet<Edge>());
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
        if (!LabeledIncomingEdges.ContainsKey(label)) {
            LabeledIncomingEdges.Add(label, new HashSet<Edge>());
        }
        LabeledIncomingEdges[label].Add(edge);
        OnEdgeChange?.Invoke();
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    void RemoveIncomingEdge(Edge edge) {
        char label = edge.Label;
        if (LabeledIncomingEdges.ContainsKey(label)) {
            LabeledIncomingEdges[label].Remove(edge);
        }
        OnEdgeChange?.Invoke();
    }


    /**
    * This method is used to get a neighbouring vertex by following a labelled edge.
    * WARNING: This always returns the first vertex associated to a generator. All others (if present) are ignored
    */
    public Vertex FollowEdge(char op) {
        return GetEdges(op)?.FirstOrDefault()?.GetOpposite(this);
    }


    protected Dictionary<char, List<Edge>> GetEdges() {
        Dictionary<char, List<Edge>> edges = new Dictionary<char, List<Edge>>();
        foreach (char op in LabeledOutgoingEdges.Keys) {
            edges.Add(op, GetOutgoingEdges(op).ToList());
        }
        foreach (char op in LabeledIncomingEdges.Keys) {
            edges.Add(char.ToUpper(op), GetIncomingEdges(op).ToList());
        }
        return edges;
    }

    protected IEnumerable<Edge> GetEdges(char op) {
        return char.IsLower(op) ? GetOutgoingEdges(op).ToList() : GetIncomingEdges(char.ToLower(op)).ToList();
    }


}
