using UnityEngine;
using System.Collections.Generic;
using System;

public class Physik : MonoBehaviour {
    public float radius; // Size of the boundingBox
    [SerializeField]
    private float precision; // Used by the QuadTree. Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. 


    [Range(0.0f, 20.0f)]
    public float repelForceFactor;
    [Range(0.0f, 20.0f)]
    public float attractForceFactor;
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
        if(generators == null) {
            return;
        }
        geschwindigkeitenZurücksetzen(graphManager);
        if (repelForceFactor != 0) calculateRepulsionForces(graphManager);
        if (attractForceFactor != 0) calculateLinkForces(graphManager);
        if (angleForceFactor != 0) calculateStress(graphManager);

        updateVertices(graphManager);

        // If physics is set to shut down then reduce the maximal force of the physics engine to 0 over 5 seconds

        if (actualMaximalForce < usualMaximalForce && 0 < actualMaximalForce) {
            actualMaximalForce -= usualMaximalForce * Time.deltaTime / shutDownTime;
        }
        if (actualMaximalForce < 0) {
            actualMaximalForce = 0;
        }
    }

    private void updateVertices(GraphManager vertexManager) {
        foreach (GroupElement vertex in vertexManager.getVertex()) {
            Vector3 force = vertex.repelForce + vertex.attractForce;
            force = Vector3.ClampMagnitude(force, actualMaximalForce);
            vertex.velocity = vertex.velocity + force;
            vertex.transform.position += vertex.velocity * Time.deltaTime;
            vertex.velocity *= velocityDecay;
            vertex.transform.position = Vector3.ClampMagnitude(vertex.transform.position, radius);
        }
    }

    /**
     * Sets the speed of all vertices to 0.
     * Also bounds th force by the maximal force.
     */
    private void geschwindigkeitenZurücksetzen(GraphManager vertexManager) {
        foreach (GroupElement vertex in vertexManager.getVertex()) {
            vertex.attractForce = Vector3.zero;
            vertex.repelForce = Vector3.zero;
        }
    }

    public float repulsionDistance;

    private void calculateRepulsionForces(GraphManager vertexManager) {
        // Every tick the BarnesQuadtree is recalculated. This is expensive but necessary since the vertices move.
        BarnesQuadtree bqb = new BarnesQuadtree(Vector2.zero, radius, precision, 0.1f);
        foreach (GroupElement vertex in vertexManager.getVertex()) {
            bqb.Add(vertex);
        }
        bqb.BerechneSchwerpunkt();
        foreach (GroupElement vertex in vertexManager.getVertex()) {
            //float hyperbolicity = 
            float ageFactor = Mathf.Max(1, (10 - 1) * (1 - vertex.age)); // Young vertices are repelled strongly
            //vertex.repelForce -= ageFactor * repelForceFactor * bqb.calculateRepulsionForceOnVertex(vertex.transform.position + vertex.velocity*Time.deltaTime, repulsionDistance);
            Vector3 force = ageFactor * repelForceFactor * bqb.calculateRepulsionForceOnVertex(vertex, repulsionDistance);
            vertex.repelForce = force;
        }
    }

    public int stabilityIterations = 1;
    private void calculateLinkForces(GraphManager graphManager) {
        for (int i = 0; i < stabilityIterations; i++) {
            foreach (Edge edge in graphManager.GetKanten()) {
                float ageFactor = Mathf.Max(1, (10 - 1) * (1 - edge.age)); // Young edges are strong
                Vector3 force = calculateLinkForce(edge);
                edge.startPoint.attractForce += ageFactor * attractForceFactor * force;
                edge.endPoint.attractForce -= ageFactor * attractForceFactor * force;
            }
        }
    }



    public int arrowAverageRecalculationIteration = 10;
    int currentIteration = 0;

    /**
     * Calculates the stress of a vertex. The stress is a measure of how unusual the angles of the vertex are. It is used to visualize weird spots.
     **/
    private void calculateStress(GraphManager graphManager) {
        if (currentIteration == 0) {
            calculateAverageAngles(graphManager);
            currentIteration = arrowAverageRecalculationIteration;
        }
        currentIteration--;

        foreach (GroupElement vertex in graphManager.getVertex()) {
            vertex.stress = 0;
            foreach (char gen1 in generators) {
                foreach (char gen2 in generators) {
                    GroupElement vertex1 = graphManager.followEdge(vertex, gen1);
                    GroupElement vertex2 = graphManager.followEdge(vertex, gen2);

                    if (gen1 != gen2 && vertex1 != null && vertex2 != null && vertex1.name != vertex2.name) {
                        Vector3 force = angleForceFactor * calculateAverageAngleForceBetweenTwoVertices(vertex, vertex1, vertex2, averageArrowAngles[gen1][gen2]);
                        vertex.stress += force.magnitude;
                    }
                }
            }
        }
    }


    /**
     * Goes over each pair of angles and calculates theirs averages. This is done once per recalculationIteration to reduce the number of calculations.
     **/
    private void calculateAverageAngles(GraphManager graphManager) {
        foreach (char gen1 in generators) {
            foreach (char gen2 in generators) {
                countedArrowAngles[gen1][gen2] = 0;
                sumArrowAngles[gen1][gen2] = 0f;
                averageArrowAngles[gen1][gen2] = 0f;

                foreach (GroupElement vertex in graphManager.getVertex()) {
                    GroupElement vertex1 = graphManager.followEdge(vertex, gen1);
                    GroupElement vertex2 = graphManager.followEdge(vertex, gen2);

                    if (vertex1 != null && vertex2 != null && vertex1.id != vertex2.id) {
                        // Thing is calculated twice, this can be optimized
                        sumArrowAngles[gen1][gen2] += Vector3.Angle(vertex1.transform.position - vertex.transform.position, vertex2.transform.position - vertex.transform.position);
                        countedArrowAngles[gen1][gen2] += 1;
                    }
                }

                averageArrowAngles[gen1][gen2] = sumArrowAngles[gen1][gen2] / countedArrowAngles[gen1][gen2];
            }
        }
    }


    private Vector3 calculateAverageAngleForceBetweenTwoVertices(GroupElement vertexBase, GroupElement vertex1, GroupElement vertex2, float averageAngle) {
        Vector3 v = Vector3.Normalize(vertex1.transform.position - vertexBase.transform.position);
        Vector3 w = Vector3.Normalize(vertex2.transform.position - vertexBase.transform.position);

        Vector3 u = v - w;
        Vector3.OrthoNormalize(ref v, ref u); // u become the orthogonal vector

        float angleDifference = averageAngle - Vector3.Angle(v, w);

        return angleDifference / 90 * (angleDifference / 90) * u;
    }


    private Vector3 calculateLinkForce(Edge edge) {
        GroupElement source = edge.startPoint;
        GroupElement target = edge.endPoint;
        Vector3 diff = target.transform.position + target.velocity * Time.deltaTime - source.transform.position - source.velocity * Time.deltaTime;
        // Wenn die zwei Knoten aufeinander liegen, dann bewege sie ein bisschen auseinander.
        if (diff == Vector3.zero) {
            diff = 0.05f * UnityEngine.Random.insideUnitSphere;
        }
        float mag = diff.magnitude;
        return (mag - edge.GetLength()) / mag * diff;
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