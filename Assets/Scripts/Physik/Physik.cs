using UnityEngine;
using System.Collections.Generic;
using System;

public class Physik : MonoBehaviour{
    public float idealeLänge;
    public float radius;
    public float präzision;

    [Range(0.0f,20.0f)]
    public float repelForceFactor; 
    [Range(0.0f,20.0f)]
    public float attractForceFactor;
    [Range(0.0f,20.0f)]
    public float oppositeForceFactor;
    [Range(0.0f,20.0f)]
    public float angleForceFactor;
    public float maximalForce = 1;
    char[] generators;
    Dictionary<char, Dictionary<char, float>> sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
    // Necessary since if you dont count to many the average is too small
    Dictionary<char, Dictionary<char, int>> countedArrowAngles = new Dictionary<char, Dictionary<char, int>>(); 
    Dictionary<char, Dictionary<char, float>> averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();

    
    public Physik(float idealeLänge, float radius) {
        this.idealeLänge = idealeLänge;
        this.radius = radius;
    }

    public void setGenerators(char[] generators) {
        this.generators = new char[generators.Length*2];
        for(int i = 0; i < generators.Length; i++) {
            this.generators[i] = generators[i];
            this.generators[i+generators.Length] = char.ToUpper(generators[i]);
        }

        sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
        countedArrowAngles = new Dictionary<char, Dictionary<char, int>>();
        averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();

        foreach(char gen1 in this.generators) {
            sumArrowAngles[gen1] = new Dictionary<char, float>();
            countedArrowAngles[gen1] = new Dictionary<char, int>();
            averageArrowAngles[gen1] = new Dictionary<char, float>();
        }
    }


