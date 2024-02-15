// A class used to simulate the force between two vertices.
using System.Collections;
using UnityEngine;

[System.Serializable]
public class LinkForce : Force {
    [SerializeField]
    public float linkForceFactor;
    [SerializeField]
    public int stabilityIterations;

    public LinkForce(float linkForceFactor = 1, int stabilityIterations = 1) {
        this.linkForceFactor = linkForceFactor;
        this.stabilityIterations = stabilityIterations;
    }

    public override IEnumerator ApplyForce(GraphManager graphManager, float alpha) {
        if (linkForceFactor == 0 || alpha == 0) yield return null;
        for (int i = 0; i < stabilityIterations; i++) {
            foreach (Edge edge in graphManager.GetKanten()) {
                VectorN force = calculateLinkForce(edge);
                edge.StartPoint.Force += linkForceFactor * alpha * force;
                edge.EndPoint.Force -= linkForceFactor * alpha * force;
            }
        }
    }

    private VectorN calculateLinkForce(Edge edge) {
        Vertex source = edge.StartPoint;
        Vertex target = edge.EndPoint;
        VectorN diff = (target.Position + target.Velocity) - (source.Position + source.Velocity);
        int dim = diff.Size();
        // Wenn die zwei Knoten aufeinander liegen, dann bewege sie ein bisschen auseinander.
        if (diff.Equals(VectorN.Zero(dim))) {
            diff = VectorN.Random(dim, 0.05f);
        }
        float length = diff.Magnitude();
        VectorN force =  (length - edge.Length) / length * diff;
        return force;
    }
}