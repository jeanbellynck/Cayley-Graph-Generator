using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroupVertex : Vertex {
    private float stress; // Measures how unusual the angles of the vertex are. It is used to visualize weird spots.

    [SerializeField]
    private int distanceToNeutralElement = 0; // This is the distance to the neutral element of the group. It is used to determine the distance to the neutral element of the group. Currently this is not properly updated.
    [SerializeField]
    private List<string> pathsToNeutralElement = new List<string>(); // The paths to the identity element. This is used to visualize the paths to the identity element.

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

    public void InitializeFromPredecessor(GroupVertex predecessor, char op, float hyperbolicScaling) {
        base.Initialize(predecessor.Position);
        name = predecessor.name + op;

        GroupVertex prepredecessor = predecessor.FollowEdge(ToggleCase(op));
        if (prepredecessor != null) {
            Position = predecessor.Position + (predecessor.Position - prepredecessor.Position + VectorN.Random(predecessor.Position.Size(), 0.1f)) * hyperbolicScaling;
        }
        else {
            Position = predecessor.Position + hyperbolicScaling * VectorN.Random(predecessor.Position.Size(), 1);
        }
        Velocity = VectorN.Zero(Position.Size());
        transform.position = VectorN.ToVector3(Position);

        DistanceToNeutralElement = predecessor.DistanceToNeutralElement + 1;
        List<string> pathsToNeutralElement = predecessor.PathsToNeutralElement;
        foreach (string path in pathsToNeutralElement) {
            AddPathToNeutralElement(path + op);
        }
        calculateVertexMass();
    }

    public void Merge(GroupVertex vertex2) {
        Position = (Position + vertex2.Position) / 2;
        Velocity = VectorN.Zero(Position.Size());
        foreach(string path in vertex2.PathsToNeutralElement) {
            AddPathToNeutralElement(path);
        }
        calculateVertexMass();
    }

    public void calculateVertexMass() {
        // Temporary implementation
        Mass =  1;
    }

    public Dictionary<char, List<GroupEdge>> GetEdges() {
        Dictionary<char, List<GroupEdge>> edges = new Dictionary<char, List<GroupEdge>>();
        foreach (char op in LabeledOutgoingEdges.Keys) {
            edges.Add(op, GetOutgoingEdges(op).Cast<GroupEdge>().ToList());
        }
        foreach (char op in LabeledIncomingEdges.Keys) {
            edges.Add(char.ToUpper(op), GetIncomingEdges(op).Cast<GroupEdge>().ToList());
        }
        return edges;
    }

    public List<GroupEdge> GetEdges(char op) {
        if (char.IsLower(op))
            return GetOutgoingEdges(op).Cast<GroupEdge>().ToList();
        else
            return GetIncomingEdges(char.ToLower(op)).Cast<GroupEdge>().ToList();
    }

    /**
    * This method is used to get a neighbouring vertex by following a labelled edge.
    * WARNING: This always returns the first vertex associated to a generator. All others (if present) are ignored
    */
    public GroupVertex FollowEdge(char op) {
        List<GroupEdge> edge = GetEdges(op);
        if (edge.Count > 0) {
            return (GroupVertex) edge[0].getOpposite(this);
        }
        else {
            return null;
        }
    }

    public void AddPathToNeutralElement(string path) {
        pathsToNeutralElement.Add(path);
    }

    public void AddPathsToNeutralElement(List<string> paths) {
        pathsToNeutralElement.AddRange(paths);
    }

    
    public char ToggleCase(char c) {
        if (char.IsUpper(c)) {
            return char.ToLower(c);
        }
        else {
            return char.ToUpper(c);
        }
    }
}
