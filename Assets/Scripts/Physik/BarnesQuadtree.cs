using System.Collections.Generic;
using UnityEngine;


public class BarnesQuadtree {

    // Konstruktorvariablen
    private Vector3 position;
    private float radius; // Radius of the BarnesQuadbaum cube.

    // Baumvariablen
    private BarnesQuadtree not;
    private BarnesQuadtree nwt;
    private BarnesQuadtree swt;
    private BarnesQuadtree sot;
    private BarnesQuadtree nob;
    private BarnesQuadtree nwb;
    private BarnesQuadtree swb;
    private BarnesQuadtree sob;
    private List<Vertex> points = new List<Vertex>();
    public Vector3 schwerpunkt;
    public float mass; // Mass is somewhat misleading since the points repel each other.
    private float precision; // Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. 
    private float maximalCubeSize; // Gives a lower bound on the smallest possible cube. This was implemented after the program crashed when two points on the same points caused a recursion loop.
    private bool isLeaf = true;

    public BarnesQuadtree(Vector3 position, float radius, float präzision, float maximalCubeSize) {
        this.position = position;
        this.radius = radius;
        this.precision = präzision;
        this.maximalCubeSize = maximalCubeSize;
        this.mass = 0;
    }

    public void Add(List<Vertex> points) {
        foreach (Vertex punkt in points) {
            Add(punkt);
        }
    }

    /** 
     * Originally this took points instead of vertices as a parameter. However, this has been changed because I wanted to compare points by id and not by value (which could shift after calculations).
     **/
    public void Add(Vertex punkt) {
        if (punktInBounds(punkt)) {
            // If this cube is empty or splitting it would result in a cube smaller than maximalCubeSize, then the point is added to this cube.
            if (mass == 0 || radius < maximalCubeSize) {
                points.Add(punkt);
            }
            else {
                if (isLeaf) {
                    // This cube is a leaf. The cube is split up. All points inside this cube are added to the subcubes.
                    Teile();
                }
                not.Add(punkt);
                nwt.Add(punkt);
                swt.Add(punkt);
                sot.Add(punkt);
                nob.Add(punkt);
                nwb.Add(punkt);
                swb.Add(punkt);
                sob.Add(punkt);
            }
            mass += punkt.getMass();
        }
    }

    public void Teile() {
        isLeaf = false;
        not = new BarnesQuadtree(position + radius / 2 * (Vector3.up + Vector3.right + Vector3.forward), radius / 2, precision, maximalCubeSize);
        nwt = new BarnesQuadtree(position + radius / 2 * (Vector3.up + Vector3.left + Vector3.forward), radius / 2, precision, maximalCubeSize);
        swt = new BarnesQuadtree(position + radius / 2 * (Vector3.down + Vector3.left + Vector3.forward), radius / 2, precision, maximalCubeSize);
        sot = new BarnesQuadtree(position + radius / 2 * (Vector3.down + Vector3.right + Vector3.forward), radius / 2, precision, maximalCubeSize);
        nob = new BarnesQuadtree(position + radius / 2 * (Vector3.up + Vector3.right + Vector3.back), radius / 2, precision, maximalCubeSize);
        nwb = new BarnesQuadtree(position + radius / 2 * (Vector3.up + Vector3.left + Vector3.back), radius / 2, precision, maximalCubeSize);
        swb = new BarnesQuadtree(position + radius / 2 * (Vector3.down + Vector3.left + Vector3.back), radius / 2, precision, maximalCubeSize);
        sob = new BarnesQuadtree(position + radius / 2 * (Vector3.down + Vector3.right + Vector3.back), radius / 2, precision, maximalCubeSize);
        not.Add(points);
        nwt.Add(points);
        swt.Add(points);
        sot.Add(points);
        nob.Add(points);
        nwb.Add(points);
        swb.Add(points);
        sob.Add(points);
        points = null;
    }

    private bool punktInBounds(Vertex vertex) {
        Vector3 punkt = vertex.transform.position;
        return punkt.x >= position.x - radius &&
        punkt.x < position.x + radius &&
        punkt.y >= position.y - radius &&
        punkt.y < position.y + radius &&
        punkt.z >= position.z - radius &&
        punkt.z < position.z + radius;
    }

