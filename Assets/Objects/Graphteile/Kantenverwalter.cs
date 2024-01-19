using System;
using System.Collections;
using System.Collections.Generic;

public class Kantenverwalter {
    IDictionary<(string, string), Kante> kanten = new Dictionary<(string, string), Kante>();
    IDictionary<string, List<string>> eingehendeKnoten = new Dictionary<string, List<string>>();
    IDictionary<string, List<string>> ausgehendeKnoten = new Dictionary<string, List<string>>();
    

    public ICollection<Kante> GetKanten() {
        return kanten.Values;
    }

    public Kante GetKante(string von, string zu) {
        return kanten[(von, zu)];
    }

    public void AddKante(Kante kante) {
        kanten.Add((kante.startPoint.name, kante.endPoint.name), kante);
        if(eingehendeKnoten.ContainsKey(kante.endPoint.name)) {
            eingehendeKnoten[kante.endPoint.name].Add(kante.startPoint.name);
        } else {
            List<string> newList = new List<string>();
            newList.Add(kante.startPoint.name);
            eingehendeKnoten.Add(kante.endPoint.name, newList);
        }
        if(ausgehendeKnoten.ContainsKey(kante.startPoint.name)) {
            ausgehendeKnoten[kante.startPoint.name].Add(kante.endPoint.name);
        } else {
            List<string> newList = new List<string>();
            newList.Add(kante.endPoint.name);
            ausgehendeKnoten.Add(kante.startPoint.name, newList);
        }
    }

    public void RemoveKante(string von, string zu) {
        kanten.Remove((von, zu));
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

    public bool ContainsKante(string von, string zu) {
        return kanten.ContainsKey((von, zu));
    }

    public Knoten kanteFolgen(Knoten knoten, char op) {
        if(knoten == null) {
            return null;
        }


        // Old Code
        if(char.IsLower(op) && ausgehendeKnoten.ContainsKey(knoten.name)) {
            foreach(string ausgehenderKnoten in ausgehendeKnoten[knoten.name]) {
                if(kanten[(knoten.name, ausgehenderKnoten)].name == op.ToString()) {
                    return kanten[(knoten.name, ausgehenderKnoten)].endPoint;
                }
            }
        } else if(eingehendeKnoten.ContainsKey(knoten.name)) {
            foreach(string eingehenderKnoten in eingehendeKnoten[knoten.name]) {
                if(kanten[(eingehenderKnoten, knoten.name)].name.ToUpper() == op.ToString()) {
                    return kanten[(eingehenderKnoten, knoten.name)].startPoint;
                }
            }
        }
        return null;
    }


    public List<Knoten> GetAngehängteKanten(Knoten knoten) {
        List<Knoten> res = GetAusgehendeKnoten(knoten);
        res.AddRange(GetEingehendeKnoten(knoten));
        return res;
    }

    public List<Knoten> GetAusgehendeKnoten(Knoten knoten) {
        List<Knoten> erg = new List<Knoten>();
        if(ausgehendeKnoten.ContainsKey(knoten.name)) {
            foreach(string ausgehenderKnoten in ausgehendeKnoten[knoten.name]) {
                erg.Add(kanten[(knoten.name, ausgehenderKnoten)].endPoint);
            }
        }
        return erg;
    }

    public List<Knoten> GetEingehendeKnoten(Knoten knoten) {
        List<Knoten> erg = new List<Knoten>();
        if(eingehendeKnoten.ContainsKey(knoten.name)) {
            foreach(string eingehenderKnoten in eingehendeKnoten[knoten.name]) {
                erg.Add(kanten[(eingehenderKnoten, knoten.name)].startPoint);
            }
        }
        return erg;
    }

    public void resetKanten()
    {
        kanten.Clear();
        eingehendeKnoten.Clear();
        ausgehendeKnoten.Clear();
    }
}