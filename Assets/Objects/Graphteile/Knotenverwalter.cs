using System;
using System.Collections;
using System.Collections.Generic;

public class VertexManager {
    public int idCounter = 0;
    private Dictionary<string, Knoten> knotenMap = new Dictionary<string, Knoten>();
    
    public ICollection<Knoten> getVertex() {
        return knotenMap.Values;
    }


    public Knoten getVertex(string key) {
        return knotenMap[key];
    }


    public void AddKnoten(Knoten vertex) {
        vertex.id = idCounter;
        idCounter++;
        knotenMap.Add(vertex.name, vertex);
    }

    public void RemoveKnoten(Knoten vertex) {
        knotenMap.Remove(vertex.name);
    }

    public bool ContainsKnoten(Knoten vertex) {
        return knotenMap.ContainsKey(vertex.name);
    }

    public bool ContainsKnoten(string knotenName) {
        return knotenMap.ContainsKey(knotenName);
    }

    public Knoten getNeutral()
    {
        return knotenMap[""];
    }

    public void resetKnoten()
    {
        knotenMap.Clear();
        idCounter = 0;
    }
}