    public void UpdatePh(Knotenverwalter knotenV, Kantenverwalter kantenV, float präzision) {
        this.präzision = präzision;
        geschwindigkeitenZurücksetzen(knotenV);
        if(repelForceFactor != 0) knotenKraftBerechnen(knotenV);
        if(attractForceFactor != 0) kantenKraftBerechnen(kantenV);
        if(oppositeForceFactor != 0) calculateOppositionForce(knotenV, kantenV);
        if(angleForceFactor != 0) calculateArrowAverageForce(knotenV, kantenV);
        
        //smitteKraftBerechnen(knotenV, kantenV); 
        //fixIdentity(knotenV);
    }

    
    /**
     * Sets the speed of all vertices to 0.
     * Also bounds th force by the maximal force.
     */
    private void geschwindigkeitenZurücksetzen(Knotenverwalter knotenV) {
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            knoten.attractForce = Vector3.zero;
            knoten.repelForce = Vector3.zero;
            knoten.oppositeForce = Vector3.zero;
            knoten.angleForce = Vector3.zero;
            knoten.maximalForce = maximalForce;
        }
    }

    private void knotenKraftBerechnen(Knotenverwalter knotenV) {
        // Abstoßung durch Knoten
        BarnesQuadbaum bqb = new BarnesQuadbaum(Vector2.zero, radius, präzision);
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            bqb.Add(knoten.transform.position);
        }
        bqb.BerechneSchwerpunkt();
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            knoten.repelForce = Vector3.zero;
            knoten.repelForce = repelForceFactor * bqb.BerechneKraftAufKnoten(knoten.transform.position);
        }
    }

    private void kantenKraftBerechnen(Kantenverwalter kantenVerwalter) {
        // Anziehung durch Graphen
        foreach(Kante kante in kantenVerwalter.GetKanten()) {
            Vector3 kraft = kantenKraftBerechnen(kante.startPoint, kante.endPoint) ;
            kante.startPoint.attractForce += attractForceFactor * kraft;
            kante.endPoint.attractForce -= attractForceFactor * kraft;
        }
    }

    /**
     * Calculates a force that makes two neighboring vertices opposite two each other if they can be reached with inverse elements. 
     * The force is 0 if the vertices are opposite of if following both arrows lead to the same element
     * The force is 1 if the angle between the vertices is very small 
     * ToDO: Check, when input and output are the same. Normalize the force.
     */
    private void calculateOppositionForce(Knotenverwalter knotenV, Kantenverwalter kantenV) {
        // ToDo: Da ist noch eine Menge manueller Code drin.
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            char[] alphabet = {'a', 'b'}; 
            
            foreach(char gen in alphabet) {
                Knoten front = kantenV.kanteFolgen(knoten, gen);
                Knoten back = kantenV.kanteFolgen(knoten, char.ToUpper(gen));
                if(front != null && back != null) {
                    front.oppositeForce += 0.25f * oppositeForceFactor*calculateOppositionForceBetweenTwoVertices(knoten, front, back);
                    back.oppositeForce += 0.25f * oppositeForceFactor*calculateOppositionForceBetweenTwoVertices(knoten, back, front);
                    knoten.oppositeForce += 0.5f * oppositeForceFactor * ((front.transform.position -knoten.transform.position).normalized + (back.transform.position -knoten.transform.position).normalized)/2;
                }
            }
        } 
    }
    
    private Vector3 calculateOppositionForceBetweenTwoVertices(Knoten vertexBase, Knoten vertex1, Knoten vertex2) {
        if(vertex1.name == vertex2.name) {return Vector3.zero;}
        
        Vector3 v = Vector3.Normalize(vertex1.transform.position - vertexBase.transform.position);
        Vector3 w = Vector3.Normalize(vertex2.transform.position - vertexBase.transform.position);

        Vector3 u = v-w;
        Vector3.OrthoNormalize(ref v, ref u); 



        return (1 + Vector3.Dot(v, w))/2 * u;
    }

    
    public int recalculationIteration = 10;
    int currentIteration = 0;

    private void calculateArrowAverageForce(Knotenverwalter knotenV, Kantenverwalter kantenV) {
        if(currentIteration == recalculationIteration) {
            calculateAverageAngles(knotenV, kantenV);
            currentIteration = 0;
        }
        currentIteration++;

        foreach(Knoten knoten in knotenV.GetKnoten()) {
            knoten.stress = 0;
            foreach(char gen1 in generators) {
                foreach(char gen2 in generators) {
                    Knoten vertex1 = kantenV.kanteFolgen(knoten, gen1);
                    Knoten vertex2 = kantenV.kanteFolgen(knoten, gen2);
                    
                    if(gen1 != gen2 && vertex1 != null && vertex2 != null && vertex1.name != vertex2.name) {
                        Vector3 force = angleForceFactor*calculateAverageAngleForceBetweenTwoVertices(knoten, vertex1, vertex2, averageArrowAngles[gen1][gen2]);
                        vertex1.angleForce += 1 / (1+knoten.distance) * force;
                        knoten.stress += force.magnitude;
                    }
                }
            }
        } 
    }


    /**
     * Goes over each pair of angles and calculates theirs averages. This is done once per recalculationIteration to reduce the number of calculations.
     **/
    private void calculateAverageAngles(Knotenverwalter knotenV, Kantenverwalter kantenV) {
        foreach(char gen1 in generators) {
            foreach(char gen2 in generators) {
                countedArrowAngles[gen1][gen2] = 0;
                sumArrowAngles[gen1][gen2] = 0f;
                averageArrowAngles[gen1][gen2] = 0f;

                foreach(Knoten knoten in knotenV.GetKnoten()) {
                    Knoten vertex1 = kantenV.kanteFolgen(knoten, gen1);
                    Knoten vertex2 = kantenV.kanteFolgen(knoten, gen2);
                    
                    if(vertex1 != null && vertex2 != null && vertex1.id != vertex2.id) {
                        // Thing is calculated twice, this can be optimized
                        sumArrowAngles[gen1][gen2] += Vector3.Angle(vertex1.transform.position - knoten.transform.position, vertex2.transform.position - knoten.transform.position);
                        countedArrowAngles[gen1][gen2] += 1;
                    }
                }

                averageArrowAngles[gen1][gen2] = sumArrowAngles[gen1][gen2] / countedArrowAngles[gen1][gen2];             
            }
        }
    }

    
    private Vector3 calculateAverageAngleForceBetweenTwoVertices(Knoten vertexBase, Knoten vertex1, Knoten vertex2, float averageAngle) {
        Vector3 v = Vector3.Normalize(vertex1.transform.position - vertexBase.transform.position);
        Vector3 w = Vector3.Normalize(vertex2.transform.position - vertexBase.transform.position);

        Vector3 u = v-w;
        Vector3.OrthoNormalize(ref v, ref u); // u become the orthogonal vector

        float angleDifference = averageAngle - Vector3.Angle(v, w);

        return (angleDifference/90)*(angleDifference/90) * u;
    }


    public static float Sigmoid(double value) {
        if(value > 0.9) return 0;

        float k = (float)Math.Exp(value);
        return 1.0f - (k / (1.0f + k));
    }


    private void mitteKraftBerechnen(Knotenverwalter knotenV, Kantenverwalter kantenV) {
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            if(kantenV.GetEingehendeKnoten(knoten).Count+ kantenV.GetAusgehendeKnoten(knoten).Count == 1) {
                //knoten.geschwindigkeit += (20/knoten.transform.position.magnitude) * knoten.transform.position.normalized;
                //knoten.geschwindigkeit += 1 * knoten.transform.position.normalized;
            }
        }
    }

    private Vector3 kantenKraftBerechnen(Knoten gewirkter, Knoten wirkender) {
        Vector3 diff = wirkender.transform.position - gewirkter.transform.position; 
        return diff.normalized*(Mathf.Min(diff.magnitude-idealeLänge, 10));
        //return diff.normalized*Mathf.Log(diff.magnitude/idealeLänge);
    }

    private void fixIdentity(Knotenverwalter knotenV) {
        foreach(Knoten knoten in knotenV.GetKnoten()) {
            if(knoten.name == "") {
                knoten.attractForce = Vector3.zero;
                knoten.repelForce = Vector3.zero;
                knoten.oppositeForce = Vector3.zero;
                knoten.angleForce = Vector3.zero;
            }
        }
    }
}