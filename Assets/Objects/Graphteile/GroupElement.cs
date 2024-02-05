using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupElement : Vertex {
    public bool isActive = true; // If set inactive it will be ignored algorithms
    private float stress; // Measures how unusual the angles of the vertex are. It is used to visualize weird spots.

    [SerializeField]
    private int distanceToNeutralElement = 0; // This is the distance to the neutral element of the group. It is used to determine the distance to the neutral element of the group. Currently this is not properly updated.
    [SerializeField]
    private List<string> pathsToNeutralElement = new List<string>(); // The paths to the identity element. This is used to visualize the paths to the identity element.
    private Dictionary<char, List<Edge>> labeledEdges = new Dictionary<char, List<Edge>>(); // This is a list-dictionary as vertices may temporarily have multiple arrows of a specific generator. If the vertex is at the border, the arrows might not have been included yet.

    public float Stress { get => stress; set => stress = value; }
    public int DistanceToNeutralElement { get => distanceToNeutralElement; set => distanceToNeutralElement = value; }
    public List<string> PathsToNeutralElement { get => pathsToNeutralElement; set => pathsToNeutralElement = value; }


    // Start is called before the first frame update
    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    public override void Update() {
        base.Update();
        Mr.material.color = new Color(stress, 0, 0);
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
        if (!labeledEdges.ContainsKey(op)) {
            labeledEdges.Add(op, new List<Edge>());
        } 
        Edges.Add(edge);
        labeledEdges[op].Add(edge);
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
        labeledEdges[generator].Remove(edge);
    }


    public Dictionary<char, List<Edge>> GetEdges() {
        return labeledEdges;
    }

    public List<Edge> GetEdges(char op) {
        if(labeledEdges.ContainsKey(op)) {
            return labeledEdges[op];
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
        if (labeledEdges.ContainsKey(op) && labeledEdges[op].Count > 0) {
            return labeledEdges[op][0];
        }
        else {
            return null;
        }
    }

    public GroupElement FollowEdge(char op) {
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

        foreach (Edge e in labeledEdges[generator]) {
            if (e.Equals(edge)) {
                return true;
            }
        }
        return false;
    }

    public void AddPathToNeutralElement(string path) {
        pathsToNeutralElement.Add(path);
    }

    public void AddPathsToNeutralElement(List<string> paths) {
        pathsToNeutralElement.AddRange(paths);
    }
}
