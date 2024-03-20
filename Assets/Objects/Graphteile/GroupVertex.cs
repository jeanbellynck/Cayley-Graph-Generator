using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GroupVertex : Vertex, ITooltipOnHover {
    [SerializeField] int distanceToNeutralElement = 0; 
    [SerializeField] List<string> pathsFromNeutralElement = new(); 

    public float Stress { get; private set; }
    TooltipContent tooltipContent = new();

    public int DistanceToNeutralElement { get => distanceToNeutralElement;
        private set => distanceToNeutralElement = value; }
    public List<string> PathsFromNeutralElement { get => pathsFromNeutralElement; protected set => pathsFromNeutralElement = value; }


    public override float EdgeCompletion { get; protected set; }

    [SerializeField] bool inSubgroup = true;

    void CalculateEdgeCompletion()
    {
        EdgeCompletion = graphManager.LabelCount > 0 ? 0.8f * (
            LabeledIncomingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty()) +
            LabeledOutgoingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty())
        ) / graphManager.LabelCount + 0.2f : 1f;
    }

    protected override void Start() {
        base.Start();
        Update();
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

        //Mr.material.SetColor("_BaseColor", new Color(Stress, 0, 0, EdgeCompletion));
    }

    public new Dictionary<char, List<GroupEdge>> GetEdges() {
        return base.GetEdges().ToDictionary(kvp=> kvp.Key, kvp => kvp.Value.Cast<GroupEdge>().ToList());
    }

    public new List<GroupEdge> GetEdges(char op) {
        return base.GetEdges(op).Cast<GroupEdge>().ToList();
    }

    public new GroupVertex FollowEdge(char op) {
        return (GroupVertex) base.FollowEdge(op);
    }

    public void Initialize(VectorN position, GraphManager graphManager, string name = null, IEnumerable<string> pathsFromNeutralElement = null, bool inSubgroup = true) {
        if (!string.IsNullOrEmpty(name)) this.name = name;
        if (pathsFromNeutralElement != null) PathsFromNeutralElement = pathsFromNeutralElement.ToList();
        this.inSubgroup = inSubgroup;

        OnEdgeChange += CalculateEdgeCompletion;
        OnEdgeChange += () => {
            tooltipContent = new() {
                text = this.name == "1"
                    ? "The neutral element, often denoted as 1 or e, or 0 in an Abelian group."
                    : string.Join("=\n", PathsFromNeutralElement)
            };
        };

        base.Initialize(position, graphManager);
    }

    public void InitializeFromPredecessor(GroupVertex predecessor, char op, float hyperbolicScaling) {
        Initialize(
            predecessor.Position, 
            predecessor.graphManager,
            predecessor.name == "1" ? op.ToString() : predecessor.name + op,
            from path in predecessor.PathsFromNeutralElement select path + op
        );

        GroupVertex prepredecessor = predecessor.FollowEdge(RelatorDecoder.invertGenerator(op));
        if (prepredecessor != null) {
            VectorN diff = predecessor.Position - prepredecessor.Position;
            if (diff.MagnitudeSquared() > 0) diff = diff.Normalize();
            Position = predecessor.Position + hyperbolicScaling * (diff * hyperbolicScaling + VectorN.Random(predecessor.Position.Size(), 0.1f));
        }
        else {
            Position = predecessor.Position + VectorN.Random(predecessor.Position.Size(), hyperbolicScaling);
        }
        Velocity = VectorN.Zero(Position.Size());
        transform.position = VectorN.ToVector3(Position);

        DistanceToNeutralElement = predecessor.DistanceToNeutralElement + 1;
        calculateVertexMass(hyperbolicScaling);
    }

    public void Merge(GroupVertex vertex2, float hyperbolicity) {
        Position = (Position + vertex2.Position) / 2;
        PathsFromNeutralElement.AddRange(vertex2.PathsFromNeutralElement);
        distanceToNeutralElement = Math.Min(vertex2.distanceToNeutralElement, distanceToNeutralElement);
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

    public TooltipContent GetTooltip() {
        return tooltipContent;
    }

    //public Kamera GetKamera() {
    //    return graphManager.kamera;
    //}
}
