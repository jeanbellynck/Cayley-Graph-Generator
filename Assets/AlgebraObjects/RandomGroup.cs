
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = System.Random;
using QuikGraph;
using TaggedGraph = QuikGraph.UndirectedGraph<string, QuikGraph.TaggedEdge<string, EdgeData>>;
using ShGraph = SharpGraph.Graph;
using GeneratorSet = System.Collections.Generic.HashSet<string>;
public class EdgeData {
    public string generator = string.Empty;
    public string? start;
    public override string ToString() => $"-{generator}->{start}";
}

public static class RandomGroups {

    static void AssignRandomOrientation(TaggedGraph graph, Random random) {
        foreach (var edge in graph.Edges) {
            edge.Tag!.start = random.Next(2) == 0 ? edge.Source : edge.Target;
        }
    }

    static string InvertGenerator(string generator) 
    => string.Concat(generator.Reverse().Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)));
    static List<string> GetGeneratorNames(int k)
    {
        if (k > 26) k = 26;
            //throw new ArgumentException("k must be <= 26");
        return (
            from i in Enumerable.Range(0, k)
            select ((char)('a' + i)).ToString()
        ).ToList();
    }

    delegate void LogFunction(string s);
    static readonly LogFunction Warn = UnityEngine.Debug.LogWarning;
    static readonly LogFunction Log = UnityEngine.Debug.Log;
    static bool AssignRandomGenerators(TaggedGraph graph, IEnumerable<string> generatorNames, Random random, int retries = 10) {
        GeneratorSet generators = new(generatorNames);
        GeneratorSet gensAndInverses = new(generatorNames);
        gensAndInverses.UnionWith(generators.Select(InvertGenerator));
        bool successful = true;

        for (int i = 0; i < retries; i++) {
            bool broken = false;
            foreach (var edge in graph.Edges) 
                if (edge.Tag != null)
                    edge.Tag.generator = string.Empty;

            foreach (string vertex in graph.Vertices) {
                GeneratorSet assignedGenerators = AssignedOutgoingGeneratorsAtVertex(vertex);

                foreach (var edge in graph.AdjacentEdges(vertex)) {
                    if (!string.IsNullOrWhiteSpace(edge.Tag?.generator))
                        continue;

                    GeneratorSet assignedIngoingGeneratorsAtOtherEnd = AssignedOutgoingGeneratorsAtVertex(edge.Target);
                    GeneratorSet blockedGenerators = new (assignedIngoingGeneratorsAtOtherEnd.Select(InvertGenerator) );
                    blockedGenerators.UnionWith(assignedGenerators);

                    
                    if (blockedGenerators.Count == gensAndInverses.Count) {
                        if (i < retries - 1) {
                            Log($"Warning: All generators are blocked at edge {edge}");
                            broken = true;
                            break;
                        }

                        Log($"Ignoring generator block at edge {edge}");
                        blockedGenerators.Remove(blockedGenerators.First());
                        successful = false;
                    }

                    string randomGenerator = RandomGenerator(blockedGenerators);

                    assignedGenerators.Add(randomGenerator);
                    edge.Tag = new EdgeData { generator = randomGenerator, start = vertex };
                }
                if (broken) break; // unsuccessful at this vertex
            }
            if (!broken) break; // successful at all vertices
        }
        if (!successful)
            Warn($"Warning: During assignment of random generators to the {graph.EdgeCount} edges of the graph, I could not assign all {generators.Count} generators uniquely to adjacent edges at all {graph.VertexCount} vertices in {retries-1} tries, and had to ignore that in the last try");

        return successful;

        GeneratorSet AssignedOutgoingGeneratorsAtVertex(string vertex) => new ( 
                from e in graph.AdjacentEdges(vertex)
                where !string.IsNullOrWhiteSpace(e.Tag?.generator)
                select e.Tag!.start == vertex ? e.Tag.generator : InvertGenerator(e.Tag.generator)
            );

        string RandomGenerator(GeneratorSet blockedGenerators) {
            var okGenerators = new GeneratorSet(gensAndInverses);
            okGenerators.ExceptWith(blockedGenerators);
            return okGenerators.ElementAt(random.Next(0, okGenerators.Count));
        }
    }

    public static string[] RelatorsFromGraph(TaggedGraph dirGraph) {
        return (
            from nodeCycle in GetSimpleCycles(dirGraph)
            let l = nodeCycle.Count
            where l != 2
            let edgeDataCycle = (
                from i in Enumerable.Range(0, nodeCycle.Count)
                select (GetEdge(nodeCycle[i], nodeCycle[(i + 1) % nodeCycle.Count]), i)
            )
            let generatorCycle = (
                from tuple in edgeDataCycle
                let edgeData = tuple.Item1
                let i = tuple.i
                select
                    edgeData.start == nodeCycle[i]
                    ? edgeData.generator
                    : InvertGenerator(edgeData.generator)
                )
            select string.Join("", generatorCycle)
        ).ToArray();

        EdgeData GetEdge(string start, string end)
        {
            dirGraph.TryGetEdge(start, end, out var edge);
            UnityEngine.Debug.Assert(edge != null, nameof(edge) + " != null");
            return edge?.Tag ?? new EdgeData();
        }
    }
    
    public static (string[] generatorNames, TaggedGraph graph) RandomGraphWithEdgeWords(int vertexCount, int edgeCount, double proportionOfGenerators)
    {
        var random = new Random(); 
        TaggedGraph graph = new();

        var vertexNumber = 0;
        string VertexFactory() => (vertexNumber++).ToString();

        TaggedEdge<string, EdgeData> EdgeFactory(string source, string target) => 
            new(source, target, new());

        QuikGraph.Algorithms.RandomGraphFactory.Create(
            graph: graph,
            vertexFactory: VertexFactory,
            edgeFactory: EdgeFactory,
            vertexCount: vertexCount,
            edgeCount: edgeCount,
            rng: random,
            selfEdges: false
        );

        int maxDegree = graph.Vertices.Max(v => graph.AdjacentDegree(v));

        AssignRandomOrientation(graph, random);

        var preferredGeneratorCount = (int)Math.Ceiling(maxDegree * proportionOfGenerators);
        var generatorNames = new List<string>(preferredGeneratorCount);
        for (int k = preferredGeneratorCount; k < preferredGeneratorCount + 3; k++) {
            generatorNames = GetGeneratorNames(k);
            if (AssignRandomGenerators(graph, generatorNames, random))
                break;
            Log("Retrying with more generators");
        }

        foreach (var generator in generatorNames.ToArray())
            if (generatorNames.Count > preferredGeneratorCount && (
                    from edge in graph.Edges
                    select edge.Tag?.generator == generator
                ).Any())
                generatorNames.Remove(generator);

        return (generatorNames.ToArray(), graph);
    }


    public static (string[], string[]) RandomPresentation(int vertexCount, int edgeCount, double proportionOfGenerators)
    {
        var (generatorNames, graph) = RandomGraphWithEdgeWords(vertexCount, edgeCount, proportionOfGenerators);
        var relators = RelatorsFromGraph(graph);
        return (generatorNames, relators);
    }

    static List<List<string>> GetSimpleCycles(TaggedGraph taggedGraph)
    {
        var sharpGraph = Converter.TaggedToSharpGraph(taggedGraph);
        return (
            from nodeList in sharpGraph.FindSimpleCycles()
            select (
                from node in nodeList
                select node.GetLabel()
            ).ToList()
        ).ToList();
    }
}

