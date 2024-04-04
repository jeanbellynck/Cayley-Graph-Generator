using System;
using UnityEngine;


public class GroupEdge : Edge {

    public void Initialize(GroupVertex startVertex, GroupVertex endVertex, char operation, float hyperbolicity, LabelledGraphManager graphManager) {

        if (!char.IsLower(operation)) {
            (startVertex, endVertex) = (endVertex, startVertex);
            operation = char.ToLower(operation);
        }

        base.Initialize(startVertex, endVertex, operation);

        calculateEdgeLength(hyperbolicity);
    }

    public void calculateEdgeLength(float hyperbolicity) {
        int distance = Math.Min(((GroupVertex) StartPoint).DistanceToNeutralElement, ((GroupVertex) EndPoint).DistanceToNeutralElement);
        Length = Mathf.Pow(hyperbolicity, distance);

        /**
        List<string> pathsToIdentity1 = v1.PathsToNeutralElement;
        List<string> pathsToIdentity2 = v2.PathsToNeutralElement;
        float length = float.MaxValue;
        foreach (string path in pathsToIdentity1) {
            float lengthCandidate = calculateScalingForGenerator(generator, path);
            if (lengthCandidate < length) {
                length = lengthCandidate;
            }
        }
        foreach (string path in pathsToIdentity2) {
            float lengthCandidate = calculateScalingForGenerator(generator, path);
            if (lengthCandidate < length) {
                length = lengthCandidate;
            }
        }
        return length;**/
    }
    
    public bool Equals(GroupEdge other) {
        if (other == null) return false;
        return StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint) && Label == other.Label;
    }

    public GroupVertex GetOpposite(GroupVertex opposite) {
        return (GroupVertex) base.GetOpposite(opposite);
    }
}
