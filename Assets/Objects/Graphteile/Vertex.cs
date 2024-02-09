using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Vertex : MonoBehaviour {
    [SerializeField]
    private VectorN position;
    private VectorN previousPosition; // This is the previous position of the vertex. It is used for smooth lerp animations
     private VectorN velocity; // This is the previous position of the vertex. It is used to calculate the forces using the improved euler method.
    private int id;
    private float age = 0;
    [SerializeField]
    private float mass = 1; // The mass of the vertex. This is used to calculate the repulsion force. It depends on the hyperbolicity and the distance to the neutral element.
   
    private VectorN repelForce;
    private VectorN linkForce;

    private Dictionary<char, List<Edge>> labeledOutgoingEdges = new Dictionary<char, List<Edge>>();
    private Dictionary<char, List<Edge>> labeledIncomingEdges = new Dictionary<char, List<Edge>>();

    public readonly Dictionary<char, Vector3> splineDirections = new();
    private readonly int SplineDirectionUpdateFrame = 10;
    public float splineDirectionFactor = 0.2f;
    public float orthogonalSplineDirectionFactor = 0.1f;

    Dictionary<char, Vector3> preferredRandomDirections = new ();
    readonly Dictionary<char, Vector3> fallbackRandomDirections = new ();
    public bool preferEights = false;

    private Renderer mr;


    public int Id { get => id; set => id = value; }
    public float Mass { get => mass; set => mass = value; }
    public float Age { get => age; set => age = value; }
    public VectorN Velocity { get => Velocity1; set => Velocity1 = value; }
    public VectorN RepelForce { get => repelForce; set => repelForce = value; }
    public VectorN LinkForce { get => linkForce; set => linkForce = value; }
    public Dictionary<char, List<Edge>> LabeledOutgoingEdges { get => labeledOutgoingEdges; set => labeledOutgoingEdges = value; }
    public Dictionary<char, List<Edge>> LabeledIncomingEdges { get => labeledIncomingEdges; set => labeledIncomingEdges = value; }
    public Renderer Mr { get => mr; set => mr = value; }
    public VectorN Position { get => position; set => position = value; }
    public VectorN Velocity1 { get => velocity; set => velocity = value; }

    // Start is called before the first frame update
    public virtual void Start() {
        if (mass == 0) {
            mass = 0.1f;
        }
        mr = GetComponent<Renderer>();
        Update();
    }

    public void Initialize(VectorN position) {
        age = 0;
        this.position = position;
        velocity = VectorN.Zero(position.Size());
        linkForce = VectorN.Zero(position.Size());
        repelForce = VectorN.Zero(position.Size());
        StartCoroutine(CalculateSplineDirections());
    }

    // Update is called once per frame
    public virtual void Update() {
        age += Time.deltaTime;
        if (Time.renderedFrameCount % 10 == 0) {
            splineDirections.Clear(); // Recompute spline directions every 10 frames
        }
    }


    /**
    * Used for debugging.
    */
    void OnDrawGizmos() {
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, transform.position + VectorN.ToVector3(repelForce));
        //Gizmos.color = Color.cyan;
        //Gizmos.DrawLine(transform.position, transform.position + VectorN.ToVector3(linkForce));
    }

    public void Destroy() {
        // Destroy all edges too
        foreach (List<Edge> genEdges in labeledIncomingEdges.Values) {
            List<Edge> genEdgesCopy = new List<Edge>(genEdges);
            foreach (Edge edge in genEdgesCopy) {
                edge.Destroy();
            }
        }
        foreach (List<Edge> genEdges in labeledOutgoingEdges.Values) {
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

    public void Reset() {
        age = 0;
        repelForce = VectorN.Zero(position.Size());
        linkForce = VectorN.Zero(position.Size());
        splineDirections.Clear();
        preferredRandomDirections.Clear();
        fallbackRandomDirections.Clear();
    }

    IEnumerator<int> CalculateSplineDirections() {
        var r = Random.Range(0, SplineDirectionUpdateFrame);
        while (true) {
            if (Time.frameCount % SplineDirectionUpdateFrame != r) yield return 0;
            RecalculateSplineDirections();
            yield return 1;
        }
    }

    public void RecalculateSplineDirections(){
        var labels = LabeledIncomingEdges.Keys.Union(LabeledOutgoingEdges.Keys);
        List<char> randomLabels = new();

        var incomingAverages = new Dictionary<char, Vector3>();
        var outgoingAverages = new Dictionary<char, Vector3>();


        foreach (var label in labels) {
            var inAvg = incomingAverages[label] = Helpers.Average(from edge in GetIncomingEdges(label)
                select edge.direction);
            var outAvg = outgoingAverages[label] = Helpers.Average(from edge in GetOutgoingEdges(label)
                select edge.direction);
            var res = 0.5f * splineDirectionFactor * (inAvg + outAvg);
            if (res.sqrMagnitude < 0.005f * splineDirectionFactor * splineDirectionFactor *
                (inAvg.sqrMagnitude + outAvg.sqrMagnitude)) {
                randomLabels.Add(label);
            }

            splineDirections[label] = res;
        }

        randomLabels.Sort();
        if (!randomLabels.SequenceEqual(preferredRandomDirections.Keys)) {
            var l = randomLabels.Count;
            preferredRandomDirections = new (
                from labelIndexTuple in randomLabels.Zip(
                    Enumerable.Range(0, l),
                    (c, i) => (c, i)
                )
                select new KeyValuePair<char, Vector3>(
                    labelIndexTuple.c, 
                    Helpers.distributedVectors[l][labelIndexTuple.i]
                )
            );
        }
        foreach (var label in randomLabels) 
            splineDirections[label] = RandomOrthogonalDirection(label, (incomingAverages[label] - outgoingAverages[label]) * 0.5f); // actually, this is just incomingAverages[label] again

        foreach (List<char> similarLabels in Helpers.GroupVectorsByAngle(splineDirections)) {
            if (similarLabels.Count <= 1) continue;
            var middle = similarLabels.Count / 2f;
            var label = similarLabels[0];
            var displacementDirection = RandomOrthogonalDirection(label, splineDirections[label]);
            for (int i = 0; i < similarLabels.Count; i++) 
                splineDirections[similarLabels[i]] += 0.5f * (i - middle) * displacementDirection;
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

        Vector3 RandomOrthogonalDirection(char label, Vector3 direction) {
            if (!preferredRandomDirections.TryGetValue(label, out var preferredRandomDirection))
            {
                // should not happen anymore!
               preferredRandomDirection = preferredRandomDirections[label] = Random.onUnitSphere;
                // TODOne: choose this so that it preferredRandomDirections contains vectors that are maximally spread (in RP2), i.e. the dot product of any two is minimal
            }
            var randomDirection = Vector3.ProjectOnPlane(preferredRandomDirection, direction.normalized);

            if (randomDirection.sqrMagnitude < 0.005f) {
                Vector3 fallbackRandomDirection;
                while (!fallbackRandomDirections.TryGetValue(label, out  fallbackRandomDirection) || preferredRandomDirection.sqrMagnitude < 0.005f) {
                  fallbackRandomDirections[label] =  preferredRandomDirection = Vector3.Cross(Random.onUnitSphere, preferredRandomDirection);
                }

                randomDirection = Vector3.ProjectOnPlane(fallbackRandomDirection, direction.normalized);
            }


            if (!preferEights && !InPositiveHalfSpace(direction)) {
                randomDirection = -randomDirection;
            }

            return direction.magnitude * orthogonalSplineDirectionFactor * randomDirection.normalized;
        }
    }

    public List<Edge> GetIncomingEdges(char op) {
        if (labeledIncomingEdges.ContainsKey(op)) {
            return labeledIncomingEdges[op];
        }
        else {
            return new List<Edge>();
        }
    }


    public List<Edge> GetOutgoingEdges(char op) {
        if (labeledOutgoingEdges.ContainsKey(op)) {
            return labeledOutgoingEdges[op];
        }
        else {
            return new List<Edge>();
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
    private void AddOutgoingEdge(Edge edge) {
        char generator = edge.Label;
        if (!labeledOutgoingEdges.ContainsKey(generator)) {
            labeledOutgoingEdges.Add(generator, new List<Edge>());
        }
        labeledOutgoingEdges[generator].Add(edge);
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    private void RemoveOutgoingEdge(Edge edge) {
        char label = edge.Label;
        if (labeledOutgoingEdges.ContainsKey(label)) {
            labeledOutgoingEdges[label].Remove(edge);
        }
    }

    /**
     * To add an Edge, use the method addEdge instead
     **/
    private void AddIncomingEdge(Edge edge) {
        char label = edge.Label;
        if (!labeledIncomingEdges.ContainsKey(label)) {
            labeledIncomingEdges.Add(label, new List<Edge>());
        }
        labeledIncomingEdges[label].Add(edge);
    }

    /**
     * To remove an Edge, use the method removeEdge instead
     **/
    private void RemoveIncomingEdge(Edge edge) {
        char label = edge.Label;
        if (labeledIncomingEdges.ContainsKey(label)) {
            labeledIncomingEdges[label].Remove(edge);
        }
    }



}
