using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

[System.Serializable]
public class RepulsionForce : Force {
    [SerializeField]
    public float repelForceFactor;
    [SerializeField]
    public float barnesQuadTreeMinimalDistance;
    [SerializeField]
    public float barnesQuadTreeMaximalDistance;
    [SerializeField]
    public float QuadTreeTheta; // Used by the QuadTree. Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. 
    [SerializeField]
    public float radius; // The radius of the BarnesQuadtree. This is the maximal distance between two points in the BarnesQuadtree. This is used to calculate the repulsion force.
    public float maximalForce;

    public RepulsionForce(float radius, float repelForceFactor = 1.5f, float barnesQuadTreeMinimalDistance = 0.5f, float barnesQuadTreeMaximalDistance = 50, float QuadTreeTheta = 0.9f, float maximalForce = 10) {
        this.repelForceFactor = repelForceFactor;
        this.barnesQuadTreeMinimalDistance = barnesQuadTreeMinimalDistance;
        this.barnesQuadTreeMaximalDistance = barnesQuadTreeMaximalDistance;
        this.QuadTreeTheta = QuadTreeTheta;
        this.radius = radius;
        this.maximalForce = maximalForce;
    }

    public override IEnumerator ApplyForce(GraphManager graphManager, float alpha) {
        if(repelForceFactor == 0 || alpha == 0) yield return null;
        // Every tick the BarnesQuadtree is recalculated. This is expensive but necessary since the vertices move.
        Profiler.BeginSample("QuadtreeCreation");
        int treeDimension = Math.Min(graphManager.getDim(), 3);
        BarnesQuadtree bqb = new BarnesQuadtree(treeDimension, VectorN.Zero(graphManager.getDim()), radius, QuadTreeTheta * QuadTreeTheta, barnesQuadTreeMinimalDistance * barnesQuadTreeMinimalDistance, barnesQuadTreeMaximalDistance * barnesQuadTreeMaximalDistance);
        Profiler.EndSample();
        Profiler.BeginSample("AddVerticesToQuadtree");
        foreach (Vertex vertex in graphManager.getVertex()) {
            bqb.Add(vertex);
        }
        Profiler.EndSample();
        Profiler.BeginSample("BerechneSchwerpunkt");
        bqb.BerechneSchwerpunkt();
        Profiler.EndSample();

        int vertexIndex = 0;
        int vertexPerBatch = 400;
        List<Vertex> vertices = graphManager.getVertex();
        while(vertexIndex < vertices.Count) {
            yield return null;
            vertices = graphManager.getVertex(); // Since simulation goes over multiple frames the vertices might have changed, so we have to fetch them every frame.
            for(int i = vertexIndex; i < Math.Min(vertexIndex+vertexPerBatch, vertices.Count); i++) {
                Vertex vertex = vertices[i];
                Profiler.BeginSample("calculateRepulsionForceOnVertex");
                VectorN force = vertex.Mass * repelForceFactor * alpha * bqb.calculateRepulsionForceOnVertex(vertex);
                Profiler.EndSample();
                force = force.ClampMagnitude(maximalForce);
                vertex.Force += force;
            }
            vertexIndex += vertexPerBatch;
        }
    }
}