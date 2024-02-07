using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

[System.Serializable]
public class RepulsionForce : Force {
    public float repelForceFactor;
    public float barnesQuadTreeMinimalDistance = 0.5f;
    public float barnesQuadTreeMaximalDistance = 50;
    public float QuadTreeTheta = 50; // Used by the QuadTree. Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. 
    private float radius; // The radius of the BarnesQuadtree. This is the maximal distance between two points in the BarnesQuadtree. This is used to calculate the repulsion force.
    public float maximalForce = 10;


    public RepulsionForce(float repelForceFactor, float repulsionDistance, float radius, float maximalForce) {
        this.repelForceFactor = repelForceFactor;
        this.barnesQuadTreeMinimalDistance = repulsionDistance;
        this.radius = radius;
        this.maximalForce = maximalForce;
    }


    public override IEnumerator ApplyForce(GraphManager graphManager, float alpha) {
        if(repelForceFactor == 0 || alpha == 0) yield return null;
        // Every tick the BarnesQuadtree is recalculated. This is expensive but necessary since the vertices move.
        BarnesQuadtree bqb = new BarnesQuadtree(VectorN.Zero(graphManager.getDim()), radius, QuadTreeTheta * QuadTreeTheta, barnesQuadTreeMinimalDistance * barnesQuadTreeMinimalDistance, barnesQuadTreeMaximalDistance * barnesQuadTreeMaximalDistance);
        foreach (Vertex vertex in graphManager.getVertex()) {
            bqb.Add(vertex);
        }
        bqb.BerechneSchwerpunkt();

        int vertexIndex = 0;
        int vertexPerBatch = 200;
        List<Vertex> vertices = graphManager.getVertex();
        while(vertexIndex < vertices.Count) {
            yield return null;
            vertices = graphManager.getVertex(); // Since simulation goes over multiple frames the vertices might have changed, so we have to fetch them every frame.
            for(int i = vertexIndex; i < Math.Min(vertexIndex+vertexPerBatch, vertices.Count); i++) {
                Vertex vertex = vertices[i];
                float ageFactor = Mathf.Max(1, (10 - 1) * (1 - vertex.Age)); // Young vertices are repelled strongly
                Profiler.BeginSample("calculateRepulsionForceOnVertex");
                VectorN force = vertex.Mass * ageFactor * repelForceFactor * alpha * bqb.calculateRepulsionForceOnVertex(vertex);
                Profiler.EndSample();
                force = force.ClampMagnitude(maximalForce);
                vertex.RepelForce = force;
            }
            vertexIndex += vertexPerBatch;
        }
    }
}