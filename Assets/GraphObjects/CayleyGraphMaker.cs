using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using MathNet.Numerics.RootFinding;

public class CayleyGraphMaker : MonoBehaviour {
    [SerializeField] GraphVisualizer graphVisualizer;
    [SerializeField] LabelledGraphManager graphManager;

    [SerializeField] MeshManager meshManager;
    [SerializeField] Physik physik; // I wonder whether the reference to Physics is necessary? 

    protected char[] generators;// = new char[]{'a', 'b', 'c'};
    protected char[] operators; // Like generators but with upper and lower case letters, unless in a semigroup!
    protected string[] relators;// = new string[]{"abAB"};
    HashSet<string> relatorVariants;


    [SerializeField] float hyperbolicity = 1; // This might be better off in GraphVisualizer too.
    readonly Dictionary<(char, char), float> hyperbolicityMatrix = new();


    [SerializeField] int vertexNumber; // Describes the number of vertices the graph should have.
    [SerializeField] float drawingSpeed = 1; // Describes the speed at which new vertices should be drawn in vertices per second 

    [SerializeField] int numberOfMeshesPerFrame = 10;
    private Func<int, int, double> comparerFunction;


    // Contains the references to all vertices on the border of the graph, sorted by distance to the center.
    SortedDictionary<(double, int), GroupVertex> borderVertices = new();
    [field: SerializeField] public GroupVertex NeutralElement { get; private set; }

    // Contains the references of all vertices which need to be checked for relator application.
    readonly HashSet<GroupVertex> relatorCandidates = new();
    readonly HashSet<GroupVertex> edgeMergeCandidates = new();


    [SerializeField] UnityEvent<bool> onStateChanged = new();
    [SerializeField] UnityEvent<string> onWantedVertexNumberChanged = new(); // this should be int, but I'm lazy, and only need this to set text
    [SerializeField] UnityEvent<string> onCurrentVertexNumberChanged = new();// this should be int, but I'm lazy, and only need this to set text
    public event Action OnStopVisualization;

    [SerializeField] GroupMode groupMode = GroupMode.Group;


    float reorderBorderVerticesTime;
    float nextBorderVertexPreferenceScale = 10;
    int lastBorderVertexReorderEdgeCount;
    [SerializeField] int reorderBorderVerticesEdgeCountInterval = 100;



    bool _running;
    public bool Running {
        get => _running;
        protected set { _running = value; onStateChanged?.Invoke(value); }
    }

    public void StartVisualization() {

        //int simulationDimensionality = 2*generators.Length + 1;

        if (graphManager == null) return;

        if (groupMode == GroupMode.SemiGroup)
            Debug.LogWarning("Semigroups without neutral element aren't implemented yet!");

        NeutralElement = CreateVertex(null, default);
        NeutralElement.semiGroup = groupMode != GroupMode.Group;
        // todo: to Initialize (it is currently just a weird way of initializing the neutral element)
        NeutralElement.SetRadius(1.6f * NeutralElement.Radius);
        NeutralElement.baseImportance = 2; // only needed for the neutral element in a monoid, bc. EdgeCompletion often will only be 0.5
        NeutralElement.Center();


        ContinueVisualization();
    }

    public void Initialize(IEnumerable<char> generators, string[] relators, Physik physik, GraphVisualizer graphVisualizer, GroupMode groupMode) {
        this.generators = generators.Select(char.ToLower).ToArray();
        this.relators = relators;
        this.physik = physik;
        this.graphVisualizer = graphVisualizer;
        this.groupMode = groupMode;
        graphManager = graphVisualizer.graphManager;
        this.relatorVariants = relators.SelectMany(GenerateRelatorVariants).ToHashSet();

        if (groupMode == GroupMode.Group) {
            this.operators = this.generators.Concat(this.generators.Select(char.ToUpper)).ToArray();
        }
        else
            this.operators = this.generators;

        Reset();
    }

    public void ContinueVisualization() {
        if (graphManager == null)
            return;
        if (!Running)
            StartCoroutine(CreateNewElementsAndApplyRelators());
        physik.Run();
    }


    public void Reset() {
        AbortVisualization();

        borderVertices.Clear();
        relatorCandidates.Clear();
        edgeMergeCandidates.Clear();
        graphManager?.ResetGraph();
        meshManager?.ResetMeshes();
    }

