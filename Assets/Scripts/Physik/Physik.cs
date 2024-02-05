using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class Physik : MonoBehaviour {
    public float radius; // Size of the boundingBox
    [SerializeField]
    private float precision; // Used by the QuadTree. Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. 


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
    char[] generators;
    Dictionary<char, Dictionary<char, float>> sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
    // Necessary since if you dont count to many the average is too small
    Dictionary<char, Dictionary<char, int>> countedArrowAngles = new Dictionary<char, Dictionary<char, int>>();
    Dictionary<char, Dictionary<char, float>> averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();

    GraphManager graphManager;

    public Physik(float radius) {
        this.radius = radius;
    }

    public void setGenerators(char[] generators) {
        this.generators = new char[generators.Length * 2];
        for (int i = 0; i < generators.Length; i++) {
            this.generators[i] = generators[i];
            this.generators[i + generators.Length] = char.ToUpper(generators[i]);
        }

        sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
        countedArrowAngles = new Dictionary<char, Dictionary<char, int>>();
        averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();

        foreach (char gen1 in this.generators) {
            sumArrowAngles[gen1] = new Dictionary<char, float>();
            countedArrowAngles[gen1] = new Dictionary<char, int>();
            averageArrowAngles[gen1] = new Dictionary<char, float>();
        }
        currentIteration = 0;
    }



    public void UpdatePh(GraphManager graphManager) {
        this.graphManager = graphManager;
        if(generators == null) {
            return;
        }
        geschwindigkeitenZurücksetzen();
        if (repelForceFactor != 0) calculateRepulsionForces();
        if (linkForceFactor != 0) calculateLinkForces();
        //if (angleForceFactor != 0) calculateStress();

        updateVertices();
        AveragePreviousAndCurrentPosition();
        updateVertices();

        // If physics is set to shut down then reduce the maximal force of the physics engine to 0 over 5 seconds
        if (actualMaximalForce < usualMaximalForce && 0 < actualMaximalForce) {
            actualMaximalForce -= usualMaximalForce * Time.deltaTime / shutDownTime;
        }
        if (actualMaximalForce < 0) {
            actualMaximalForce = 0;
        }
    }

    private void updateVertices() {
        float velocityDecayForThisTomeStep = Mathf.Pow(velocityDecay, Time.deltaTime);
        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.PreviousPosition = vertex.transform.position;
            Vector3 force = vertex.RepelForce + vertex.LinkForce;
            force = Vector3.ClampMagnitude(force, actualMaximalForce);
            vertex.Velocity = vertex.Velocity + force;
            vertex.transform.position += vertex.Velocity * Time.deltaTime;
            vertex.Velocity *= velocityDecayForThisTomeStep;
            vertex.transform.position = Vector3.ClampMagnitude(vertex.transform.position, radius);
        }
    }

    /**
     * This method averages the previous and the current position of the vertices. This is used to calculate the forces using the improved euler method.
     **/
    private void AveragePreviousAndCurrentPosition() {
        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.transform.position = (vertex.transform.position + vertex.PreviousPosition) / 2;
        }
    }

    /**
     * Sets the speed of all vertices to 0.
     * Also bounds th force by the maximal force.
     */
    private void geschwindigkeitenZurücksetzen() {
        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.LinkForce = Vector3.zero;
            vertex.RepelForce = Vector3.zero;
        }
    }

    public float repulsionDistance;

    private void calculateRepulsionForces() {
        // Every tick the BarnesQuadtree is recalculated. This is expensive but necessary since the vertices move.
        BarnesQuadtree bqb = new BarnesQuadtree(Vector2.zero, radius, precision, 0.1f);
        foreach (Vertex vertex in graphManager.getVertex()) {
            bqb.Add(vertex);
        }
        bqb.BerechneSchwerpunkt();
        foreach (Vertex vertex in graphManager.getVertex()) {
            //float hyperbolicity = 
            float ageFactor = Mathf.Max(1, (10 - 1) * (1 - vertex.Age)); // Young vertices are repelled strongly
            //vertex.repelForce -= ageFactor * repelForceFactor * bqb.calculateRepulsionForceOnVertex(vertex.transform.position + vertex.velocity*Time.deltaTime, repulsionDistance);
            Vector3 force = ageFactor * repelForceFactor * bqb.calculateRepulsionForceOnVertex(vertex, repulsionDistance);
            vertex.RepelForce = force;
        }
    }

    public int stabilityIterations = 1;
    private void calculateLinkForces() {
        for (int i = 0; i < stabilityIterations; i++) {
            foreach (Edge edge in graphManager.GetKanten()) {
                float ageFactor = Mathf.Max(1, (10 - 1) * (1 - edge.age)); // Young edges are strong
                Vector3 force = calculateLinkForce(edge);
                edge.StartPoint.LinkForce = edge.StartPoint.LinkForce + ageFactor * linkForceFactor * force;
                edge.EndPoint.LinkForce = edge.EndPoint.LinkForce-ageFactor * linkForceFactor * force;
            }
        }
    }



    public int arrowAverageRecalculationIteration = 10;
    int currentIteration = 0;

    /**
     * Calculates the stress of a vertex. The stress is a measure of how unusual the angles of the vertex are. It is used to visualize weird spots.
     **/
     /**
    private void calculateStress() {
        if (currentIteration == 0) {
            //calculateAverageAngles();
            currentIteration = arrowAverageRecalculationIteration;
        }
        currentIteration--;

        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.Stress = 0;
            foreach (char gen1 in generators) {
                foreach (char gen2 in generators) {
                    Vertex vertex1 = vertex.FollowEdge(gen1);
                    Vertex vertex2 = vertex.FollowEdge(gen2);

                    if (gen1 != gen2 && vertex1 != null && vertex2 != null && vertex1.name != vertex2.name) {
                        Vector3 force = angleForceFactor * calculateAverageAngleForceBetweenTwoVertices(vertex, vertex1, vertex2, averageArrowAngles[gen1][gen2]);
                        vertex.Stress += force.magnitude;
                    }
                }
            }
        }
    }**/


    /**
     * Goes over each pair of angles and calculates theirs averages. This is done once per recalculationIteration to reduce the number of calculations.
     * Disabled since this method should not be sitting in physics. 
     **/
     /**
    private void calculateAverageAngles() {
        foreach (char gen1 in generators) {
            foreach (char gen2 in generators) {
                countedArrowAngles[gen1][gen2] = 0;
                sumArrowAngles[gen1][gen2] = 0f;
                averageArrowAngles[gen1][gen2] = 0f;

                foreach (Vertex vertex in graphManager.getVertex()) {
                    Vertex vertex1 = vertex.followEdge(gen1);
                    Vertex vertex2 = vertex.followEdge(gen2);

                    if (vertex1 != null && vertex2 != null && vertex1.Id != vertex2.Id) {
                        // Thing is calculated twice, this can be optimized
                        sumArrowAngles[gen1][gen2] += Vector3.Angle(vertex1.transform.position - vertex.transform.position, vertex2.transform.position - vertex.transform.position);
                        countedArrowAngles[gen1][gen2] += 1;
                    }
                }

                averageArrowAngles[gen1][gen2] = sumArrowAngles[gen1][gen2] / countedArrowAngles[gen1][gen2];
            }
        }
    }

    private Vector3 calculateAverageAngleForceBetweenTwoVertices(Vertex vertexBase, Vertex vertex1, Vertex vertex2, float averageAngle) {
        Vector3 v = Vector3.Normalize(vertex1.transform.position - vertexBase.transform.position);
        Vector3 w = Vector3.Normalize(vertex2.transform.position - vertexBase.transform.position);

        Vector3 u = v - w;
        Vector3.OrthoNormalize(ref v, ref u); // u become the orthogonal vector

        float angleDifference = averageAngle - Vector3.Angle(v, w);

        return angleDifference / 90 * (angleDifference / 90) * u;
    }
    **/


    private Vector3 calculateLinkForce(Edge edge) {
        Vertex source = edge.StartPoint;
        Vertex target = edge.EndPoint;
        Vector3 diff = target.transform.position + target.Velocity * Time.deltaTime - source.transform.position - source.Velocity * Time.deltaTime;
        // Wenn die zwei Knoten aufeinander liegen, dann bewege sie ein bisschen auseinander.
        if (diff == Vector3.zero) {
            diff = 0.05f * UnityEngine.Random.insideUnitSphere;
        }
        float mag = diff.magnitude;
        // float vertexFactor = Mathf.Sqrt(graphManager.getVertex().Count); // The idea is the following. In a normal graph the edges of a graph grow with the number of vertices. To simulate this effekt the link force is multiplied by the number of vertices.
        return (mag - edge.Length) / mag * diff;
    }

    public void startUp() {
        actualMaximalForce = usualMaximalForce;
    }

    /** 
    * Slowly reduces the maximal force to 0. This is used to stop the simulation.
    */
    public void shutDown() {
        actualMaximalForce -= 0.01f;
    }
}