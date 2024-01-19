using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreiAst : DreiBaum{
    private IDictionary<int, DreiBaum> aeste;


    void Start() {

    }


    public IEnumerator Bewegung(Line vorherigeKante, GameObject inhalt, int richtung, float kantenlaenge, float clusterabstand, DreiBaum links, DreiBaum mitte, DreiBaum rechts, float animationsZeit) {
        // Numerische Werte initialisieren
        this.richtung = richtung;
        this.kantenlaenge = kantenlaenge;
        this.clusterabstand = clusterabstand;


        // Äste erstellen und verschieben
        aeste = new Dictionary<int, DreiBaum>();
        aeste.Add((richtung+90)%360, links);
        aeste.Add(richtung, mitte);
        aeste.Add((richtung+270)%360, rechts);
        links.transform.parent = transform;
        mitte.transform.parent = transform;
        rechts.transform.parent = transform;

        links.setKantenlaenge(kantenlaenge/clusterabstand, animationsZeit);
        mitte.setKantenlaenge(kantenlaenge/clusterabstand, animationsZeit);
        rechts.setKantenlaenge(kantenlaenge/clusterabstand, animationsZeit);
        links.setPosition(transform.position + Quaternion.Euler(0,0,(richtung+90)%360) * Vector3.right * kantenlaenge, animationsZeit);
        mitte.setPosition(transform.position + Quaternion.Euler(0,0,(richtung)%360) * Vector3.right *kantenlaenge, animationsZeit);
        rechts.setPosition(transform.position + Quaternion.Euler(0,0,(richtung+270)%360) * Vector3.right * kantenlaenge, animationsZeit);

        // Inhalt und Kanten setzen
        this.inhalt = inhalt;
        this.inhalt.transform.parent = transform;
        this.kante = vorherigeKante;
        kante.transform.parent = transform;
        Vector3 inhaltPos = this.inhalt.transform.position;
        float vergangeneZeit = 0;
        while(vergangeneZeit < animationsZeit) {
            vergangeneZeit += Time.deltaTime;
            this.inhalt.transform.position = Vector3.Lerp(inhaltPos, transform.position, vergangeneZeit/animationsZeit);
            yield return null;
        }
    }



    public override void Init(GameObject knotenPrefab, GameObject linePrefab, int tiefe, int richtung, GameObject vorherigerKnoten) {
        inhalt = Instantiate(knotenPrefab, transform.position, Quaternion.identity, transform);
        linieErstellen(linePrefab, vorherigerKnoten, transform);
        
        aeste = new Dictionary<int, DreiBaum>();
        this.richtung = richtung;

        int[] neueRichtungen = new int[]{(richtung + 90)%360, richtung, (richtung + 270)%360};
        foreach(int neueRichtung in neueRichtungen) {
            DreiBaum neuerAst;
            if(tiefe == 1) {
                neuerAst = new GameObject("Blatt").AddComponent<DreiBlatt>();
            } else {
                neuerAst = new GameObject("Ast").AddComponent<DreiAst>();
            }
            neuerAst.transform.parent = transform;
            neuerAst.transform.position = transform.position + Quaternion.Euler(0,0,richtung) * Vector3.right * kantenlaenge;
            neuerAst.Init(knotenPrefab, linePrefab, tiefe-1, neueRichtung, inhalt);
            aeste.Add(neueRichtung, neuerAst);
        }
    }

    public override void setPosition(Vector3 position) {
        transform.position = position;
    }


    public override void setKantenlaenge(float kantenlaenge) {
        base.kantenlaenge = kantenlaenge;
        foreach(DreiBaum ast in aeste.Values) {
            ast.setKantenlaenge(kantenlaenge/clusterabstand);
        }
    }

    public override void setClusterabstand(float clusterabstand) {
        this.clusterabstand = clusterabstand;
        foreach(DreiBaum ast in aeste.Values) {
            ast.setKantenlaenge(kantenlaenge/clusterabstand);
            ast.setClusterabstand(clusterabstand);
        }
    }

    public override DreiBaum getTeilbaum(int richtung) {
        return aeste[richtung%360];
    }

    public override void updatePosition() {
        print("UpdatePosition wurde aufgerufen.");
        foreach (KeyValuePair<int, DreiBaum> eintrag in aeste) {
            //kante.UpdatePostion();
            eintrag.Value.transform.position = transform.position + Quaternion.Euler(0,0,eintrag.Key) * Vector3.right * kantenlaenge;
            eintrag.Value.updatePosition();
        }
        
    }
    
    
    void Update() {
        
    }
}