    public void StopVisualization() {
        Running = false;
        if (graphManager == null)
            return;
        DrawMeshes();
        physik.BeginShutDown();
        OnStopVisualization?.Invoke();
    }

    public void AbortVisualization() {
        Running = false;
        StopAllCoroutines();
    }


    IEnumerator CreateNewElementsAndApplyRelators() {
        bool firstIteration = true;
        Running = true;

        while (Running) {
            var currentCount = graphManager.GetVertices().Count;
            onCurrentVertexNumberChanged?.Invoke(currentCount.ToString());
            if (vertexNumber <= currentCount)
                break;

            // Speed is proportional to the number of vertices on the border. This makes knotting less likely
            float waitTime = 1 / (drawingSpeed * Mathf.Max(1, borderVertices.Count));
            if (firstIteration)
                firstIteration = false;
            else
                yield return new WaitForSeconds(waitTime);

            GroupVertex borderVertex = GetNextBorderVertex();
            if (borderVertex == null) {
                print("No vertices remaining. Stopping.");
                break;
            }

            foreach (char gen in operators) {
                if (borderVertex.FollowEdge(gen) != null) continue;
                // Vertex creation!
                relatorCandidates.Add(CreateVertex(borderVertex, gen));
            }
            MergeAll();
        }
        StopVisualization();
    }


    /**
    * Creates a new vertex and adds it to the graph. Also creates an edge between the new vertex and the predecessor.
    */
    GroupVertex CreateVertex(GroupVertex predecessor, char op) {
        GroupVertex newVertex = graphVisualizer.CreateVertex(predecessor, op, hyperbolicity);
        AddBorderVertex(newVertex);
        // Vertex is not the neutral element and an edge need to be created
        if (predecessor != null)
            CreateEdge(predecessor, newVertex, op);
        return newVertex;
    }


    GroupEdge CreateEdge(GroupVertex startVertex, GroupVertex endVertex, char op) {
        if (GroupVertex.IsReverseLabel(op)) {
            (startVertex, endVertex) = (endVertex, startVertex);
            op = GroupVertex.ReverseLabel(op);
        }
        return graphVisualizer.CreateEdge(startVertex, endVertex, op, hyperbolicity);
    }


    void AddBorderVertex(GroupVertex vertex) {
        borderVertices[(comparerFunction(vertex.DistanceToNeutralElement, vertex.DistanceToSubgroup), vertex.Id)] = vertex;
    }

    public GroupVertex GetNextBorderVertex() {
        while (borderVertices.Count > 0)
            if (borderVertices.Pop() is GroupVertex nextBorderVertex && nextBorderVertex != null) 
                // Ah, Unity sometimes keeps the C# objects longer than the "actual" C++ objects, thus "is not null" and null propagation are not good for Unity objects, "== null" works.
                return nextBorderVertex;
        return null;
    }


    /**
    * Applies the relators to all groupElements in the mergeCandidates list.
    */
    void MergeAll() {
        while (true) {
            // If two edges of the same generator lead to the different vertices, they need to be merged as fast as possible. Otherwise, following generators is yucky.
            var edgeMergeCandidatesCount = edgeMergeCandidates.Count;
            var mergeCandidate = edgeMergeCandidates.Pop();
            if (mergeCandidate != null) {
                MergeEdges(mergeCandidate);
                continue;
            }

            mergeCandidate = relatorCandidates.Pop();
            if (mergeCandidate != null) {
                MergeByRelators(mergeCandidate);
                continue;
            }
            break;
        }
        onCurrentVertexNumberChanged.Invoke(graphManager.GetVertices().Count.ToString());
    }

    void MergeEdges(GroupVertex vertex) {
        foreach (char op in operators) {
            while (true) {
                // this way we don't try to merge on edges that were actually destroyed or iterate over a list that gets modified
                var edges = vertex.GetEdges(op).Take(2).ToArray();

                if (edges.Length < 2)
                    break;

                MergeVertices(edges[0].GetOpposite(vertex), edges[1].GetOpposite(vertex));
            }
            //edgeMergeCandidates.Add(vertex); // After an edge merge a vertex might be merged with its neighbor meaning its edges can be merged again.

        }
    }


