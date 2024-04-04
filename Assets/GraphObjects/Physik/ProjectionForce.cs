using System.Collections;
using UnityEngine;

[System.Serializable]
public class ProjectionForce : Force {
    [SerializeField]
    public float projectionForceFactor;
    public int dim;

    public ProjectionForce(float projectionForceFactor = 0.5f, int dim = 2) {
        this.projectionForceFactor = projectionForceFactor;
        this.dim = dim;
    }

    public override IEnumerator ApplyForce(LabelledGraphManager graphManager, float alpha) {
        if(projectionForceFactor == 0 || alpha == 0) yield return null;
        
        foreach(Vertex vertex in graphManager.getVertices()) {
            vertex.Force += projectionForceFactor * vectorToHyperplane(vertex.Position);
        }
    }

    private VectorN vectorToHyperplane(VectorN position) {
        VectorN result = VectorN.Zero(position.Size());
        for(int i = dim; i < position.Size(); i++) {
            result[i] = -position[i];
        }
        return result;
    }
}