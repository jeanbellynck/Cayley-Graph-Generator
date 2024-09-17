using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GroupVertex : Vertex {

    public float Stress { get; private set; }

    [field: SerializeField] public int DistanceToNeutralElement { get; private set; }
    [field: SerializeField] public List<string> PathsFromNeutralElement { get; protected set; } = new();
    [field: SerializeField] public int DistanceToSubgroup { get; set; }

    float edgeCompletion;
    public override float Importance => activeHighlightTypes.Count > 0 ? 1 : MathF.Min(edgeCompletion * baseImportance, 1);


    /** not currently used **/
    public bool semiGroup;

    public void Initialize(VectorN position, GraphVisualizer graphVisualizer, string name = null, IEnumerable<string> pathsFromNeutralElement = null, bool semiGroup = false) {
        if (!string.IsNullOrEmpty(name)) this.name = name;
        if (pathsFromNeutralElement != null) PathsFromNeutralElement = pathsFromNeutralElement.Take(equivalentWordsMax).ToList();
        // in some cases, the pathsFromNeutralElement are extremely many and this slowed down the program significantly!
        this.semiGroup = semiGroup;

        OnEdgeChange += CalculateEdgeCompletion;
        OnEdgeChange += () => {
            tooltipContent = new() {
                text = this.name == "1"
                    ? "The neutral element, often denoted as 1 or e, or 0 in an Abelian group."
                    : string.Join("=\n", PathsFromNeutralElement.Take(equivalentWordsMax-1)) 
                      + (PathsFromNeutralElement.Count >= equivalentWordsMax ? "\n..." : "")
            };
        };

        base.Initialize(position, graphVisualizer);
    }

    const int equivalentWordsMax = 30;

    void CalculateEdgeCompletion()
    {
        edgeCompletion = graphVisualizer.LabelCount > 0 ? 0.8f * (
            LabeledIncomingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty()) +
            LabeledOutgoingEdges.Values.Count(edgeSet => !edgeSet.IsEmpty())
        ) / graphVisualizer.LabelCount + 0.2f : 1f;
        SetRadius();
    }


    protected override void Start() {
        base.Start();
        Update();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
        //if (activeHighlightTypes.Count > 0) return;
        //Stress = (from generator in LabeledOutgoingEdges.Keys
        //          let inEdge = GetIncomingEdges(generator).FirstOrDefault()?.Direction ?? Vector3.zero
        //          let outEdge = GetOutgoingEdges(generator).FirstOrDefault()?.Direction ?? Vector3.zero
        //          select Vector3.Angle(inEdge, outEdge) / 180
        //).DefaultIfEmpty(0).Max();
        //Mr.material.color = new Color(Stress, 0, 0, EdgeCompletion);
        //todo: Implement stress properly!(also make this not clash with highlight)
        //todo: find a material / shader that works like diffuse fast but with transparency

    }

    public override void SetRadius(float radius) {
        base.SetRadius(radius);
        transform.localScale = 2 * Importance * radius * Vector3.one;
    }

    public Dictionary<char, List<GroupEdge>> GetEdges() {
        Dictionary<char, List<GroupEdge>> edges = LabeledOutgoingEdges.Keys.ToDictionary(op => op, op => GetOutgoingEdges(op).Cast<GroupEdge>().ToList());
        //if (semiGroup) return edges;
        foreach (char op in LabeledIncomingEdges.Keys) {
            var reverseLabel = ReverseLabel(op);
            if (!edges.ContainsKey(reverseLabel))
                edges.Add(reverseLabel, GetIncomingEdges(op).Cast<GroupEdge>().ToList());
            else
                edges[reverseLabel].AddRange(GetIncomingEdges(op).Cast<GroupEdge>());
        }
        return edges;
    }

    public static char ReverseLabel(char label) => RelatorDecoder.InvertGenerator(label);
    public static bool IsReverseLabel(char label) => char.IsUpper(label);

    public IEnumerable<GroupEdge> GetEdges(char label) {
        //if (semiGroup) return GetOutgoingEdges(label).Cast<GroupEdge>().ToList();
        // in semigroups, this will only be called for reverse labels when backwards-following edges
        return (IsReverseLabel(label) ? GetIncomingEdges(ReverseLabel(label)) : GetOutgoingEdges(label)
            ).Cast<GroupEdge>();
    }
    
    public GroupVertex FollowEdge(char op) {
        return GetEdges(op).FirstOrDefault()?.GetOpposite(this);
    }

    /**
     * Takes in a vertex and a string and follows the path as given by the generator string.
     * Returns the vertex at the end of the path.
     * Returns null if the path is not valid or leaves the ambient graph.
     **/
    public GroupVertex FollowGeneratorPath(string word) {
        GroupVertex currentVertex = this;
        foreach (char op in word) {
            currentVertex = currentVertex.FollowEdge(op);
            if (currentVertex == null)
                return null;
        }
        return currentVertex;
    }

    public List<GroupVertex> GeneratorPath(string word) {
        List<GroupVertex> path = new(capacity: word.Length + 1) {this};
        GroupVertex currentVertex = this;
        foreach (var op in word) {
            currentVertex = currentVertex.FollowEdge(op);
            if (currentVertex == null)
                return path;
            path.Add(currentVertex);
        }
        return path;
    
    }


    public void InitializeFromPredecessor(GroupVertex predecessor, char op, float hyperbolicScaling) {
        Initialize(
            predecessor.Position, 
            predecessor.graphVisualizer,
            predecessor.name == "1" ? op.ToString() : predecessor.name + op,
            from path in predecessor.PathsFromNeutralElement select path + op,
            predecessor.semiGroup
        );

        GroupVertex prepredecessor = predecessor.FollowEdge(RelatorDecoder.InvertGenerator(op));
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
        DistanceToSubgroup = predecessor.DistanceToSubgroup + 1;
        calculateVertexMass(hyperbolicScaling);
    }

    public void Merge(GroupVertex vertex2, float hyperbolicity) {
        Position = (Position + vertex2.Position) / 2;
        vertex2.centerPointer.transform = centerPointer.transform; // Some Kamera might reference the old (redundant) Vertex as its center. We now point it to the kept equivalent vertex.
        DistanceToNeutralElement = Math.Min(vertex2.DistanceToNeutralElement, DistanceToNeutralElement);
        calculateVertexMass(hyperbolicity);

        // the following is just to merge the pathsFromNeutralElement without saving too many (we assume PathsFromNeutralElement is sorted by length)
        var oldPathCount = PathsFromNeutralElement.Count;
        var newPathCount = vertex2.PathsFromNeutralElement.Count;
        int goalOldPathCount = equivalentWordsMax / 2;
        int goalNewPathCount = goalOldPathCount;
        if (oldPathCount < equivalentWordsMax / 2) {
            goalNewPathCount = equivalentWordsMax - oldPathCount;
            goalOldPathCount = oldPathCount;
        }
        else if (newPathCount < equivalentWordsMax / 2) {
            goalOldPathCount = Math.Min(oldPathCount, equivalentWordsMax - newPathCount);
            goalNewPathCount = newPathCount;
        }

        PathsFromNeutralElement.RemoveRange(goalOldPathCount, oldPathCount - goalOldPathCount);
        PathsFromNeutralElement.AddRange(vertex2.PathsFromNeutralElement.Take(goalNewPathCount));
        // this is the only place where we add paths to the vertex, so we only need to sort here
        PathsFromNeutralElement.Sort((s, t) => s.Length - t.Length);
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


    public void HighlightPathsFromIdentity(bool removeHighlight) {
        var primaryPathsToIdentity =
            from path in PathsFromNeutralElement.Take(1) 
            select RelatorDecoder.InvertSymbol(path);
        var secondaryPathsToNeutralElement = 
            from path in PathsFromNeutralElement.Skip(1).Take(4)
            select RelatorDecoder.InvertSymbol(path);

        graphVisualizer.CancelActions(removeHighlight ? "#" + name : "~" + name);
        // todo: bad practice (the # ~ are defined also in the overload for Highlight)

        Highlight(HighlightType.PrimaryPath, FollowPaths(primaryPathsToIdentity), removeHighlight, true);
        Highlight(HighlightType.Path, FollowPaths(secondaryPathsToNeutralElement), removeHighlight, true);
        return;

        Func<string, (IEnumerable<char>, IEnumerable<char>)> FollowPaths(IEnumerable<string> pathsToFollow) {
            pathsToFollow = pathsToFollow.ToArray();
            return path => {
                var nextOperations = (
                    from pathToNeutralElement in pathsToFollow
                    where pathToNeutralElement.Length > path.Length && pathToNeutralElement.StartsWith(path)
                    select pathToNeutralElement[path.Length]
                ).ToArray();
                return (
                    from op in nextOperations
                    where !IsReverseLabel(op)
                    select op,
                    // follow the outgoing edges for normal labels
                    from op in nextOperations
                    where IsReverseLabel(op)
                    select ReverseLabel(op)
                    // follow the ingoing edges for reverse labels (we traverse the pathToFollow)
                );
            };
        }
    }

    public override void OnHover(Kamera activeKamera) {
        base.OnHover(activeKamera);
        //HighlightPathsFromIdentity(false);
    }

    public override void OnHoverEnd() {
        base.OnHoverEnd();
        //HighlightPathsFromIdentity(true);
    }

    public override void OnClick(Kamera activeKamera) {
        base.OnClick(activeKamera);
        graphVisualizer.SelectedVertex = this;
    }
}