    /**
    * Follows the relators from the startingElement until it finds  and tries to merge the resulting element with the startingElement.
    */
    void MergeByRelators(GroupVertex startingElement) {
        // Der Code ist ein wenig unoptimiert. Nach dem Anwenden eines Relators versucht er wieder alle anzuwenden. 
        // Dadurch steigt die Komplexität im Worst-Case zu n^2 falls alle Relatoren genutzt werden. (Was natürlich unwahrscheinlich ist)
        foreach (string relatorVariant in relatorVariants) {

            var otherElement = startingElement.FollowGeneratorPath(relatorVariant);
            if (otherElement == null || otherElement.Equals(startingElement))
                continue;
            MergeVertices(startingElement, otherElement);
            return;
        }
    }

    /**
     * Generates all possible variants of the given relator.
     * Variants are generated by rotating or inverting the relator string.
     **/
    List<string> GenerateRelatorVariants(string relator) {
        if (groupMode != GroupMode.Group) {
            // we assume that all relators have the form v w^-1 or w^-1 v for two positive words v and w (they come in this form if they were written as v=w or [x,y])
            string v, wInv;
            if (char.IsUpper(relator[^1])) {
                int i = relator.Length;
                while (i > 0 && char.IsUpper(relator[i - 1]))
                    i--;

                v = relator[..i];
                wInv = relator[i..];
            }
            else {
                int i = 0;
                while (i < relator.Length && char.IsUpper(relator[i]))
                    i++;
                v = relator[i..];
                wInv = relator[..i];
                // actually equivalent: return new() { relator, RelatorDecoder.InvertSymbol(relator) }
            }
            return new() { wInv + v, RelatorDecoder.InvertSymbol(v) + RelatorDecoder.InvertSymbol(wInv) };
            // the equivalence relation in the monoid presentation is such that we only need to check these two variants of the relator; going backwards, then forwards
        }

        string relatorInverse = RelatorDecoder.InvertSymbol(relator);
        List<string> variants = new() { relator, relatorInverse };
        for (int i = 1; i < relator.Length; i++) {
            variants.Add(relator[i..] + relator[..i]);
            variants.Add(relatorInverse[i..] + relatorInverse[..i]);
        }
        return variants;
    }

    /**
    * Merges vertex2 and vertex1. The groupElement with the shorter name is deleted and all edges are redirected to the other groupElement.
    */
    void MergeVertices(GroupVertex vertex1, GroupVertex vertex2) {
        if (vertex1.Equals(vertex2)) return;
        // The vertex that was further from the identity will be deleted. (We don't want to delete the neutral element.)
        if (vertex2.DistanceToNeutralElement < vertex1.DistanceToNeutralElement)
            (vertex1, vertex2) = (vertex2, vertex1);

        vertex1.Merge(vertex2, hyperbolicity);

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach (char op in generators) {
            foreach (GroupEdge edge in vertex2.GetIncomingEdges(op).Concat(vertex2.GetOutgoingEdges(op)).Cast<GroupEdge>()) {
                // todo: actually, we should be able to just reuse the existing edges!!!
                var startVertex = edge.StartPoint == vertex2 ? vertex1 : (GroupVertex)edge.StartPoint;
                var endVertex = edge.EndPoint == vertex2 ? vertex1 : (GroupVertex)edge.EndPoint;
                if (startVertex.GetOutgoingEdges(op).All(edge1 => !edge1.EndPoint.Equals(endVertex))) // don't create duplicate edges, because then they will be merged again!
                    CreateEdge(startVertex, endVertex, op);
            }
        }

        // Delete vertex2
        graphManager.RemoveVertex(vertex2);
        vertex2.Destroy(); // also destroys all edges

        // Neuen Knoten nochmal prüfen
        edgeMergeCandidates.Add(vertex1);
        relatorCandidates.Add(vertex1);
    }


    /**
     * This is a method that would better fit into a "group element" class.
     * Taking in paths to identity and using the hyperbolicityMatrix it calculates the scaling of a generator. This give the desired length of an edge.
     **/
    float CalculateScalingForGenerator(char generator, string path) {
        float scaling = 1;
        foreach (char op in path) {
            scaling *= hyperbolicityMatrix[(op, generator)];
        }
        return scaling;
    }


