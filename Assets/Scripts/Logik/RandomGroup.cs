using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuikGraph;
using Unity.VisualScripting;
using UnityEditor;
using Random = System.Random;
using Graph = QuikGraph.UndirectedGraph<string, QuikGraph.TaggedEdge<string, EdgeData>>;
using GeneratorSet = System.Collections.Generic.HashSet<string>;
public class EdgeData {
    public string Generator { get; set; }

    public string Start { get; set; }
}

public class RandomGroups {

    static void AssignRandomOrientation(Graph graph, Random random) {
        foreach (var edge in graph.Edges) {
            edge.Tag.Start = random.Next(2) == 0 ? edge.Source : edge.Target;
        }
    }

    static string InvertGenerator(string generator) 
    => string.Concat(generator.Reverse().Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)));
    static string[] GetGeneratorNames(int k) => 
        Enumerable.Range(0, k).Select(i => 
                ((char)('a' + i)).ToString()
            ).ToArray();

    static void AssignRandomGenerators(Graph graph, string[] generatorNames, Random random) {
        GeneratorSet generators = new(generatorNames);
        GeneratorSet gensAndInverses = new(generators);
        generators.UnionWith(generators.Select(InvertGenerator));

        string RandomGenerator(GeneratorSet blockedGenerators) {
            var okGenerators = new GeneratorSet(gensAndInverses);
            okGenerators.ExceptWith(blockedGenerators);
            return okGenerators.ElementAt(random.Next(0, okGenerators.Count));
        }

        GeneratorSet AssignedOutgoingGeneratorsAtVertex(string vertex) {
            return new GeneratorSet(graph.AdjacentEdges(vertex)
                .Where(e => e.Tag is { Generator: not null })
                .Select(e => e.Tag.Start == vertex ? e.Tag.Generator : InvertGenerator(e.Tag.Generator)));
        }

        const int retries = 10;
        for (int i = 0; i < retries; i++) {
            bool broken = false;
            foreach (string vertex in graph.Vertices) {
                GeneratorSet assignedGenerators = AssignedOutgoingGeneratorsAtVertex(vertex);

                foreach (var edge in graph.AdjacentEdges(vertex)) {
                    if (edge.Tag is { Generator: not null })
                        continue;

                    GeneratorSet assignedGeneratorsAtTarget = new GeneratorSet(
                        AssignedOutgoingGeneratorsAtVertex(edge.Target).Select(InvertGenerator));
                    if (i < retries - 1)
                        assignedGenerators.UnionWith(assignedGeneratorsAtTarget);

                    if (assignedGenerators.Count == gensAndInverses.Count) {
                        Console.WriteLine("Warning: All generators are blocked at edge " + edge.Source + " - " + edge.Target);
                        broken = true;
                        break;
                    }

                    string randomGenerator = RandomGenerator(assignedGenerators);

                    assignedGenerators.Add(randomGenerator);
                    edge.Tag = new EdgeData { Generator = randomGenerator, Start = vertex };
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

    static string[] RelatorsFromGraph(Graph dirGraph) {

        EdgeData f(string start, string end)
        {
            dirGraph.TryGetEdge(start, end, out TaggedEdge<string, EdgeData> edge);
            Debug.Assert(edge != null, nameof(edge) + " != null");
            return edge.Tag;
        }

        var loops = 
            from nodeCycle in GetSimpleCycles(dirGraph)
            let l = nodeCycle.Count
            where l != 2
            let edgeDataCycle = (
                from i in Enumerable.Range(0, nodeCycle.Count)
                select (f(nodeCycle[i], nodeCycle[(i + 1) % nodeCycle.Count]), i)
            )
            let generatorCycle = (
                from tuple in edgeDataCycle
                let edgeData = tuple.Item1
                let i = tuple.i
                select
                    edgeData.Start == nodeCycle[i]
                    ? edgeData.Generator
                    : InvertGenerator(edgeData.Generator)
                )
            select string.Join("", generatorCycle)
            ;
        return loops.ToArray();
    }
    
    static Graph RandomGraphWithEdgeWords(int vertexCount, int edgeCount, double proportionOfGenerators, Random random) {
        Graph graph = new();

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

        return graph;
    }


    public static (string[], string[]) RandomPresentation() {
        Random random = new Random();
        string[] generatorNames = GetGeneratorNames(5); // Adjust the size as needed
        Graph graph = RandomGraphWithEdgeWords(5, 20, 0.75, random);
        AssignRandomOrientation(graph, random);
        AssignRandomGenerators(graph, generatorNames, random);
        var relators = RelatorsFromGraph(graph);

        //Console.WriteLine("Generator Names: " + string.Join(", ", generatorNames));
        //Console.WriteLine("Relators: " + string.Join(", ", relators));
        return (generatorNames, relators);
    }

    static List<List<string>> GetSimpleCycles(Graph dirGraph) {
        //return QuikGraph.Algorithms.Search.AllCyclesSearch.FindAllCycles(dirGraph);
        return new List<List<string>>();
        // TODO change to different graph library
    }

}
