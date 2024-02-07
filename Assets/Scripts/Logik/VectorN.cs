using System;
using System.Drawing;
using System.Numerics;
using UnityEngine;

public struct VectorN {
    [SerializeField]
    public float[] values;
    public int size;

    public VectorN(float[] values) {
        this.values = values;
        this.size = values.Length;
    }

    public VectorN(int size) {
        this.values = new float[size];
        this.size = size;
    }

    public VectorN(VectorN vector) {
        values = new float[vector.values.Length];
        for (int i = 0; i < vector.values.Length; i++) {
            values[i] = vector.values[i];
        }
        size = vector.size;
    }

    // New constructor for performing operations
    public VectorN(in VectorN a, in VectorN b, Func<float, float, float> operation) {
        if (a.size != b.size) {
            throw new ArgumentException("Vectors must have the same size");
        }
        size = a.size;
        values = new float[size];
        for (int i = 0; i < size; i++) {
            values[i] = operation(a.values[i], b.values[i]);
        }
    }

    public float this[int index] {
        get {
            return values[index];
        }
        set {
            values[index] = value;
        }
    }

    public int Size() {
        return size;
    }

    public float MagnitudeSquared() {
        float sum = 0;
        for (int i = 0; i < values.Length; i++) {
            sum += values[i] * values[i];
        }
        return sum;
    }

    public float Magnitude() {
        return (float) Math.Sqrt(MagnitudeSquared());
    }

    public VectorN Normalize() {
        float length = Magnitude();
        float reciprocal = 1.0f / length;
        for (int i = 0; i < values.Length; i++) {
            values[i] *= reciprocal;
        }
        return this;
    }

    public VectorN Add(VectorN b) {
        if (size != b.Size()) {
            throw new ArgumentException("Vectors must have the same size");
        }
        for (int i = 0; i < size; i++) {
            values[i] += b[i];
        }
        return this;
    }

    public VectorN Multiply(float b) {
        for (int i = 0; i < size; i++) {
            values[i] *= b;
        }
        return this;
    }

    // Updated operator overloads
    public static VectorN operator +(in VectorN a, in VectorN b) {
        return new VectorN(a, b, (x, y) => x + y);
    }

    public static VectorN operator -(in VectorN a, in VectorN b) {
        return new VectorN(a, b, (x, y) => x - y);
    }

    public static VectorN operator *(VectorN a, float b) {
        VectorN result = new VectorN(a.Size());
        for (int i = 0; i < a.Size(); i++) {
            result[i] = a[i] * b;
        }
        return result;
    }

    public static VectorN operator /(VectorN a, float b) {
        VectorN result = new VectorN(a.Size());
        for (int i = 0; i < a.Size(); i++) {
            result[i] = a[i] / b;
        }
        return result;
    }

    public static VectorN operator *(float a, VectorN b) {
        return b * a;
    }

    public bool Equals(VectorN b) {
        for (int i = 0; i < size; i++) {
            if (values[i] != b[i]) {
                return false;
            }
        }
        return true;
    }

    public static UnityEngine.Vector3 ToVector3(VectorN a) {
        switch (a.Size()) {
            case 0:
                return new UnityEngine.Vector3(0, 0, 0);
            case 1:
                return new UnityEngine.Vector3(a[0], 0, 0);
            case 2:
                return new UnityEngine.Vector3(a[0], a[1], 0);
            default:
                return new UnityEngine.Vector3(a[0], a[1], a[2]);
        }
    }

    public VectorN ClampMagnitude(float max) {
        float magSquared = MagnitudeSquared();
        if (magSquared > max * max) {
            return Normalize() * max;
        }
        else {
            return this;
        }
    }


    public static VectorN Zero(int dim) {
        return new VectorN(dim);
    }


    public static VectorN Random(int dim, float radius) {
        VectorN result = new VectorN(dim);
        for (int i = 0; i < dim; i++) {
            result[i] = UnityEngine.Random.Range(-radius, radius);
        }
        if(result.MagnitudeSquared() == 0) {
            return Random(dim, radius);
        }
        return result;
    }

    public static float DistanceSquared(VectorN a, VectorN b) {
        if (a.Size() != b.Size()) {
            throw new ArgumentException("Vectors must have the same size");
        }
        float sum = 0;
        for (int i = 0; i < a.Size(); i++) {
            sum += (a[i] - b[i]) * (a[i] - b[i]);
        }
        return sum;
    }

    public static float Distance(VectorN a, VectorN b) {
        return (float) Math.Sqrt(DistanceSquared(a, b));
    }
}