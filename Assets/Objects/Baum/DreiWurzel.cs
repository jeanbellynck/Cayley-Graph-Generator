using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreiWurzel : DreiBaum{
    public IDictionary<int, DreiBaum> aeste;


    void Start() {

    }

    public void Init(GameObject knotenPrefab, GameObject linePrefab, int tiefe) {
        inhalt = Instantiate(knotenPrefab, transform.position, Quaternion.identity, transform);
        aeste = new Dictionary<int, DreiBaum>();

        int[] richtungen = new int[]{0, 90, 180, 270};
        foreach(int richtung in richtungen) {
            DreiBaum neuerAst;
            if(tiefe == 1) {
                neuerAst = new GameObject("Blatt").AddComponent<DreiBlatt>();
            }else{
                neuerAst = new GameObject("Ast").AddComponent<DreiAst>();
            }
            neuerAst.transform.parent = transform;
            neuerAst.transform.position = Quaternion.Euler(0,0,richtung) * Vector3.right * kantenlaenge;
            neuerAst.Init(knotenPrefab, linePrefab, tiefe-1, richtung, inhalt);
            aeste.Add(richtung, neuerAst);
        }
    }

    public override void Init(GameObject knotenPrefab, GameObject linePrefab, int tiefe, int richtung, GameObject vorherigerKnoten) {
        Init(knotenPrefab, linePrefab, tiefe);        
    }

    public override DreiBaum getTeilbaum(int richtung) {
        return null;
    }

    public override void setPosition(Vector3 position) {
        transform.position = position;
    }

    public override void setKantenlaenge(float kantenlaenge) {
        base.kantenlaenge = kantenlaenge;
        foreach(DreiBaum ast in aeste.Values) {
            ast.setKantenlaenge(kantenlaenge/clusterabstand);
        }
        //updatePosition();
    }

    public override void setClusterabstand(float clusterabstand) {
        this.clusterabstand = clusterabstand;
        foreach(DreiBaum ast in aeste.Values) {
            ast.setKantenlaenge(kantenlaenge/clusterabstand);
            ast.setClusterabstand(clusterabstand);
        }
    }

    


    void Update() {
        foreach(KeyValuePair<int, DreiBaum> eintrag in aeste) {
            //eintrag.Value.transform.position = transform.position + Quaternion.Euler(0,0,eintrag.Key) * Vector3.right * kantenlaengeVar;
            //eintrag.Value.setKantenlaenge(kantenlaengeVar/clusterabstand);
            //eintrag.Value.setClusterabstand(clusterabstand);
        }
    }

    public void bewegung(int richtung, float animationsZeit) {
        // Bewegt alles, was komprimiert, erstellt dazu einen Ast und schiebt die Hälfte drauf
        DreiAst neuerAst = new GameObject("Ast (Neu)").AddComponent<DreiAst>();
        neuerAst.transform.position = Quaternion.Euler(0,0,richtung) * Vector3.right * kantenlaenge;
        neuerAst.transform.parent = transform;
        StartCoroutine(neuerAst.Bewegung(aeste[(richtung+180)%360].getKante(), inhalt, richtung, kantenlaenge/clusterabstand, clusterabstand, aeste[(richtung+90)%360], aeste[richtung], aeste[(richtung+270)%360], animationsZeit));
        aeste[richtung] = neuerAst;

        // Bewegt alles was gestreckt wird
        StartCoroutine(expandieren(richtung, animationsZeit));
    }
    private IEnumerator expandieren(int richtung, float animationsZeit) {
        // Verschiebt äste des kleinen Teilbaums auf den großen
        DreiBaum zuExpandienderBaum = aeste[(richtung+180)%360];
        aeste[(richtung+90)%360] = zuExpandienderBaum.getTeilbaum((richtung+90)%360);
        aeste[(richtung+270)%360] = zuExpandienderBaum.getTeilbaum((richtung+270)%360);
        aeste[(richtung+180)%360] = zuExpandienderBaum.getTeilbaum((richtung+180)%360);
        aeste[(richtung+90)%360].transform.parent = transform;
        aeste[(richtung+270)%360].transform.parent = transform;
        aeste[(richtung+180)%360].transform.parent = transform;
        
        aeste[(richtung+90)%360].setKantenlaenge(zuExpandienderBaum.getKantenlaenge(), animationsZeit);
        aeste[(richtung+180)%360].setKantenlaenge(zuExpandienderBaum.getKantenlaenge(), animationsZeit);
        aeste[(richtung+270)%360].setKantenlaenge(zuExpandienderBaum.getKantenlaenge(), animationsZeit);
        aeste[(richtung+90)%360].setPosition(transform.position + Quaternion.Euler(0,0,(richtung+90)%360) * Vector3.right * kantenlaenge, animationsZeit);
        aeste[(richtung+180)%360].setPosition(transform.position + Quaternion.Euler(0,0,(richtung+180)%360) * Vector3.right * kantenlaenge, animationsZeit);
        aeste[(richtung+270)%360].setPosition(transform.position + Quaternion.Euler(0,0,(richtung+270)%360) * Vector3.right * kantenlaenge, animationsZeit);

        // Ändert Eltern des Inhalts
        inhalt = zuExpandienderBaum.getInhalt();
        inhalt.transform.position = transform.position;
        inhalt.transform.parent = transform;
        Vector3 inhaltPos = inhalt.transform.position;
        
        // Zerstört alten Baum
        Destroy(zuExpandienderBaum.gameObject);

        // Verschiebt Inhalt        
        float vergangeneZeit = 0;
        while(vergangeneZeit < animationsZeit) {
            vergangeneZeit += Time.deltaTime;
            this.inhalt.transform.position = Vector3.Lerp(inhaltPos, transform.position, vergangeneZeit/animationsZeit);
            yield return null;
        }
    }

    public override void updatePosition() {
        print("UpdatePosition wurde aufgerufen.");
        foreach (KeyValuePair<int, DreiBaum> eintrag in aeste) {
            eintrag.Value.transform.position = transform.position + Quaternion.Euler(0,0,eintrag.Key) * Vector3.right * kantenlaenge;
            eintrag.Value.updatePosition();
        }
    }

    public void testMovement() {
        aeste[0].setKantenlaenge(kantenlaenge*2, 10);   
        aeste[0].setPosition(60*Vector3.right, 10);
    }
}
