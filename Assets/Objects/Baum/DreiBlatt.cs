using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreiBlatt : DreiBaum {

    void Start() {}

    public override void Init(GameObject knotenPrefab, GameObject linePrefab, int tiefe, int richtung, GameObject vorherigerKnoten) {
        inhalt = Instantiate(knotenPrefab, transform.position, Quaternion.identity, transform);
        this.richtung = richtung;
        linieErstellen(linePrefab, vorherigerKnoten, transform);
    }

    public override void setPosition(Vector3 position) {
        transform.position = position;
    }


    public override void setKantenlaenge(float kantenlaenge) {
        base.kantenlaenge = kantenlaenge;
        updatePosition();
    }

    public override void setClusterabstand(float clusterabstand) {
        this.clusterabstand = clusterabstand;
        updatePosition();
    }

    public override DreiBaum getTeilbaum(int richtung) {
        return null;
    }
    public override void updatePosition() {
        //kante.UpdatePostion();
    }

    void Update() {}
}
