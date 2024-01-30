using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vertex : MonoBehaviour
{
    // Used by Link force algorithm
    public Vector3 velocity = Vector3.zero;
    
    // The following is used in v1 and v2
    public Vector3 repelForce = Vector3.zero;
    public Vector3 attractForce = Vector3.zero;
    public Vector3 oppositeForce = Vector3.zero;
    public Vector3 angleForce = Vector3.zero;
    public float stress; // Measures how unusual the angles of the vertex are. It is used to visualize weird spots.
    
    public int distance = 0; // Measures the distance to the identity
    private Renderer mr;

    // The following is only used in v2 
    public int id;
    public Dictionary<char, List<Kante>> edges = new Dictionary<char, List<Kante>>(); // This is a list-dictionary as vertices may temporarily have zero or multiple arrows of a specific generator.
    public int distanceToNeutralElement = 0; // This is the distance to the neutral element of the group. It is used to determine the distance to the neutral element of the group. Currently this is not properly updated.
    //public Knoten predecessorToNeutral = null; // This is the predecessor of the neutral element. It is used to find the path to the neutral element of the group.

    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update() {
        /**
        Vector3 force = repelForce + attractForce + oppositeForce + angleForce;
        force = Vector3.ClampMagnitude(force, maximalForce);

        transform.position += velocity * Time.deltaTime;
        velocity = velocity + force;
        transform.position = Vector3.ClampMagnitude(transform.position, 100);
        velocity *= velocityDecay;
        **/

        //DrawCircle(radius);
        mr.material.color = new Color(stress, 0, 0);
    }

    /**
    * Used for debugging.
    */
    void OnDrawGizmos() {
        /**
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + oppositeForce);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + angleForce);
        **/
    }


    public void addEdge(Kante k) {
        char op = ' ';
        if(k.startPoint.Equals(this)) {
            op = k.getGenerator();
        } else if (k.endPoint.Equals(this)) {
            op = char.ToUpper(k.getGenerator());
        }
        if(!edges.ContainsKey(op)) {
            edges.Add(op, new List<Kante>());
        }
        edges[op].Add(k);
    }


    public Dictionary<char, List<Kante>> getEdges() {
        return edges;
    }
}
