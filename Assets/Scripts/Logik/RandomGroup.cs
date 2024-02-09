
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Random = System.Random;
using QuikGraph;
using TaggedGraph = QuikGraph.UndirectedGraph<string, QuikGraph.TaggedEdge<string, EdgeData>>;
using SharpGraph;
using ShGraph = SharpGraph.Graph;
using GeneratorSet = System.Collections.Generic.HashSet<string>;
public class EdgeData {
    public string generator = string.Empty;
    public string? start;
}

public static class RandomGroups {

    static void AssignRandomOrientation(TaggedGraph graph, Random random) {
        foreach (var edge in graph.Edges) {
            edge.Tag!.start = random.Next(2) == 0 ? edge.Source : edge.Target;
        }
    }

    static string InvertGenerator(string generator) 
    => string.Concat(generator.Reverse().Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)));
    static string[] GetGeneratorNames(int k)
    {
        if (k>26)
            //throw new ArgumentException("k must be <= 26");
            k = 26;
        return Enumerable.Range(0, k).Select(i =>
            ((char)('a' + i)).ToString()
        ).ToArray();
    }

    static void AssignRandomGenerators(TaggedGraph graph, string[] generatorNames, Random random) {
        GeneratorSet generators = new(generatorNames);
        GeneratorSet gensAndInverses = new(generators);
        gensAndInverses.UnionWith(generators.Select(InvertGenerator));

        string RandomGenerator(GeneratorSet blockedGenerators) {
            var okGenerators = new GeneratorSet(gensAndInverses);
            okGenerators.ExceptWith(blockedGenerators);
            return okGenerators.ElementAt(random.Next(0, okGenerators.Count));
        }

        GeneratorSet AssignedOutgoingGeneratorsAtVertex(string vertex) {
            return new GeneratorSet(graph.AdjacentEdges(vertex)
                .Where(e => string.IsNullOrWhiteSpace(e.Tag?.generator))
                .Select(e => e.Tag!.start == vertex ? e.Tag.generator : InvertGenerator(e.Tag.generator)));
        }

        const int retries = 10;
        for (int i = 0; i < retries; i++) {
            bool broken = false;
            foreach (string vertex in graph.Vertices) {
                GeneratorSet assignedGenerators = AssignedOutgoingGeneratorsAtVertex(vertex);

                foreach (var edge in graph.AdjacentEdges(vertex)) {
                    if (!string.IsNullOrWhiteSpace(edge.Tag?.generator))
                        continue;

                    GeneratorSet assignedGeneratorsAtTarget = new GeneratorSet(
                        AssignedOutgoingGeneratorsAtVertex(edge.Target).Select(InvertGenerator));
                    if (i < retries - 1)
                        assignedGenerators.UnionWith(assignedGeneratorsAtTarget);

                    if (assignedGenerators.Count == gensAndInverses.Count)
                    {
                        Console.WriteLine("Warning: All generators are blocked at edge " + edge.Source + " - " +
                                          edge.Target);
                        broken = true;
                        break;
                    }

                    string randomGenerator = RandomGenerator(assignedGenerators);

                    assignedGenerators.Add(randomGenerator);
                    edge.Tag = new EdgeData { generator = randomGenerator, start = vertex };
                }
                if (broken)
                    break;
            }
            if (broken) {
                if (i == retries - 2) 
                    Console.WriteLine("Warning: Could not assign all generators, will now ignore remote blocked generators");
                if (i == retries - 1)
                    Console.WriteLine("Warning: Could not assign all generators");
                continue;
            }
            break;
        }
    }

    public static string[] RelatorsFromGraph(TaggedGraph dirGraph) {
        var loops = 
            from nodeCycle in GetSimpleCycles(dirGraph)
            let l = nodeCycle.Count
            where l != 2
            let edgeDataCycle = (
                from i in Enumerable.Range(0, nodeCycle.Count)
                select (getEdge(nodeCycle[i], nodeCycle[(i + 1) % nodeCycle.Count]), i)
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
            ;
        return loops.ToArray();

        EdgeData getEdge(string start, string end)
        {
            dirGraph.TryGetEdge(start, end, out var edge);
            Debug.Assert(edge != null, nameof(edge) + " != null");
            return edge.Tag ?? new EdgeData();
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

        int k = (int)Math.Ceiling(maxDegree * proportionOfGenerators);
        string[] generatorNames = GetGeneratorNames(k);
        AssignRandomGenerators(graph, generatorNames, random);

        return (generatorNames, graph);
    }


    public static (string[], string[]) RandomPresentation(int vertexCount, int edgeCount, double proportionOfGenerators)
    {
        var (generatorNames, graph) = RandomGraphWithEdgeWords(vertexCount, edgeCount, proportionOfGenerators);
        var relators = RelatorsFromGraph(graph);
        return (generatorNames, relators);
    }

    static IEnumerable<List<string>> GetSimpleCycles(TaggedGraph taggedGraph)
    {
        var sharpGraph = Converter.ConvertToSharpGraph(taggedGraph);
        return (
            from nodeList in sharpGraph.FindSimpleCycles()
            select (
                from node in nodeList
                select node.GetLabel()
            ).ToList()
        );
    }
}


public static class Converter {
    public static ShGraph ConvertToSharpGraph(TaggedGraph quickGraph) =>
        new ((
            from edge in quickGraph.Edges
            select new SharpGraph.Edge(edge.Source, edge.Target)
        ).ToList());
}
