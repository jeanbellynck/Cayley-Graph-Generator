using System;
using System.Collections.Generic;
using UnityEngine;


public class GroupEdge : Edge {
    public static readonly IDictionary<char, Color> LabelColours = new Dictionary<char, Color>();
    public override float Activity => (StartPoint is GroupVertex sp) ? sp.Activity : (EndPoint is GroupVertex ep) ? ep.Activity : 1;

    public void Initialize(GroupVertex startVertex, GroupVertex endVertex, char operation, float hyperbolicity) {
        if(char.IsLower(operation)) {
            base.Initialize(startVertex, endVertex, operation);
        } else {
            base.Initialize(endVertex, startVertex, char.ToLower(operation));
        }
        SetFarbe(LabelColours[char.ToLower(operation)], new Color(100, 100, 100));
        calculateEdgeLength(hyperbolicity);
    }

    // Start is called before the first frame update
    void Start() { }

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

    /**
     * This method is used to get the start Vertex of the edge.
     * The result is dependent on whether op is upper or lower case
     */
    public Vertex getStartPoint(char op) {
        if (char.IsLower(op)) {
            return StartPoint;
        }
        else {
            return EndPoint;
        }
    }



    public bool Equals(GroupEdge other) {
        if (other == null) return false;
        return StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint) && Label == other.Label;
    }

    public new GroupVertex getOpposite(GroupVertex opposite) {
        return (GroupVertex) base.getOpposite(opposite);
    }
}
