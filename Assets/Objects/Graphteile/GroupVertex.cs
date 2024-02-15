using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GroupVertex : Vertex {
    [SerializeField] int distanceToNeutralElement = 0; // This is the distance to the neutral element of the group. It is used to determine the distance to the neutral element of the group. Currently this is not properly updated.
    [SerializeField] List<string> pathsToNeutralElement = new(); // The paths to the identity element. This is used to visualize the paths to the identity element.

    public float Stress { get; private set; }

    public int DistanceToNeutralElement { get => distanceToNeutralElement;
        private set => distanceToNeutralElement = value; }
    public List<string> PathsToNeutralElement { get => pathsToNeutralElement; protected set => pathsToNeutralElement = value; }


    public override float EdgeCompletion { get; protected set; }

    void CalculateEdgeCompletion()
    {
        EdgeCompletion = (float)(
            LabeledIncomingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty()) +
            LabeledOutgoingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty())
        ) / graphManager.LabelCount;
    }

    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();

        Stress = (from generator in LabeledOutgoingEdges.Keys
            let inEdge = GetIncomingEdges(generator).FirstOrDefault()?.Direction ?? Vector3.zero
            let outEdge = GetOutgoingEdges(generator).FirstOrDefault()?.Direction ?? Vector3.zero
            select Vector3.Angle(inEdge, outEdge) / 180
        ).DefaultIfEmpty(0).Max();
        Mr.material.color = new Color(Stress, 0, 0, EdgeCompletion);
    }

    public override void Initialize(VectorN position, GraphManager graphManager) {
        OnEdgeChange -= CalculateEdgeCompletion;
        OnEdgeChange += CalculateEdgeCompletion;
        base.Initialize(position, graphManager);
    }

    public void InitializeFromPredecessor(GroupVertex predecessor, char op, float hyperbolicScaling) {
        Initialize(predecessor.Position, predecessor.graphManager);
        name = predecessor.name + op;

        GroupVertex prepredecessor = predecessor.FollowEdge(ToggleCase(op));
        if (prepredecessor != null) {
            VectorN diff = predecessor.Position - prepredecessor.Position;
            Position = predecessor.Position + hyperbolicScaling * (diff.Normalize()*hyperbolicScaling + VectorN.Random(predecessor.Position.Size(), 0.1f));
        }
        else {
            Position = predecessor.Position +  VectorN.Random(predecessor.Position.Size(), hyperbolicScaling);
        }
        Velocity = VectorN.Zero(Position.Size());
        transform.position = VectorN.ToVector3(Position);

        DistanceToNeutralElement = predecessor.DistanceToNeutralElement + 1;
        List<string> pathsToNeutralElement = predecessor.PathsToNeutralElement;
        foreach (string path in pathsToNeutralElement) {
            AddPathToNeutralElement(path + op);
        }
        calculateVertexMass(hyperbolicScaling);
    }

    public void Merge(GroupVertex vertex2, float hyperbolicity) {
        Initialize((Position + vertex2.Position) / 2, graphManager);
        foreach(string path in vertex2.PathsToNeutralElement) {
            AddPathToNeutralElement(path);
        }
        calculateVertexMass(hyperbolicity);
    }

    public void calculateVertexMass(float hyperbolicity) {
        Mass =  Mathf.Pow(hyperbolicity, DistanceToNeutralElement);


        /**float mass = 1;
        int rootExponent = 0;
        foreach (string path in pathsToIdentity) {
            foreach (char gen in generators) {
                mass *= calculateScalingForGenerator(gen, path);
                rootExponent++;
            }
        }
        mass = Mathf.Pow(mass, 1f / rootExponent);
        if(mass == 0) {
            throw new System.Exception("The mass of the vertex is 0. This is not allowed.");
        }
        return mass;**/
        /**
        float mass = float.MaxValue;
        foreach (string path in pathsToIdentity) {
            foreach (char gen in generators) {
                float massCandidate = calculateScalingForGenerator(gen, path);
                if (massCandidate < mass) {
                    mass = massCandidate;
                }
            }
        }
        return mass;**/
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
