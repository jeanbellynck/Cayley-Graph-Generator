using System;
using System.Collections;
using System.Collections.Generic;

public class Knotenverwalter {
    public int idCounter = 0;
    private Dictionary<string, Knoten> knotenMap = new Dictionary<string, Knoten>();
    
    public ICollection<Knoten> GetKnoten() {
        return knotenMap.Values;
    }


    public Knoten GetKnoten(string key) {
        return knotenMap[key];
    }


    public void AddKnoten(Knoten knoten) {
        knoten.id = idCounter;
        idCounter++;
        knotenMap.Add(knoten.name, knoten);
    }

    public void RemoveKnoten(Knoten knoten) {
        knotenMap.Remove(knoten.name);
    }

    public bool ContainsKnoten(Knoten knoten) {
        return knotenMap.ContainsKey(knoten.name);
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