using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupEdge : Edge {
    public static IDictionary<char, Color> generatorColours;

    public void Initialize(GroupVertex startVertex, GroupVertex endVertex, char operation) {
        if(char.IsLower(operation)) {
            base.Initialize(startVertex, endVertex, operation);
        } else {
            base.Initialize(endVertex, startVertex, char.ToLower(operation));
        }
        SetFarbe(generatorColours[char.ToLower(operation)], new Color(100, 100, 100));
        calculateEdgeLength((GroupVertex) StartPoint, (GroupVertex) EndPoint, char.ToLower(operation));
    }

    // Start is called before the first frame update
    void Start() { }

    public void calculateEdgeLength(GroupVertex startVertex, GroupVertex endVertex, char operation) {
        Length = 1;
        //(endVertex.transform.position - startVertex.transform.position).magnitude;
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


    /**
    public void SetEndpoints(Vertex startPoint, Vertex endPoint, char generator) {
        if (this.StartPoint != null || this.EndPoint != null) {
            throw new Exception("The Endpoints of an Edge are final and should not be changed. Create a new Edge instead");
        }
        if (char.IsLower(generator)) {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
        }
        else {
            this.StartPoint = endPoint;
            this.EndPoint = startPoint;
        }
        this.Label = char.ToLower(generator);

        name = StartPoint.name + " --" + Label + "-> " + EndPoint.name;
        StartPoint.addEdge(this);
        EndPoint.addEdge(this);
        Update();
    }**/

    public void SetFarbe(Color farbe1, Color farbe2) {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.startColor = farbe1;
        //lr.endColor = farbe2;
    }

    
    public bool Equals(GroupEdge other) {
        return StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint) && Label == other.Label;
    }

    public GroupVertex getOpposite(GroupVertex opposite) {
        return (GroupVertex) base.getOpposite(opposite);
    }
}