internal class Logfunction {
}


public static class Converter {
    public static ShGraph TaggedToSharpGraph(TaggedGraph quickGraph) =>
        new ((
            from edge in quickGraph.Edges
            select new SharpGraph.Edge(edge.Source, edge.Target)
        ).ToList());

    public static TaggedGraph SharpToTaggedGraph(ShGraph sharpGraph) {
        var taggedGraph = new TaggedGraph();
        foreach (var edge in sharpGraph.GetEdges()) {
            string sourceLabel = edge.From().GetLabel();
            string targetLabel = edge.To().GetLabel();
            taggedGraph.AddVerticesAndEdge(
                new(sourceLabel, 
                    targetLabel, 
                    new(){generator = "?", start = sourceLabel}
                ));
        }
        return taggedGraph;
    }


    public static string ToStringF(this TaggedGraph graph) {
        StringBuilder sb = new();
        sb.Append("Graph: \n");
        foreach(var edge in graph.Edges) {
            bool reverse = edge.Tag?.start == edge.Target;
            sb.Append(edge.Source);
            sb.Append(reverse ? " <-" : " --");
            sb.Append(edge.Tag?.generator ?? "?");
            sb.Append(reverse ? "-- " : "-> ");
            sb.Append(edge.Target);
            sb.Append("\n");
        }

        sb.Append("End");
        return sb.ToString();
    }
}
