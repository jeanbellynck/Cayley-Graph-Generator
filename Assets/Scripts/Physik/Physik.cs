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
    public float angleForceFactor;

    // The actual maximal force is used to reduce the force over time. If this is smaller than usualMaximalForce then the force is reduced over time.
    private float alpha = 1;
    public float alphaSetting = 1;
    
    public float alphaDecay = 0.1f;

    public float velocityDecay = 0.9f;

    GraphManager graphManager;

    [SerializeField]
    RepulsionForce repulsionForce;
    [SerializeField]
    LinkForce linkForce;

    public void Start() {
        repulsionForce = new RepulsionForce(radius);
        linkForce = new LinkForce();
    }

    public void startUp(GraphManager graphManager) {
        this.graphManager = graphManager;
        alpha = alphaSetting;
        //dim = 2*generators.Length + 1;
        StartCoroutine(LoopPhysics());
    }

    public void Update() {
        if(graphManager== null) return;
        foreach(Vertex vertex in graphManager.getVertex()) {
            UnityEngine.Vector3 velocity = VectorN.ToVector3(vertex.Velocity);
            vertex.transform.position += velocity * Time.deltaTime;
        }
    }

    public IEnumerator LoopPhysics() {
        while(true) {
            dim = graphManager.getDim();
            
            geschwindigkeitenZurücksetzen();
            yield return repulsionForce.ApplyForce(graphManager, alpha);
            yield return linkForce.ApplyForce(graphManager, alpha);
            updateVertices();

            // If physics is set to shut down then reduce the maximal force of the physics engine to 0 over 5 seconds
            yield return null;
        }
    }

    public float timeStep = 0.25f;

    private void updateVertices() {
        float realVelocityDecay = Mathf.Pow(velocityDecay, timeStep);
        foreach (Vertex vertex in graphManager.getVertex()) {
            float ageFactor = Mathf.Max(1, (3 - 1) * (1 - vertex.Age)); // Young vertices are strong
            VectorN force = ageFactor * (vertex.RepelForce + vertex.LinkForce); 
            vertex.Position += vertex.Velocity * timeStep + 0.5f * force * timeStep * timeStep;
            vertex.Velocity += force * timeStep;
            vertex.Velocity *= realVelocityDecay;
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
            vertex.Velocity = vertex.Velocity.ClampMagnitude(radius/10);
            vertex.Position = vertex.Position.ClampMagnitude(radius);
            vertex.transform.position = VectorN.ToVector3(vertex.Position);
        }
    }



    /** 
    * Slowly reduces the maximal force to 0. This is used to stop the simulation.
    */
    public void shutDown() {
        alpha -= 0.01f;
        
            /**
            if (actualMaximalForce < usualMaximalForce && 0 < actualMaximalForce) {
                actualMaximalForce -= usualMaximalForce * Time.deltaTime / shutDownTime;
            }
            if (actualMaximalForce < 0) {
                actualMaximalForce = 0;
            }**/
        StopAllCoroutines();
    }

    public IEnumerator decayAlpha() {
        while(alpha > 0) {
            alpha -= alphaDecay * Time.deltaTime;
            yield return null;
        }
        alpha = 0;
    }
}