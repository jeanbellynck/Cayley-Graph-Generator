using UnityEngine;
using System.Collections;
using Dreamteck.Splines.Primitives;

public class Physik : MonoBehaviour, IActivityProvider {
    int dim;

    [Range(0.0f, 20.0f)] [SerializeField] float angleForceFactor;

    // The actual maximal force is used to reduce the force over time. If this is smaller than usualMaximalForce then the force is reduced over time.
    [SerializeField] float alpha;

    [SerializeField] float radius; // Size of the boundingBox
    public float Activity => alpha;
    [SerializeField] float alphaSetting = 1;
    
    [SerializeField] float alphaDecay = 0.1f;

    [SerializeField] float velocityDecay = 0.9f;
    [SerializeField] bool running;
    [SerializeField] bool decaying;

    [SerializeField] LabelledGraphManager graphManager;

    [SerializeField] RepulsionForce repulsionForce;
    [SerializeField] LinkForce linkForce;
    [SerializeField] ProjectionForce projectionForce;

    public void Initialize(LabelledGraphManager graphManager, int dimension, int generatorCount) {
        Abort();
        this.graphManager = graphManager;
        timeStep = generatorCount > 0 ? 0.5f / generatorCount : 0.1f;
        
        repulsionForce = new RepulsionForce(radius);
        linkForce = new LinkForce();
        projectionForce = new ProjectionForce(0.5f, dimension);
        //dim = 2*generators.Length + 1;
        //alpha = alphaSetting;
        //StartCoroutine(LoopPhysics());
    }

    
    public void Update() {
        if (graphManager == null) return;
        // The following code interpolate the vertices between the physics steps. This makes the animation smoother.
        if (alpha == 0) return;
        foreach(Vertex vertex in graphManager.GetVertices()) {
            var movingDirection = VectorN.ToVector3(vertex.Position) - vertex.transform.position;
            vertex.transform.position += Mathf.Min(Time.deltaTime, 1) * movingDirection;
        }
    }


    public float physicsDeltaTime = 1f;

    IEnumerator LoopPhysics() {
        running = true;
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
        running = false;
    }

    float timeStep = 0.25f;

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
    public void BeginShutDown(float time = -1f) {
        if (!decaying)
            StartCoroutine(DecayAlpha(time));
    }

    public void Abort() {
        running = false;
        decaying = false;
        alpha = 0;
        StopAllCoroutines();
    }

    /**
     * Similar to BeginShutDown. For a few seconds the physics engine is reactivated
     **/
    public void RunShortly() {
        Run();
        BeginShutDown();
    }

    public void Run() {
        alpha = alphaSetting;
        if (!running) 
            StartCoroutine(LoopPhysics());
    }


    IEnumerator DecayAlpha(float time = -1f) {
        var decay = time > 0 ? 1 / time : alphaDecay;
        decaying = true;
        while(true) {
            alpha -= alphaDecay * Time.deltaTime;
            if (alpha <= 0) {
                alpha = 0;
                break;
            }
            yield return null;
        }

        decaying = false;
        //StopAllCoroutines(); // the Physics Coroutine should stop by itself!
    }
}