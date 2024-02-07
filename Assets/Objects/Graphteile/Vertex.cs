using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour {
    [SerializeField]
    private VectorN position;
     private VectorN velocity; // This is the previous position of the vertex. It is used to calculate the forces using the improved euler method.
    private int id;
    private float age = 0;
    [SerializeField]
    private float mass = 1; // The mass of the vertex. This is used to calculate the repulsion force. It depends on the hyperbolicity and the distance to the neutral element.
   
    private VectorN repelForce;
    private VectorN linkForce;

    private Dictionary<char, List<Edge>> labeledOutgoingEdges = new Dictionary<char, List<Edge>>();
    private Dictionary<char, List<Edge>> labeledIncomingEdges = new Dictionary<char, List<Edge>>();

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
        this.position = position;
        linkForce = VectorN.Zero(position.Size());
        repelForce = VectorN.Zero(position.Size());
    }

    // Update is called once per frame
    public virtual void Update() {
        age += Time.deltaTime;
        transform.position = VectorN.ToVector3(position);
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
        Destroy(gameObject);
    }

    public bool Equals(Vertex other) {
        return Id == other.Id;
    }

    public List<Edge> GetOutgoingEdges(char op) {
        if (labeledOutgoingEdges.ContainsKey(op)) {
            return labeledOutgoingEdges[op];
        }
        else {
            return new List<Edge>();
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
