using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Vertex : MonoBehaviour {
    public float age = 0;
    public bool isActive = true; // If set inactive it will be ignored algorithms

    // Used by Link force algorithm
    public Vector3 velocity = Vector3.zero;

    // The following is used in v1 and v2
    public Vector3 repelForce = Vector3.zero;
    public Vector3 attractForce = Vector3.zero;
    public Vector3 oppositeForce = Vector3.zero;
    public Vector3 angleForce = Vector3.zero;
    public float stress; // Measures how unusual the angles of the vertex are. It is used to visualize weird spots.

    private Renderer mr;


    // The following is only used in v2 
    public int id;
    private Dictionary<char, List<Edge>> edges = new Dictionary<char, List<Edge>>(); // This is a list-dictionary as vertices may temporarily have multiple arrows of a specific generator. If the vertex is at the border, the arrows might not have been included yet.

    [SerializeField]
    private int distanceToNeutralElement = 0; // This is the distance to the neutral element of the group. It is used to determine the distance to the neutral element of the group. Currently this is not properly updated.
    [SerializeField]
    public List<string> pathsToNeutralElement = new List<string>(); // The paths to the identity element. This is used to visualize the paths to the identity element.

    [SerializeField]
    private float mass = 1; // The mass of the vertex. This is used to calculate the repulsion force. It depends on the hyperbolicity and the distance to the neutral element.



    // Start is called before the first frame update
    void Start() {
        if(mass == 0) {
            mass = 0.1f;
        }
        mr = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update() {
        //DrawCircle(radius);
        mr.material.color = new Color(stress, 0, 0);
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
        //Gizmos.DrawLine(transform.position, transform.position + repelForce);
        //Gizmos.color = Color.cyan;
        //Gizmos.DrawLine(transform.position, transform.position + angleForce);
    }

    public void SetId(int id) {
        this.id = id;
    }

    /**
     * This method is used to add an edge to the list of edges of this vertex.
     * It dynamically checks whether this vertex is the start or the end. The vertex therefore need to already be set as start or end.
     * If an edge with the same generator and the same endpoints already exists, it is not added.
     */
    public void addEdge(Edge edge) {
        // Determine whether this the edge points to this vertex or away from it
        char op = ' ';
        if (edge.startPoint.Equals(this)) {
            op = edge.getGenerator();
        }
        else if (edge.endPoint.Equals(this)) {
            op = char.ToUpper(edge.getGenerator());
        } else {
            // Throw exception
            throw new Exception("The edge " + edge.name + " does not point to this vertex " + name);
        }
        // Create a new list if there is no list for this generator yet
        // Also check whether an edge of this kind has already been added. If so the edge is deleted.
        if (!edges.ContainsKey(op)) {
            edges.Add(op, new List<Edge>());
        } 
        // 
        
        edges[op].Add(edge);
    }

    public readonly Dictionary<char, Vector3> splineDirections = new();

    public float splineDirectionFactor = 0.2f;
    public float orthogonalSplineDirectionFactor = 0.1f;
    Vector3 oldRandomDirection = Vector3.up;
    public Vector3 CalculateSplineDirection(char generator, Vector3 direction) {

        if (splineDirections.TryGetValue(generator, out var result))
            return result;


        result = direction * splineDirectionFactor;
        var expectedLengthSquared = result.sqrMagnitude;
        char inverseGenerator = RelatorDecoder.invertGenerator(generator);

        bool otherDirectionAlreadyComputed = splineDirections.TryGetValue(inverseGenerator, out var oldResult);
        if (otherDirectionAlreadyComputed)
            result = 0.5f * (result + oldResult);

        // If in- and outgoing splines for this generator are exactly opposite (a^2 = 1), set direction to an orthogonal vector, so that the two edges are not exactly parallel
        if (result.sqrMagnitude < 0.005f * expectedLengthSquared)
            result = RandomOrthogonalDirection();

        foreach (var (gen, splineDirection) in splineDirections) {
            if (gen == generator || gen == inverseGenerator || Vector3.Angle(splineDirection, result) is > 3f and < 177f) continue;
            result += RandomOrthogonalDirection() * 0.5f;
            break;
        }

        splineDirections[generator] = result;
        if (otherDirectionAlreadyComputed)
            splineDirections[inverseGenerator] = result;

        return result;

        Vector3 RandomOrthogonalDirection()
        {
            Vector3 randVector;
            float angle = Vector3.Angle(direction, oldRandomDirection);
            switch (angle)
            {
                case < 91f and > 89f:
                    randVector = oldRandomDirection;
                    break;
                case < 175f and > 5f:
                    randVector = Vector3.ProjectOnPlane(oldRandomDirection, direction.normalized);
                    break;
                default:
                    do randVector = Vector3.Cross(Random.onUnitSphere, direction);
                    while (randVector.sqrMagnitude < 0.005f * expectedLengthSquared);
                    oldRandomDirection = randVector;
                    break;
            }

            return direction.magnitude * orthogonalSplineDirectionFactor * randVector.normalized;
        }
    }

    public void removeEdge(Edge edge) {
        char generator;
        if (edge.startPoint.Equals(this)) {
            generator = edge.getGenerator();
        }
        else if (edge.endPoint.Equals(this)) {
            generator = char.ToUpper(edge.getGenerator());
        }
        else {
            return;
        }
        edges[generator].Remove(edge);
    }


    public Dictionary<char, List<Edge>> GetEdges() {
        return edges;
    }

    public List<Edge> GetEdges(char op) {
        if(edges.ContainsKey(op)) {
            return edges[op];
        }
        else {
            return new List<Edge>();
        }
    }

    /**
    * This method is used to get the edge associated to a generator.
    * WARNING: This always returns the first edge associated to a generator. All others (if present) are ignored
    */
    public Edge GetEdge(char op) {
        if (edges.ContainsKey(op) && edges[op].Count > 0) {
            return edges[op][0];
        }
        else {
            return null;
        }
    }

    public Vertex FollowEdge(char op) {
        Edge edge = GetEdge(op);
        if (edge == null) {
            return null;
        }
        else {
            if (char.IsLower(op)) {
                return edge.endPoint;
            }
            else {
                return edge.startPoint;
            }
        }
    }

    /**
     * Checks whether the given edge is contained in the list of edges of this vertex.
     * i.e. whether is has an edge with the same start point, end point and generator.
     */
    public bool ContainsEdge(Edge edge) {
        char generator;
        if (edge.startPoint != null && edge.startPoint.Equals(this)) {
            generator = edge.getGenerator();
        }
        else if (edge.endPoint != null && edge.endPoint.Equals(this)) {
            generator = char.ToUpper(edge.getGenerator());
        }
        else {
            return false;
        }

        foreach (Edge e in edges[generator]) {
            if (e.Equals(edge)) {
                return true;
            }
        }
        return false;
    }

    public void Destroy() {
        Destroy(gameObject);
    }

    public bool Equals(Vertex other) {
        return id == other.id;
    }

    public void SetDistanceToNeutralElement(int distance) {
        distanceToNeutralElement = distance;
    }

    public int GetDistanceToNeutralElement() {
        return distanceToNeutralElement;
    }

    public void setMass(float mass) {
        this.mass = mass;
    }

    public float getMass() {
        return mass;
    }

    public void SetPathsToNeutralElement(List<string> paths) {
        pathsToNeutralElement = paths;
    }

    public List<string> GetPathsToNeutralElement() {
        return pathsToNeutralElement;
    }

    public void AddPathToNeutralElement(string path) {
        pathsToNeutralElement.Add(path);
    }

    public void AddPathsToNeutralElement(List<string> paths) {
        pathsToNeutralElement.AddRange(paths);
    }
}
