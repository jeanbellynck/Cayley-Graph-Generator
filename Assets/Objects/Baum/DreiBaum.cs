using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DreiBaum : MonoBehaviour{
    protected GameObject inhalt;
    protected Line kante;
    protected bool inAnimation = false;
    protected int richtung;
    
    private Coroutine positionCoroutine;
    public float kantenlaenge = 16;
    private Coroutine kantenlaengeCoroutine;
    public float clusterabstand = 2;
    private Coroutine clusterabstandCoroutine;

    public abstract void Init(GameObject knotenPrefab, GameObject linePrefab, int tiefe, int richtung, GameObject vorherigerKnoten);

    public void linieErstellen(GameObject linePrefab, GameObject start, Transform parent) {
        GameObject kanteGO = Instantiate(linePrefab, parent);
        kante = kanteGO.GetComponent<Line>();
        kante.startPoint = start;
        kante.endPoint = inhalt;
    }

    public GameObject getInhalt() {
        return inhalt;
    }
    
    public Line getKante() {
        return kante;
    }

    public abstract void setPosition(Vector3 position);


    public void setPosition(Vector3 position, float time) {
        if(transform.position != position) {
            if(positionCoroutine != null) StopCoroutine(positionCoroutine);
            positionCoroutine = StartCoroutine(setPositionAnimation(transform.position, position, time));
        }
    }
    
    protected IEnumerator setPositionAnimation(Vector3 prevPosition, Vector3 nextPosition, float animationsZeit) {
        float vergangeneZeit = 0;
        while(vergangeneZeit < animationsZeit) {
            vergangeneZeit += Time.deltaTime;
            setPosition(Vector3.Lerp(prevPosition, nextPosition, vergangeneZeit/animationsZeit));
            updatePosition();
            yield return null;
            print("Position wird verändert");
        }
    }

    public Vector3 getPosition() {
        return transform.position;
    }

    public abstract void setKantenlaenge(float kantenlaenge);


    public void setKantenlaenge(float kantenlaenge, float time) {
        if(this.kantenlaenge != kantenlaenge) {
            if(kantenlaengeCoroutine != null) StopCoroutine(kantenlaengeCoroutine);
            kantenlaengeCoroutine = StartCoroutine(setKantenlaengeAnimation(this.kantenlaenge, kantenlaenge, time));
        }
    }
    
    protected IEnumerator setKantenlaengeAnimation(float prevKantenlaenge, float nextKantenlaenge, float animationsZeit) {
        float vergangeneZeit = 0;
        while(vergangeneZeit < animationsZeit) {
            vergangeneZeit += Time.deltaTime;
            setKantenlaenge(Mathf.Lerp(prevKantenlaenge, nextKantenlaenge, vergangeneZeit/animationsZeit));
            updatePosition();
            yield return null;
        }
    }

    public float getKantenlaenge() {
        return kantenlaenge;
    }

    

    public abstract void setClusterabstand(float clusterabstand);

    public void setClusterabstand(float clusterabstand, float time) {
        if(this.clusterabstand != clusterabstand) {
            if(clusterabstandCoroutine != null) StopCoroutine(clusterabstandCoroutine);
            clusterabstandCoroutine = StartCoroutine(setClusterabstandAnimation(this.clusterabstand, clusterabstand, time));
        }
    }
    
    protected IEnumerator setClusterabstandAnimation(float prevClusterabstand, float nextClusterabstand, float animationsZeit) {
        float vergangeneZeit = 0;
        while(vergangeneZeit < animationsZeit) {
            vergangeneZeit += Time.deltaTime;
            setClusterabstand(Mathf.Lerp(prevClusterabstand, nextClusterabstand, vergangeneZeit/animationsZeit));
            updatePosition();
            yield return null;
        }
    }

    public float getClusterabstand() {
        return clusterabstand;
    }

    public abstract void updatePosition();

    public abstract DreiBaum getTeilbaum(int richtung);
}
