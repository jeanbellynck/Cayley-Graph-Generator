using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CayleySubgraphUI : MonoBehaviour
{
    public CayleySubGraphMaker cayleySubGraphMaker;
    public RelatorMenu subgroupGenerators;


    public void GenerateSubgroup() {
        IEnumerable<string> generators = subgroupGenerators.GetRelators();
        cayleySubGraphMaker.RegenerateSubgroup(generators);
    }
}