    /**
     * Calculates the center of mass of this cube as well as all subcubes.
     **/
    public void BerechneSchwerpunkt() {
        if (mass == 0) {
            // This cube is empty. Do nothing.
        }
        else if (isLeaf) {
            // This cube is a leaf, the center of mass is determined by the average of the points.
            schwerpunkt = Vector3.zero;
            foreach (Vertex punkt in points) {
                schwerpunkt += punkt.transform.position;
            }
            schwerpunkt /= mass;
        }
        else {
            // This cube has benn split up in subcubes. The center of mass is the weighted average of the centers of mass of the subcubes.
            not.BerechneSchwerpunkt();
            nwt.BerechneSchwerpunkt();
            swt.BerechneSchwerpunkt();
            sot.BerechneSchwerpunkt();
            nob.BerechneSchwerpunkt();
            nwb.BerechneSchwerpunkt();
            swb.BerechneSchwerpunkt();
            sob.BerechneSchwerpunkt();
            schwerpunkt = (not.schwerpunkt * not.mass +
                nwt.schwerpunkt * nwt.mass +
                sot.schwerpunkt * sot.mass +
                swt.schwerpunkt * swt.mass +
                nob.schwerpunkt * nob.mass +
                nwb.schwerpunkt * nwb.mass +
                sob.schwerpunkt * sob.mass +
                swb.schwerpunkt * swb.mass) / mass;
        }
    }

    /**
     * Calculates how all vertices inside this cube repel the given point.
     **/
    public Vector3 calculateRepulsionForceOnVertex(Vertex pointActedOn, float maximalRepulsionDistance) {
        if (mass == 0) {
            // This cube is empty. It does not repel the point.
            return Vector3.zero;
        }
        else if (isLeaf) {
            // This cube is a leaf. If this is different point, there will be repelling.
            //return Vector3.zero;
            return calculateRepulsionForceOnVertexInsideLeaf(pointActedOn);
        }
        else {
            // This cube contains more than one point. The cube is split up. The repulsion force is the sum of the repulsion forces of the subcubes.

            // If repulsionDistance is a positive number, Ignore points that are far away.
            if (maximalRepulsionDistance != 0 && Vector3.Distance(pointActedOn.transform.position, schwerpunkt) > maximalRepulsionDistance) {
                return Vector2.zero;
            }
            // If this cube is small and far away, then the cube is not split up.
            // 
            if (2 * radius / Vector3.Distance(pointActedOn.transform.position, schwerpunkt) < precision && !punktInBounds(pointActedOn)) {
                return mass * BerechneKraft(pointActedOn);
            }
            else {
                return not.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance) + nwt.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance)
                    + sot.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance) + swt.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance)
                    + nob.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance) + nwb.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance)
                    + sob.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance) + swb.calculateRepulsionForceOnVertex(pointActedOn, maximalRepulsionDistance);
            }
        }
    }

    /**
     * Calculates how all vertices inside this cube-leaf repel the given point.
     **/
    public Vector3 calculateRepulsionForceOnVertexInsideLeaf(Vertex pointActedOn) {
        // If the pointActedOn is in bound, then the calculation must exclude the pointActedOn.
        Vector3 force = Vector3.zero;
        foreach (Vertex point in points) {
            if (!point.Equals(pointActedOn)) {
                // Look a step into the future to calculate the force.
                force += BerechneKraft(pointActedOn, point);
            }
        }
        return force;
    }

    private Vector3 BerechneKraft(Vertex bewirkter, Vertex wirkender) {
        Vector3 diff = wirkender.transform.position - bewirkter.transform.position;
        float distance = diff.magnitude;
        if (distance == 0) {
            return Vector3.zero;
        }
        else {

            return -diff.normalized * (bewirkter.getMass() * wirkender.getMass()) / (distance * distance);
        }
    }

    /**
     * Does the same as above but uses the center of mass of the cube instead of a second vertex
     **/
    private Vector3 BerechneKraft(Vertex bewirkter) {
        Vector3 diff = schwerpunkt - bewirkter.transform.position;
        float distance = diff.magnitude;
        if (distance == 0) {
            return Vector3.zero;
        }
        else {
            return -diff.normalized * (bewirkter.getMass() * mass) / (distance * distance);
        }
    }
}