    void DrawMeshes() {
        meshManager.UpdateTypes(relators);
        StartCoroutine(DrawMeshesCoroutine());
        return;

        IEnumerator DrawMeshesCoroutine() {
            var drawnMeshes = 0;
            var vertexEnumerator = graphManager.GetVertices().GetEnumerator();
            while (true) {
                // this is foreach(vertex in graphManager.GetVertices())
                // where I catch the InvalidOperationException that is thrown when the collection is modified
                try {
                    if (!vertexEnumerator.MoveNext()) break;
                }
                catch (InvalidOperationException e) {
                    vertexEnumerator.Dispose();
                    if (!e.Message.StartsWith("Collection was modified"))
                        throw;
                    vertexEnumerator = graphManager.GetVertices().GetEnumerator();
                    continue;
                    // restart 
                }

                var vertex = vertexEnumerator.Current;
                //if (vertex == null) continue;

                foreach (var relator in relators) {

                    if (vertex is not GroupVertex groupVertex) continue; // shouldn't happen

                    var path = groupVertex.GeneratorPath(relator);
                    // this is a list of vertices, and is shorter than relator.Length + 1 only if the path exited the graph

                    if (path.Count > relator.Length &&
                        meshManager.AddMesh(path.Take(relator.Length), parent: transform, type: relator) &&
                        // doesn't draw multiply, so even if this is called after continuing, it doesn't matter that old vertices get called again here.
                        ++drawnMeshes % numberOfMeshesPerFrame == 0
                       )
                        yield return null;
                }

            }
        }

    }

    public void SetVertexNumber(int v) {
        if (v > vertexNumber)
            ContinueVisualization();
        vertexNumber = v;
        onWantedVertexNumberChanged?.Invoke(vertexNumber.ToString());
    }

    public void SetHyperbolicity(float hyperbolicity) {
        this.hyperbolicity = hyperbolicity;
        // Set all values of the hyperbolicity matrix to the new hyperbolicity
        float[,] matrix = new float[generators.Length, generators.Length];
        for (int i = 0; i < generators.Length; i++) {
            for (int j = 0; j < generators.Length; j++) {
                matrix[i, j] = hyperbolicity;
            }
        }
        SetHyperbolicityMatrix(matrix);
    }

    public void SetHyperbolicityMatrix(float[,] matrix) {
        for (int i = 0; i < matrix.GetLength(0); i++) {
            for (int j = 0; j < matrix.GetLength(1); j++) {
                var matrixValue = matrix[i, j];
                if (matrixValue < 0) {
                    // For negative Values the hyperbolic scaling is done in one direction
                    hyperbolicityMatrix[(generators[i], generators[j])] = -matrixValue;
                    hyperbolicityMatrix[(char.ToUpper(generators[i]), generators[j])] = -1 / matrixValue;
                }
                else {
                    // For positive Values the hyperbolic scaling is done in both directions
                    hyperbolicityMatrix[(generators[i], generators[j])] = matrixValue;
                    hyperbolicityMatrix[(char.ToUpper(generators[i]), generators[j])] = matrixValue;
                }
            }
        }
        recalculateHyperbolicity();
    }

    /**
     * Recalculates the length of all edges according to the hyperbolicity.
     */
    void recalculateHyperbolicity() {
        if (graphManager == null)
            return;

        foreach (var edge in graphManager.GetEdges())
            if (edge is GroupEdge groupEdge)
                groupEdge.calculateEdgeLength(hyperbolicity);

        foreach (var vertex in graphManager.GetVertices())
            if (vertex is GroupVertex groupVertex)
                groupVertex.calculateVertexMass(hyperbolicity);
    }


    public void SetGenerators(char[] generators) {
        this.generators = generators;
    }

    // referenced from UI
    public void ToggleActiveState() {
        if (Running)
            StopVisualization();
        else {
            var currentVertexCount = graphManager == null ? 0 : graphManager.GetVertices().Count;
            onCurrentVertexNumberChanged?.Invoke(currentVertexCount.ToString());
            if (currentVertexCount >= vertexNumber) {
                vertexNumber = currentVertexCount + 50;
                onWantedVertexNumberChanged.Invoke(vertexNumber.ToString());
            }
            if (currentVertexCount == 0)
                StartVisualization();
            else
                ContinueVisualization();
        }
    }

