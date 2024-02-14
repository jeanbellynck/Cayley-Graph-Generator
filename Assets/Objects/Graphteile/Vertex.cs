using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Vertex : MonoBehaviour {


    public readonly Dictionary<char, Vector3> splineDirections = new();
    const int SplineDirectionUpdateFrameInterval = 15;
    [SerializeField] float splineDirectionFactor = 0.2f; // actually I would like to see these in the inspector AND have them be static
    [SerializeField] float orthogonalSplineDirectionFactor = 0.1f;
    [SerializeField] bool preferEights = false;
    [SerializeField] float equalDirectionDisplacementFactor = 0.3f;


    Dictionary<char, Vector3> preferredRandomDirections = new ();
    Dictionary<char, Vector3> fixedPreferredRandomDirections = new ();
    readonly Dictionary<char, Vector3> fallbackRandomDirections = new ();


    public int Id { get; set; }

    [SerializeField] float mass = 1;
    public float Mass { get => mass; protected set => mass = value; }

    public VectorN Velocity { get => Velocity1; set => Velocity1 = value; }
    public VectorN Force { get; set; }

    public Dictionary<char, HashSet<Edge>> LabeledOutgoingEdges { get; set; } = new();
    public Dictionary<char, HashSet<Edge>> LabeledIncomingEdges { get; set; } = new();
    protected Renderer Mr { get; private set; }

    [SerializeField] VectorN position;
    VectorN previousPosition; // This is the previous position of the vertex. It is used for smooth lerp animations

    float creationTime; // = Time.time;
    public float Age => Time.time - creationTime;

    public VectorN Position { get => position; set => position = value; }
    VectorN Velocity1 { get; set; }

    // Start is called before the first frame update
    protected virtual void Start() {
        creationTime = Time.time;
        if (Mass == 0) {
            Mass = 0.1f;
        }
        Mr = GetComponent<Renderer>();
        Update();
    }

    protected void Initialize(VectorN position) {
        this.position = position;
        VectorN zero = VectorN.Zero(position.Size());
        Velocity1 = zero;
        Force = zero;
        //StartCoroutine(CalculateSplineDirections()); // weird: in node n, this is at some point never called again // moved to edge
    }

    protected virtual void Update() {
        
    }

    public void OnDrawGizmos() {
        // Draw a line pointing in the direction of the force
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + VectorN.ToVector3(Velocity));
        // Draw a sphere at the current position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(VectorN.ToVector3(position), 0.1f);
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

    public void Reset(int dimension) {
        creationTime = Time.time;
        VectorN zero = VectorN.Zero(dimension);
        Force = zero;
        Position = zero;
        Velocity = zero;
        splineDirections.Clear();
        preferredRandomDirections.Clear();
        fallbackRandomDirections.Clear();
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
        if (edge.StartPoint.Equals(this)) {
            AddOutgoingEdge(edge);
        }
        if (edge.EndPoint.Equals(this)) {
            AddIncomingEdge(edge);
        }
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
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    void RemoveOutgoingEdge(Edge edge) {
        char label = edge.Label;
        if (LabeledOutgoingEdges.ContainsKey(label)) {
            LabeledOutgoingEdges[label].Remove(edge);
        }
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
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    void RemoveIncomingEdge(Edge edge) {
        char label = edge.Label;
        if (LabeledIncomingEdges.ContainsKey(label)) {
            LabeledIncomingEdges[label].Remove(edge);
        }
    }



}
