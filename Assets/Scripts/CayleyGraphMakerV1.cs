using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CayleyGraphMakerV1 : CayleyGraphMaker
{
    List<string> elementReihenfolge = new List<string>();

    IDictionary<char, Color> operationColors = new Dictionary<char, Color>();

    // Konfigurationen
    public bool loadFile;
    public float idealeLänge;
    public float tiefe;
    public float schrumpffaktor; 

    

    

    // Start is called before the first frame update
    public override void InitializeCGMaker()
    {
        if(loadFile) {
            dateiLaden();
        }

        float abstandzwischenkreisen = Mathf.Sin((2*Mathf.PI)/(2*2*generators.Length));
        print("Der halbe normierte Abstand zwischen Kreisen ist: " + abstandzwischenkreisen);
        schrumpffaktor = 1-1/(abstandzwischenkreisen+1);
        print("Schrumpffaktor ist: " + schrumpffaktor);

        for(int i = 0; i<generators.Length; i++) {
            operationColors.Add(generators[i], colourList[i]);
        }

        freieGruppeZeichnen();
        StartCoroutine(RelatorenAnwenden());
    }

    public class Config
    {
        public int depth = 4;
        public string[] generators;// = new string[]{"a", "b", "c"};
        public string[] relators;// = new string[]{"abAB"};
        public bool hasPhysics = true;
    }

    void dateiLaden() {
        using (StreamReader r = new StreamReader("config.json"))
        {
            string json = r.ReadToEnd();
            Config config = JsonUtility.FromJson<Config>(json);
            tiefe = config.depth;
            generators = new char[config.generators.Length];
            for(int i = 0; i < config.generators.Length; i++) {
                generators[i] = config.generators[i][0];
            }
            relators = config.relators;
        }
    }

    void freieGruppeZeichnen() {
        // Erzeugen des symmetrischen Alphabets
        char[] alphabet = new char[2*generators.Length];
        for (int i = 0; i< generators.Length; i++) {
            alphabet[i] = generators[i];
            alphabet[generators.Length + i] = Char.ToUpper(generators[i]);
        }

        // Erzeugen der Knoten/Element
        print("Das Alphabet ist " + alphabet.ToString());
        for(int wortlaenge = 0; wortlaenge <= tiefe; wortlaenge++) {
            for(int currentword = 0; currentword < Math.Pow(alphabet.Length, wortlaenge); currentword++) {
                konstruiereCurrentWord(currentword, wortlaenge, alphabet);
            }
        }
    }


    void konstruiereCurrentWord(int wortid, int wortlaenge, char[] alphabet) {
        int[] wortcode = new int[wortlaenge];
        for(int currentletter = 0; currentletter < wortlaenge; currentletter++) {
            wortcode[currentletter] = (int)(wortid/Math.Pow(alphabet.Length, currentletter))%alphabet.Length;
            if(currentletter > 0 && Math.Abs(wortcode[currentletter]-wortcode[currentletter-1]) == alphabet.Length/2) {
                return;
            }
        }
        elementHinzufügen(wortcode, alphabet);
        if(wortlaenge > 0) {
            kanteHinzufügen(wortcode, alphabet);
        }
    }

    void elementHinzufügen(int[] wortcode, char[] alphabet) {
        string elementName = "";
        Vector3 elementPosition = Vector3.zero;
        for(int i = 0; i<wortcode.Length; i++) {
            elementName += alphabet[wortcode[i]];
            float richtung = 2*Mathf.PI*wortcode[i]/alphabet.Length;
            Vector3 verschiebung = (idealeLänge * Mathf.Pow(schrumpffaktor, i)) * new Vector3(Mathf.Cos(richtung), Mathf.Sin(richtung), 0);
            elementPosition += verschiebung;
        }
        // Zufallsverschiebung
        System.Random r = new System.Random();
        elementPosition = elementPosition + new Vector3(0, 0, r.Next(20)-10);
        //elementPosition = elementPosition + new Vector3(0, 0, 0);
        
        Knoten neuerKnoten = Instantiate(knotenPrefab, transform.position+elementPosition, Quaternion.identity, transform).GetComponent<Knoten>();
        neuerKnoten.name = elementName;
        knotenverwalter.AddKnoten(neuerKnoten);
        elementReihenfolge.Add(neuerKnoten.name);
    }

    void kanteHinzufügen(int[] wortcode, char[] alphabet) {
        string elementName = "";
        for(int i = 0; i<wortcode.Length-1; i++) {
            elementName += alphabet[wortcode[i]];
        }
        char letzterBuchstabe = alphabet[wortcode[wortcode.Length-1]];

        // Kante erstellen
        Kante neueKante = Instantiate(kantenPrefab, transform).GetComponent<Kante>();
        neueKante.SetFarbe(operationColors[Char.ToLower(letzterBuchstabe)], new Color(100,100,100));
        neueKante.name = ""+Char.ToLower(letzterBuchstabe);
        Knoten startKnoten = knotenverwalter.GetKnoten(elementName);
        Knoten endKnoten = knotenverwalter.GetKnoten(elementName+letzterBuchstabe);
        if(Char.IsLower(letzterBuchstabe)) {
            neueKante.SetEndpunkte(startKnoten, endKnoten);
        }else{
            neueKante.SetEndpunkte(endKnoten, startKnoten);
        }
        kantenverwalter.AddKante(neueKante);
    }
    
    IEnumerator RelatorenAnwenden() {
        foreach (string relator in relators) {
            print("Wende Relator " + relator + " an");
            foreach (string element in elementReihenfolge) {
                print("Prüfe, ob das Element " + element + " in der Knotenmenge ist ");
                if(knotenverwalter.ContainsKnoten(element)) {
                    print("Wende Relator " + relator + " auf " + element + " an");
                    RelatorAnwenden(knotenverwalter.GetKnoten(element), relator);
                    print("Vereine alle Zweige");
                    yield return AlleZweigeVereinen();
                }
            }
        }
    }

    List<string> knotenAbzuarbeiten = new List<string>();

    void RelatorAnwenden(Knoten anfangselement, string relator) {
        Knoten aktuellesWort = anfangselement;
        // Kante folgen
        foreach(char op in relator) {
            aktuellesWort = kantenverwalter.kanteFolgen(aktuellesWort, op);
            if(aktuellesWort == null) return;
        }
        
        ZweigeVereinen(anfangselement, aktuellesWort);
    }

    IEnumerator AlleZweigeVereinen() {
        int iterator = 0;
        int iterationenProFrame = 1;
        
        while(knotenAbzuarbeiten.Count != 0) {
            
            
            if(knotenverwalter.ContainsKnoten(knotenAbzuarbeiten[0])) {
                if(iterator++ == iterationenProFrame) {
                    iterator = 0;
                    yield return new WaitForSeconds(0.001f);
                }
                Knoten knoten = knotenverwalter.GetKnoten(knotenAbzuarbeiten[0]); 

                bool duplikateGefunden = false;
                Dictionary<char, Knoten> geprüfteKanten = new Dictionary<char, Knoten>();
                foreach(Knoten ausgehenderKnoten in kantenverwalter.GetAusgehendeKnoten(knoten)) {
                    char aktuelleOp = kantenverwalter.GetKante(knoten.name, ausgehenderKnoten.name).name[0];
                    
                    if(geprüfteKanten.ContainsKey(aktuelleOp)) {
                        ZweigeVereinen(geprüfteKanten[aktuelleOp], ausgehenderKnoten);
                        duplikateGefunden = true;
                    } else {
                        geprüfteKanten.Add(aktuelleOp, ausgehenderKnoten);
                    }
                }
                geprüfteKanten = new Dictionary<char, Knoten>();
                foreach(Knoten eingehenderKnoten in kantenverwalter.GetEingehendeKnoten(knoten)) {
                    char aktuelleOp = kantenverwalter.GetKante(eingehenderKnoten.name, knoten.name).name[0];
                    
                    if(geprüfteKanten.ContainsKey(aktuelleOp)) {
                        ZweigeVereinen(geprüfteKanten[aktuelleOp], eingehenderKnoten);
                        duplikateGefunden = true;
                    } else {
                        geprüfteKanten.Add(aktuelleOp, eingehenderKnoten);
                    }
                
                }

                if(!duplikateGefunden) {
                    knotenAbzuarbeiten.RemoveAt(0);
                }
            } else {
                knotenAbzuarbeiten.RemoveAt(0);
            }
        }
        iterationenProFrame = (int)(100/Time.deltaTime);
        print("DeltaTime:" + Time.deltaTime);
    }


    void ZweigeVereinen(Knoten zweig1, Knoten zweig2) {
        String name1 = zweig1.name;
        String name2 = zweig2.name;
        if(zweig1.name == zweig2.name) {
            return;
        }
        // Neuen Knoten erstellen, soll den Namen des kürzeren Wegs tragen
        Knoten kurzesWort;
        Knoten langesWort;
        if(zweig1.name.Length <= zweig2.name.Length) {
            kurzesWort = zweig1;
            langesWort = zweig2;
        } else {
            kurzesWort = zweig2;
            langesWort = zweig1;
        }

        // Alle ausgehenden und eingehenden Kanten auf den neuen Knoten umleiten.
        foreach(Knoten ausgehenderKnoten in kantenverwalter.GetAusgehendeKnoten(langesWort)) {
            Kante kante = kantenverwalter.GetKante(langesWort.name, ausgehenderKnoten.name);
            if(kantenverwalter.ContainsKante(kurzesWort.name, ausgehenderKnoten.name)) {
                Debug.Assert(kantenverwalter.GetKante(kurzesWort.name, ausgehenderKnoten.name).name == kante.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                // Duplikat löschen
                Destroy(kante.gameObject);
            } else {
                kante.SetStartpunkt(kurzesWort);
                kantenverwalter.AddKante(kante);
            }
            kantenverwalter.RemoveKante(langesWort.name, ausgehenderKnoten.name);
        }
        foreach(Knoten eingehenderKnoten in kantenverwalter.GetEingehendeKnoten(langesWort)) {
            Kante kante = kantenverwalter.GetKante(eingehenderKnoten.name, langesWort.name);
            if(kantenverwalter.ContainsKante(eingehenderKnoten.name, kurzesWort.name)) {
                Debug.Assert(kantenverwalter.GetKante(eingehenderKnoten.name, kurzesWort.name).name == kante.name, "Wenn zwei gleiche Operationen zum gleichen Element führen, kann man beide Operationen gleichsetzen. Das ist aber noch nicht implementiert");
                Destroy(kante.gameObject);
            } else {
                kante.SetEndpunkt(kurzesWort);
                kantenverwalter.AddKante(kante);
            }
            kantenverwalter.RemoveKante(eingehenderKnoten.name, langesWort.name);
        }
        
        // Anderen Knoten löschen
        knotenverwalter.RemoveKnoten(langesWort);
        Destroy(langesWort.gameObject);
        knotenAbzuarbeiten.Add(kurzesWort.name);
    }

    internal override void setVertexNumber(int v)
    {
        throw new NotImplementedException();
    }
}
