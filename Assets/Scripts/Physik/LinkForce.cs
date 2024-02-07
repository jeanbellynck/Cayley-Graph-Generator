// A class used to simulate the force between two vertices.
using System.Collections;
using UnityEngine;

public class LinkForce : Force {
    private float linkForceFactor;

    public LinkForce(float linkForceFactor) {
        this.linkForceFactor = linkForceFactor;
    }

    public int stabilityIterations = 1;

    public override IEnumerator ApplyForce(GraphManager graphManager, float alpha) {
        if (linkForceFactor == 0 || alpha == 0) yield return null;
        for (int i = 0; i < stabilityIterations; i++) {
            foreach (Edge edge in graphManager.GetKanten()) {
                float ageFactor = Mathf.Max(1, (10 - 1) * (1 - edge.age)); // Young edges are strong
                VectorN force = calculateLinkForce(edge);
                edge.StartPoint.LinkForce = edge.StartPoint.LinkForce + ageFactor * linkForceFactor * alpha * force;
                edge.EndPoint.LinkForce = edge.EndPoint.LinkForce - ageFactor * alpha * force;
            }
        }
    }

    private VectorN calculateLinkForce(Edge edge) {
        Vertex source = edge.StartPoint;
        Vertex target = edge.EndPoint;
        VectorN diff = target.Position - source.Position;
        int dim = diff.Size();
        // Wenn die zwei Knoten aufeinander liegen, dann bewege sie ein bisschen auseinander.
        if (diff.Equals(VectorN.Zero(dim))) {
            diff = VectorN.Random(dim, 0.05f);
        }
        float mag = diff.Magnitude();
        // float vertexFactor = Mathf.Sqrt(graphManager.getVertex().Count); // The idea is the following. In a normal graph the edges of a graph grow with the number of vertices. To simulate this effekt the link force is multiplied by the number of vertices.
        return (mag - edge.Length) / mag * diff;
    }
}