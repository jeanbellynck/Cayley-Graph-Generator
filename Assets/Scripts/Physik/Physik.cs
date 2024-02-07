using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Numerics;
using System.Collections;

public class Physik : MonoBehaviour {
    public float radius; // Size of the boundingBox
    private int dim;

    [Range(0.0f, 20.0f)]
    public float repelForceFactor;
    [Range(0.0f, 20.0f)]
    public float linkForceFactor;
    [Range(0.0f, 20.0f)]
    public float angleForceFactor;

    public float usualMaximalForce = 10;
    // The actual maximal force is used to reduce the force over time. If this is smaller than usualMaximalForce then the force is reduced over time.
    public float actualMaximalForce = 10;
    public float shutDownTime = 5;

    public float velocityDecay = 0.9f;

    GraphManager graphManager;

    [SerializeField]
    RepulsionForce repulsionForce;
    [SerializeField]
    LinkForce linkForce;

    public void Start() {
        repulsionForce = new RepulsionForce(repelForceFactor, 50, radius, actualMaximalForce);
        linkForce = new LinkForce(linkForceFactor);
    }

    public void startUp(GraphManager graphManager) {
        this.graphManager = graphManager;
        actualMaximalForce = usualMaximalForce;
        //dim = 2*generators.Length + 1;
        StartCoroutine(LoopPhysics());
    }

    public IEnumerator LoopPhysics() {
        while(true) {
            dim = graphManager.getDim();
            
            geschwindigkeitenZurücksetzen();
            yield return repulsionForce.ApplyForce(graphManager, 1);
            yield return linkForce.ApplyForce(graphManager, 1);
            updateVertices();

            // If physics is set to shut down then reduce the maximal force of the physics engine to 0 over 5 seconds
            /**
            if (actualMaximalForce < usualMaximalForce && 0 < actualMaximalForce) {
                actualMaximalForce -= usualMaximalForce * Time.deltaTime / shutDownTime;
            }
            if (actualMaximalForce < 0) {
                actualMaximalForce = 0;
            }**/
            yield return null;
        }
    }

    private void updateVertices() {
        foreach (Vertex vertex in graphManager.getVertex()) {
            VectorN force = vertex.RepelForce + vertex.LinkForce;
            vertex.Position += vertex.Velocity * Time.deltaTime + 0.5f * force * Time.deltaTime * Time.deltaTime;
            vertex.Position = vertex.Position.ClampMagnitude(radius);
            vertex.Velocity *= Mathf.Pow(velocityDecay, Time.deltaTime);
            vertex.Velocity += force * Time.deltaTime;
        }
    }


    /**
     * Sets the speed of all vertices to 0.
     * Also bounds th force by the maximal force.
     */
    private void geschwindigkeitenZurücksetzen() {
        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.LinkForce = VectorN.Zero(dim);
            vertex.RepelForce = VectorN.Zero(dim);
        }
    }



    /** 
    * Slowly reduces the maximal force to 0. This is used to stop the simulation.
    */
    /**
    public void shutDown() {
        actualMaximalForce -= 0.01f;
        StopAllCoroutines();
    }
    **/
}