using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vertex : MonoBehaviour {
    private Vector3 previousPosition; // This is the previous position of the vertex. It is used to calculate the forces using the improved euler method.
    private int id;
    private float age = 0;
    [SerializeField]
    private float mass = 1; // The mass of the vertex. This is used to calculate the repulsion force. It depends on the hyperbolicity and the distance to the neutral element.
    private Vector3 velocity = Vector3.zero;
    private Vector3 repelForce = Vector3.zero;
    private Vector3 linkForce = Vector3.zero;
    private List<Edge> edges = new List<Edge>();
    
    private Renderer mr;

    public Vector3 PreviousPosition { get => previousPosition; set => previousPosition = value; }
    public int Id { get => id; set => id = value; }
    public float Mass { get => mass; set => mass = value; }
    public float Age { get => age; set => age = value; }
    public Vector3 Velocity { get => velocity; set => velocity = value; }
    public Vector3 RepelForce { get => repelForce; set => repelForce = value; }
    public Vector3 LinkForce { get => linkForce; set => linkForce = value; }
    public List<Edge> Edges { get => edges; set => edges = value; }
    public Renderer Mr { get => mr; set => mr = value; }

    // Start is called before the first frame update
    public virtual void Start() {
        if(mass == 0) {
            mass = 0.1f;
        }
        mr = GetComponent<Renderer>();
    }

    // Update is called once per frame
    public virtual void Update() {
        age += Time.deltaTime;
    }

    
    /**
    * Used for debugging.
    */
    void OnDrawGizmos() {
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, transform.position + repelForce);
        //Gizmos.color = Color.cyan;
        //Gizmos.DrawLine(transform.position, transform.position + angleForce);
    }

    public void Destroy() {
        Destroy(gameObject);
    }

    public bool Equals(GroupElement other) {
        return Id == other.Id;
    }
}
