using System;
using System.Collections;
using System.Collections.Generic;

public class EdgeManager {
    IDictionary<(string, string), Kante> edges = new Dictionary<(string, string), Kante>();
    IDictionary<string, List<string>> eingehendeKnoten = new Dictionary<string, List<string>>();
    IDictionary<string, List<string>> ausgehendeKnoten = new Dictionary<string, List<string>>();
    

    public ICollection<Kante> GetKanten() {
        return edges.Values;
    }

    public Kante GetEdge(string von, string zu) {
        return edges[(von, zu)];
    }

    public void AddEdge(Kante edge) {
        edges.Add((edge.startPoint.name, edge.endPoint.name), edge);
        if(eingehendeKnoten.ContainsKey(edge.endPoint.name)) {
            eingehendeKnoten[edge.endPoint.name].Add(edge.startPoint.name);
        } else {
            List<string> newList = new List<string>();
            newList.Add(edge.startPoint.name);
            eingehendeKnoten.Add(edge.endPoint.name, newList);
        }
        if(ausgehendeKnoten.ContainsKey(edge.startPoint.name)) {
            ausgehendeKnoten[edge.startPoint.name].Add(edge.endPoint.name);
        } else {
            List<string> newList = new List<string>();
            newList.Add(edge.endPoint.name);
            ausgehendeKnoten.Add(edge.startPoint.name, newList);
        }
    }

    public void RemoveEdge(string von, string zu) {
        edges.Remove((von, zu));
        if(eingehendeKnoten.ContainsKey(zu)) {
            if(eingehendeKnoten[zu].Contains(von)) {
                eingehendeKnoten[zu].Remove(von);
            }
        }
        if(ausgehendeKnoten.ContainsKey(von)) {
            if(ausgehendeKnoten[von].Contains(zu)) {
                ausgehendeKnoten[von].Remove(zu);
            }
        }
    }

    public bool ContainsEdge(string von, string zu) {
        return edges.ContainsKey((von, zu));
    }

    public Vertex followEdge(Vertex vertex, char op) {
        if(vertex == null) {
            return null;
        }


        // Old Code
        if(char.IsLower(op) && ausgehendeKnoten.ContainsKey(vertex.name)) {
            foreach(string ausgehenderKnoten in ausgehendeKnoten[vertex.name]) {
                if(edges[(vertex.name, ausgehenderKnoten)].name == op.ToString()) {
                    return edges[(vertex.name, ausgehenderKnoten)].endPoint;
                }
            }
        } else if(eingehendeKnoten.ContainsKey(vertex.name)) {
            foreach(string eingehenderKnoten in eingehendeKnoten[vertex.name]) {
                if(edges[(eingehenderKnoten, vertex.name)].name.ToUpper() == op.ToString()) {
                    return edges[(eingehenderKnoten, vertex.name)].startPoint;
                }
            }
        }
        return null;
    }


    public List<Vertex> GetAngehängteKanten(Vertex vertex) {
        List<Vertex> res = GetOutgoingVertices(vertex);
        res.AddRange(GetIngoingVertices(vertex));
        return res;
    }

    public List<Vertex> GetOutgoingVertices(Vertex vertex) {
        List<Vertex> erg = new List<Vertex>();
        if(ausgehendeKnoten.ContainsKey(vertex.name)) {
            foreach(string ausgehenderKnoten in ausgehendeKnoten[vertex.name]) {
                erg.Add(edges[(vertex.name, ausgehenderKnoten)].endPoint);
            }
        }
        return erg;
    }

    public List<Vertex> GetIngoingVertices(Vertex vertex) {
        List<Vertex> erg = new List<Vertex>();
        if(eingehendeKnoten.ContainsKey(vertex.name)) {
            foreach(string eingehenderKnoten in eingehendeKnoten[vertex.name]) {
                erg.Add(edges[(eingehenderKnoten, vertex.name)].startPoint);
            }
        }
        return erg;
    }

    public void resetKanten()
    {
        edges.Clear();
        eingehendeKnoten.Clear();
        ausgehendeKnoten.Clear();
    }
}