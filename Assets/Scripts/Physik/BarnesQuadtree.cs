using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;


public class BarnesQuadtree {

    // Konstruktorvariablen
    private VectorN position;
    private float radius; // Radius of the BarnesQuadbaum cube.

    // Baumvariablen
    private BarnesQuadtree[] subtrees;
    private List<Vertex> points = new List<Vertex>();
    public VectorN schwerpunkt;
    public float mass; // Mass is somewhat misleading since the points repel each other.
    private float thetaSquared; // Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. Is apparently usually set to a value 0.9
    private float minimalDistanceSquared; // Gives a lower bound on the smallest possible cube. This was implemented after the program crashed when two points on the same points caused a recursion loop.
    private float maximalDistanceSquared;
    private bool isLeaf = true;
    private int treeDim = 3; // The dimension can by smaller than the dimension of the vertices. Thin should be ok, since the force always scales to the sqare of the distance.
    private int vertexDim;

    public BarnesQuadtree(int dimension, VectorN position, float radius, float theta, float minimalDistanceSquared, float maximalDistanceSquared) {
        this.treeDim = dimension;
        this.position = position;
        this.radius = radius;
        this.thetaSquared = theta;
        this.minimalDistanceSquared = minimalDistanceSquared;
        this.maximalDistanceSquared = maximalDistanceSquared;
        this.mass = 0;
        vertexDim = position.Size();
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
            if (mass == 0 || radius < minimalDistanceSquared) {
                points.Add(punkt);
            }
            else {
                if (isLeaf) {
                    // This cube is a leaf. The cube is split up. All points inside this cube are added to the subcubes.
                    Teile();
                }
                foreach (BarnesQuadtree subtree in subtrees) {
                    subtree.Add(punkt);
                }
            }
            mass += punkt.Mass;
        }
    }

    public void Teile() {
        isLeaf = false;
        subtrees = new BarnesQuadtree[(int)Mathf.Pow(2, treeDim)];
        for (int i = 0; i < subtrees.Length; i++) {
            int[] quadrant = new int[treeDim];
            for (int j = 0; j < treeDim; j++) {
                quadrant[j] = i / (int)Mathf.Pow(2, j) % 2;
            }
            subtrees[i] = new BarnesQuadtree(treeDim, position + radius / 2 * quadrantToVector(quadrant), radius / 2, thetaSquared, minimalDistanceSquared, maximalDistanceSquared);
            subtrees[i].Add(points);
        }
        points = null;
    }

    private bool punktInBounds(Vertex vertex) {
        VectorN punkt = vertex.Position;
        if(punkt.IsNaN) {
            return false;
        }
        for (int i = 0; i < treeDim; i++) {
            if (punkt[i] < position[i] - radius || punkt[i] >= position[i] + radius) {
                return false;
            }
        }
        return true;
    }

    /**
     * Calculates the center of mass of this cube as well as all subcubes.
     **/
    public void BerechneSchwerpunkt() {
        schwerpunkt = VectorN.Zero(vertexDim);
        if (mass == 0) {
            // This cube is empty. Do nothing.
        }
        else if (isLeaf) {
            // This cube is a leaf, the center of mass is determined by the average of the points.
            foreach (Vertex punkt in points) {
                schwerpunkt += punkt.Position * punkt.Mass;
            }
            schwerpunkt /= mass;
        }
        else {
            // This cube has benn split up in subcubes. The center of mass is the weighted average of the centers of mass of the subcubes.
            foreach (BarnesQuadtree subtree in subtrees) {
                subtree.BerechneSchwerpunkt();
                schwerpunkt += subtree.schwerpunkt * subtree.mass;
            }
            schwerpunkt /= mass;
        }
    }

    /**
     * Calculates how all vertices inside this cube repel the given point.
     **/
    public VectorN calculateRepulsionForceOnVertex(Vertex pointActedOn) {
        // This cube is empty. It does not repel the point.
        if (mass == 0) return VectorN.Zero(vertexDim);

        VectorN diff = schwerpunkt - pointActedOn.Position;
        float distanceSquared = diff.MagnitudeSquared();

        // This cube sits too far 
        if (distanceSquared > maximalDistanceSquared) return VectorN.Zero(vertexDim);

        // If the points are far away, use the Barnes Hut approximation, else process them directly
        if (4 * radius * radius * thetaSquared < distanceSquared) {
            return CalculateForce(diff, distanceSquared);
        }
        else {
            if (isLeaf) {
                return calculateRepulsionForceOnVertexInsideLeaf(pointActedOn, diff, distanceSquared);
            }
            else {
                return calculateRepulsionForceOnVertexInsideSubtree(pointActedOn);
            }
        }
    }

    /**
     * Calculates how all vertices inside this cube-leaf repel the given point.
     **/
    public VectorN calculateRepulsionForceOnVertexInsideLeaf(Vertex pointActedOn, VectorN diff, float distanceSquared) {
        // Leaves are either of minimalSize or contain only one point
        if (points.Count == 1) {
            // This cube contains the point itself and can be ignored
            if (points[0].Equals(pointActedOn)) return VectorN.Zero(vertexDim);
        }
        if (distanceSquared > minimalDistanceSquared) {
            return CalculateForce(diff, distanceSquared);
        } else if (distanceSquared == 0) {
            return VectorN.Random(treeDim, 0.05f);
        } else {
            return CalculateForce(diff, distanceSquared);
        }
    }

    public VectorN calculateRepulsionForceOnVertexInsideSubtree(Vertex pointActedOn) {
        VectorN force = VectorN.Zero(vertexDim);
        foreach (BarnesQuadtree subtree in subtrees) {
            force.Add(subtree.calculateRepulsionForceOnVertex(pointActedOn));
        }
        return force;
    }


    /**
     * Does the same as above but uses the center of mass of the cube instead of a second vertex
     **/
    private VectorN CalculateForce(VectorN diff, float distanceSquared) {
        int power = Math.Max(diff.Size()-1, 0);
        float distance = Mathf.Sqrt(distanceSquared);
        return diff.Normalize().Multiply((-1) * mass / Mathf.Pow(distance, power));
    }

    private VectorN quadrantToVector(int[] quadrant) {
        VectorN result = new VectorN(vertexDim);
        for (int i = 0; i < treeDim; i++) {
            result[i] = quadrant[i] == 0 ? -1 : 1;
        }
        return result;
    }
}