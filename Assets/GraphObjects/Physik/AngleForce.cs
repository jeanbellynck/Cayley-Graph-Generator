using System.Collections;
using System.Collections.Generic;

/** Currently not working and not in use. **/
[System.Serializable]
public class AngleForce : Force {
    
    char[] generators = new char[0];
    Dictionary<char, Dictionary<char, float>> sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
    // Necessary since if you dont count to many the average is too small
    Dictionary<char, Dictionary<char, int>> countedArrowAngles = new Dictionary<char, Dictionary<char, int>>();
    Dictionary<char, Dictionary<char, float>> averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();
    
    public override IEnumerator ApplyForce(LabelledGraphManager graphManager, float alpha) {
        throw new System.NotImplementedException();
    }

    public void setGenerators(char[] generators) {
        this.generators = new char[generators.Length * 2];
        for (int i = 0; i < generators.Length; i++) {
            this.generators[i] = generators[i];
            this.generators[i + generators.Length] = char.ToUpper(generators[i]);
        }

        sumArrowAngles = new Dictionary<char, Dictionary<char, float>>();
        countedArrowAngles = new Dictionary<char, Dictionary<char, int>>();
        averageArrowAngles = new Dictionary<char, Dictionary<char, float>>();

        foreach (char gen1 in this.generators) {
            sumArrowAngles[gen1] = new Dictionary<char, float>();
            countedArrowAngles[gen1] = new Dictionary<char, int>();
            averageArrowAngles[gen1] = new Dictionary<char, float>();
        }
        //currentIteration = 0;
    }

    

    //public int arrowAverageRecalculationIteration = 10;
    //int currentIteration = 0;

    /**
     * Calculates the stress of a vertex. The stress is a measure of how unusual the angles of the vertex are. It is used to visualize weird spots.
     **/
     /**
    private void calculateStress() {
        if (currentIteration == 0) {
            //calculateAverageAngles();
            currentIteration = arrowAverageRecalculationIteration;
        }
        currentIteration--;

        foreach (Vertex vertex in graphManager.getVertex()) {
            vertex.Stress = 0;
            foreach (char gen1 in generators) {
                foreach (char gen2 in generators) {
                    Vertex vertex1 = vertex.FollowEdge(gen1);
                    Vertex vertex2 = vertex.FollowEdge(gen2);

                    if (gen1 != gen2 && vertex1 != null && vertex2 != null && vertex1.name != vertex2.name) {
                        VectorN force = angleForceFactor * calculateAverageAngleForceBetweenTwoVertices(vertex, vertex1, vertex2, averageArrowAngles[gen1][gen2]);
                        vertex.Stress += force.magnitude;
                    }
                }
            }
        }
    }**/


    /**
     * Goes over each pair of angles and calculates theirs averages. This is done once per recalculationIteration to reduce the number of calculations.
     * Disabled since this method should not be sitting in physics. 
     **/
     /**
    private void calculateAverageAngles() {
        foreach (char gen1 in generators) {
            foreach (char gen2 in generators) {
                countedArrowAngles[gen1][gen2] = 0;
                sumArrowAngles[gen1][gen2] = 0f;
                averageArrowAngles[gen1][gen2] = 0f;

                foreach (Vertex vertex in graphManager.getVertex()) {
                    Vertex vertex1 = vertex.followEdge(gen1);
                    Vertex vertex2 = vertex.followEdge(gen2);

                    if (vertex1 != null && vertex2 != null && vertex1.Id != vertex2.Id) {
                        // Thing is calculated twice, this can be optimized
                        sumArrowAngles[gen1][gen2] += VectorN.Angle(vertex1.transform.position - vertex.transform.position, vertex2.transform.position - vertex.transform.position);
                        countedArrowAngles[gen1][gen2] += 1;
                    }
                }

                averageArrowAngles[gen1][gen2] = sumArrowAngles[gen1][gen2] / countedArrowAngles[gen1][gen2];
            }
        }
    }

    private VectorN calculateAverageAngleForceBetweenTwoVertices(Vertex vertexBase, Vertex vertex1, Vertex vertex2, float averageAngle) {
        VectorN v = VectorN.Normalize(vertex1.transform.position - vertexBase.transform.position);
        VectorN w = VectorN.Normalize(vertex2.transform.position - vertexBase.transform.position);

        VectorN u = v - w;
        VectorN.OrthoNormalize(ref v, ref u); // u become the orthogonal vector

        float angleDifference = averageAngle - VectorN.Angle(v, w);

        return angleDifference / 90 * (angleDifference / 90) * u;
    }
    **/

}