    //referenced from UI
    public void SetGroupMode(int mode) {
        groupMode = (GroupMode)mode;
    }

    void UpdateBorderVertices() {
        comparerFunction = new VertexDistanceComparer(nextBorderVertexPreferenceScale).F;
        borderVertices = new(new Dictionary<(double, int), GroupVertex>(
                from vertex in borderVertices.Values
                where vertex != null
                select new KeyValuePair<(double, int), GroupVertex>(
                    (comparerFunction(vertex.DistanceToNeutralElement, vertex.DistanceToSubgroup), vertex.Id), vertex
                )
            ));
    }

    public void UpdateSubgroupPreference(float r) {
        reorderBorderVerticesTime = Time.time + 0.5f;
        nextBorderVertexPreferenceScale = r;
    }

    void Update() {
        if (graphManager?.EdgeCount() >= lastBorderVertexReorderEdgeCount + reorderBorderVerticesEdgeCountInterval) {
            // the ordering doesn't update when we update the values of DistanceToNeutralElement and DistanceToSubgroup. That happens when Edges are added.
            UpdateBorderVertices();
            lastBorderVertexReorderEdgeCount = graphManager.EdgeCount();
        }
        if (Time.time > reorderBorderVerticesTime) { 
            UpdateBorderVertices();
            reorderBorderVerticesTime = float.MaxValue;
        }
    }
}

public enum GroupMode { Group, Monoid, SemiGroup }
class VertexDistanceComparerComplicated {
    readonly double α;
    static readonly double γ = 1.5f;
    static readonly double ε = 0.4f;
    static readonly double oneOverε = 1/ε;
    static readonly double β = 2f;
    static readonly double twototheε = Math.Pow(2, ε);
    static readonly Dictionary<(int, int), double> cache = new();
    static readonly Dictionary<int,  double> thresholds = new();
    static readonly double accuracy = 0.5f;

    public VertexDistanceComparerComplicated(double N = 10) {
        this.α = N/(Math.Pow(γ*N+β, ε) - Math.Pow(β, ε));
    }

    static double fInv(double F, double a) => F <= a ? 0 : (Math.Pow(F + 2, ε) - twototheε) * Math.Sqrt(1 - Math.Pow(a / F, 2));
    // F should be >= a

    static double dfInv(double F, double a) => ε * Math.Pow(F + 2, ε - 1) * Math.Sqrt(1 - Math.Pow(a / F, 2)) + (Math.Pow(F + 2, ε) - twototheε) * 2 * a * a / Math.Pow(F, 3) / Math.Sqrt(1 - Math.Pow(a / F, 2));

    public double F(int a, int b) => f(a - b, b / α);
    // var (DistanceFromNeutralElement, DistanceFromSubgroup, Id) = x;
    // var (a, b) = (DistanceFromNeutralElement - DistanceFromSubgroup, DistanceFromSubgroup);
    // 

    static double f(int a, double bOverα) {
        int bCache = (int)Math.Round(bOverα * 10);

        if (cache.TryGetValue((a, bCache), out var res))
            return res;
        double approximateResult = asymptoticF(bOverα);
        if (thresholds.TryGetValue(a, out var threshold) && bOverα > threshold) 
            return approximateResult;
        if (approximateResult >= a && bOverα - fInv(approximateResult, a) < accuracy) {
            // fInv is only defined for F >= a, thus f(a,b) >= a.
            thresholds[a] = bOverα;
            return approximateResult;
        }

        var result = RobustNewtonRaphson.FindRoot(F => fInv(F, a) - bOverα, F => dfInv(F, a), a, a * a + 20, accuracy: 1E-03, maxIterations: 20, subdivision: 10);
        cache[(a, bCache)] = result;
        return result;
    }

    static double asymptoticF(double bOverα) {
        return Math.Pow(bOverα + twototheε, oneOverε) - 2; // asymptotic formula for large b/α
    }
}

class VertexDistanceComparer {
    readonly double c;
    readonly double e;

    public VertexDistanceComparer(double N = 10, double e = 1.5) {
        this.e = e;
        this.c = Math.Pow(N, 1 - e);
    }

    public double F(int DistanceFromNeutralElement, int DistanceFromSubgroup) {
        return DistanceFromNeutralElement + c * Math.Pow(DistanceFromSubgroup, e) ;
    }

}

