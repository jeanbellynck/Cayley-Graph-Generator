using System;
using System.Collections;
using System.Collections.Generic;

public class VertexManager {
    public int idCounter = 0;
    private Dictionary<string, Vertex> knotenMap = new Dictionary<string, Vertex>();
    
    public ICollection<Vertex> getVertex() {
        return knotenMap.Values;
    }


    public Vertex getVertex(string key) {
        return knotenMap[key];
    }


    public void AddKnoten(Vertex vertex) {
        vertex.id = idCounter;
        idCounter++;
        knotenMap.Add(vertex.name, vertex);
    }

    public void RemoveVertex(Vertex vertex) {
        knotenMap.Remove(vertex.name);
    }

    public bool ContainsVertex(Vertex vertex) {
        return knotenMap.ContainsKey(vertex.name);
    }

    public bool ContainsKnoten(string knotenName) {
        return knotenMap.ContainsKey(knotenName);
    }

    public Vertex getNeutral()
    {
        return knotenMap[""];
    }

    public void resetKnoten()
    {
        knotenMap.Clear();
        idCounter = 0;
    }
}