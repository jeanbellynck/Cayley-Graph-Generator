using System;
using System.Collections.Generic;
using UnityEngine;


public class BarnesQuadtree {

    // Konstruktorvariablen
    VectorN position;
    float radius; // Radius of the BarnesQuadbaum cube.

    // Baumvariablen
    BarnesQuadtree[] subtrees;
    List<Vertex> points = new List<Vertex>();
    public VectorN centerOfMass;
    public float mass; // Mass is somewhat misleading since the points repel each other.
    float thetaSquared; // Determines how detailed the repulsion calculation is. Setting this to 0.1*radius means that cubes of the size smaller than 10 sitting at the boundary won't bw broken up. Is apparently usually set to a value 0.9
    float minimalDistanceSquared; // Gives a lower bound on the smallest possible cube. This was implemented after the program crashed when two points on the same points caused a recursion loop.
    float maximalDistanceSquared;
    bool isLeaf = true;
    int treeDim = 3; // The dimension can by smaller than the dimension of the vertices. Thin should be ok, since the force always scales to the sqare of the distance.
    int vertexDim;


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

    bool punktInBounds(Vertex vertex) {
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
        centerOfMass = VectorN.Zero(vertexDim);
        if (mass == 0) {
            // This cube is empty. Do nothing.
        }
        else if (isLeaf) {
            // This cube is a leaf, the center of mass is determined by the average of the points.
            foreach (Vertex punkt in points) {
                centerOfMass += punkt.Position * punkt.Mass;
            }
            centerOfMass /= mass;
        }
        else {
            // This cube has benn split up in subcubes. The center of mass is the weighted average of the centers of mass of the subcubes.
            foreach (BarnesQuadtree subtree in subtrees) {
                subtree.BerechneSchwerpunkt();
                centerOfMass += subtree.centerOfMass * subtree.mass;
            }
            centerOfMass /= mass;
        }
    }

    /**
     * Calculates how all vertices inside this cube repel the given point.
     **/
    public VectorN calculateRepulsionForceOnVertex(Vertex pointActedOn) {
        // This cube is empty. It does not repel the point.
        if (mass == 0) return VectorN.Zero(vertexDim);

        VectorN diff = centerOfMass - pointActedOn.Position;
        float distanceSquared = diff.MagnitudeSquared();


        if(punktInBounds(pointActedOn)) {
            if (isLeaf) {
                return calculateRepulsionForceOnVertexInsideLeaf(pointActedOn, diff);
            }
            else {
                return CalculateRepulsionForceOnVertexInsideSubtree(pointActedOn);
            }
        }
        if (distanceSquared > maximalDistanceSquared) {
            return VectorN.Zero(vertexDim);
        }
        if (4 * radius * radius * thetaSquared > distanceSquared) {
            if (isLeaf) {
                return calculateRepulsionForceOnVertexInsideLeaf(pointActedOn, diff);
            }
            else {
                return CalculateRepulsionForceOnVertexInsideSubtree(pointActedOn);
            }
        } else {
            return CalculateForce(diff, distanceSquared);
        }
    }

    /**
     * Calculates how all vertices inside this cube-leaf repel the given point.
     **/
    public VectorN calculateRepulsionForceOnVertexInsideLeaf(Vertex pointActedOn, VectorN diff) {
        VectorN force = VectorN.Zero(vertexDim);
        foreach (Vertex point in points) {
            if (point.Equals(pointActedOn)) continue;
            float distanceSquared = (point.Position - pointActedOn.Position).MagnitudeSquared();
            if (distanceSquared == 0) {
                force += VectorN.Random(treeDim, 0.05f);
            } else {
                force += CalculateForceWithMass(diff, distanceSquared, point.Mass);
            }
        }
        return force;
    }

    public VectorN CalculateRepulsionForceOnVertexInsideSubtree(Vertex pointActedOn) {
        VectorN force = VectorN.Zero(vertexDim);
        foreach (BarnesQuadtree subtree in subtrees) {
            force.Add(subtree.calculateRepulsionForceOnVertex(pointActedOn));
        }
        return force;
    }


    /**
     * Does the same as above but uses the center of mass of the cube instead of a second vertex
     **/
    VectorN CalculateForce(VectorN diff, float distanceSquared) {
        return CalculateForceWithMass(diff, distanceSquared, mass);
    }

    VectorN CalculateForceWithMass(VectorN diff, float distanceSquared, float mass) {
        return diff.Normalize().Multiply((-1) * mass / distanceSquared);
    }

    VectorN quadrantToVector(int[] quadrant) {
        VectorN result = new VectorN(vertexDim);
        for (int i = 0; i < treeDim; i++) {
            result[i] = quadrant[i] == 0 ? -1 : 1;
        }
        return result;
    }
}