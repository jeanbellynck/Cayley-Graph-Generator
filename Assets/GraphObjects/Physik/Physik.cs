using UnityEngine;
using System.Collections;
using Dreamteck.Splines.Primitives;

public class Physik : MonoBehaviour, IActivityProvider {
    public float radius; // Size of the boundingBox
    private int dim;

    [Range(0.0f, 20.0f)]
    public float angleForceFactor;

    // The actual maximal force is used to reduce the force over time. If this is smaller than usualMaximalForce then the force is reduced over time.
    [SerializeField] float alpha;
    public float Activity => alpha;
    public float alphaSetting = 1;
    
    public float alphaDecay = 0.1f;

    public float velocityDecay = 0.9f;
    private int dimension = 3; // Describes the dimension of the projection. The default is always 4D.


    LabelledGraphManager graphManager;

    [SerializeField]
    RepulsionForce repulsionForce;
    [SerializeField]
    LinkForce linkForce;
    [SerializeField]
    ProjectionForce projectionForce;

    public void Start() {
    }

    public void startUp(LabelledGraphManager graphManager, int dimension, int generatorCount) {
        this.graphManager = graphManager;
        alpha = alphaSetting;
        this.dimension = dimension;
        this.timeStep = generatorCount > 0 ? 0.5f / generatorCount : 0.1f;
        
        repulsionForce = new RepulsionForce(radius);
        linkForce = new LinkForce();
        projectionForce = new ProjectionForce(0.5f, dimension);
        //dim = 2*generators.Length + 1;
        StartCoroutine(LoopPhysics());
    }

    
    public void Update() {
        if(graphManager== null) return;
        // The following code interpolate the vertices between the physics steps. This makes the animation smoother.
        if (alpha == 0) return;
        foreach(Vertex vertex in graphManager.GetVertices()) {
            var movingDirection = VectorN.ToVector3(vertex.Position) - vertex.transform.position;
            vertex.transform.position += Mathf.Min(Time.deltaTime, 1) * movingDirection;
        }
    }


    public float physicsDeltaTime = 1f;

    public IEnumerator LoopPhysics() {
        float startTime = Time.time - 1; // Reduce on to prevent physicsDeltaTime from being 0
        while(alpha > 0) {
            // Measures the time of a physics step
            physicsDeltaTime = Time.time - startTime;
            startTime = Time.time;

            dim = graphManager.getDim();
            
            ResetForces();
            yield return repulsionForce.ApplyForce(graphManager, alpha);
            yield return projectionForce.ApplyForce(graphManager, alpha);
            yield return linkForce.ApplyForce(graphManager, alpha);
            UpdateVertices();
            // If physics is set to shut down then reduce the maximal force of the physics engine to 0 over 5 seconds
        }
    }

    public float timeStep = 0.25f;

    void UpdateVertices() {
        float realVelocityDecay = Mathf.Pow(velocityDecay, timeStep);
        foreach (Vertex vertex in graphManager.GetVertices()) {
            float ageFactor = Mathf.Max(1, (3 - 1) * (1 - vertex.Age)); // Young vertices are strong
            VectorN force = ageFactor * vertex.Force; 
            vertex.Position += vertex.Velocity * timeStep + 0.5f * force * timeStep * timeStep;
            vertex.Velocity += force * timeStep;
            vertex.Velocity *= realVelocityDecay;

            //vertex.transform.position = VectorN.ToVector3(vertex.Position);
        }
    }


    void ResetForces() {
        foreach (Vertex vertex in graphManager.GetVertices()) {
            vertex.Force = VectorN.Zero(dim);
            vertex.Velocity = vertex.Velocity.ClampMagnitude(radius/10);
            vertex.Position = vertex.Position.ClampMagnitude(radius);
            //vertex.transform.position = VectorN.ToVector3(vertex.Position);
        }
    }



    /** 
    * Slowly reduces the maximal force to 0. This is used to stop the simulation.
    */
    public void shutDown() {
        StartCoroutine(decayAlpha());
    }

    /**
     * Similar to shutDown. For a few seconds the physics engine is reactivated
     **/
    public void shortRevive(float seconds) {
        // Physics is currently disabled
        StopAllCoroutines();
        
        StartCoroutine(LoopPhysics());
        if(alpha != alphaSetting) {
            // Decay is ongoing
            alpha = seconds * alphaDecay;
            StartCoroutine(decayAlpha());
        }
    }


    IEnumerator decayAlpha() {
        while(alpha > 0) {
            alpha -= alphaDecay * Time.deltaTime;
            yield return null;
        }
        alpha = 0;
        StopAllCoroutines();
    }
}