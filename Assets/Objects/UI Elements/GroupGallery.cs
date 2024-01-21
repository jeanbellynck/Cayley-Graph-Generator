using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGallery : MonoBehaviour
{
    // List of groups to be displayed, parameters are name, generators and relators.
    string[][] groupList = {
        
        new string[] {"C₅×C₁₂", "Product of two cyclic groups, shaped like a torus", "a, b", "abAB, aaaaa, bbbbbbbbbbbb"},
        new string[] {"e", "The trivial group", "", ""},
        new string[] {"C₂", "The cyclic group of order 2", "a", "aa"},
        new string[] {"V₄", "The Klein group", "a, b", "aa, bb, abAB"},
        new string[] {"D₃ ≅ S₃", "The dihedral group of order 6", "r, f", "rrr, ff, rfrf"},
        new string[] {"D∞", "The infinite dihedral group", "r, f", "ff, rfrf"},
        new string[] {"S₄", "The symmetric group on 4 symbols", "a, b, c", "aa, bb, cc, acac, ababab, bcbcbc"},
        new string[] {"T ≅ A₄", "The symmetry group of the tetrahedron", "s, t", "ss, ttt, ststst"},
        new string[] {"O ≅ S₄", "The symmetry group of the octahedron", "s, t", "ss, ttt, stststst"},
        new string[] {"I ≅ A₅", "The symmetry group of the icosahedron", "s, t", "ss, ttt, ststststst"},
        new string[] {"ℤ", "The whole numbers", "a", ""},
        new string[] {"ℤ²", "ℤ² from two generators", "a, b", "abAB"},
        new string[] {"ℤ²", "ℤ² from three generators", "a, b, c", "abAB, abC"},
        new string[] {"Q₈", "The quaternion group", "i, j", "iiii, jijI, ijiJ"},
        new string[] {"F₂", "Free Group with two Generators", "a, b", ""},
        new string[] {"F₂", "Free Group with three Generators", "a, b, c", ""},
        new string[] {"B₃", "The braid group on three strands", "a, b", "abaBAB"},
        new string[] {"B₄", "The braid group on four strands", "a, b, c", "abaBAB, bcbCBC, acAC"},
        new string[] {"ℍ", "A Simple hyperbolic group", "a, b", "abab, aaaaa, bbbbb"},
        new string[] {"SL(2, ℤ)", "The special linear group of 2-matrices", "a, b", "abaBAB, abaabaabaaba"},
        new string[] {"PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a, b", "aa, bbb"},
        new string[] {"GL(2, ℤ)", "The general linear group of 2-matrices", "a, b, j", "abaBAB, abaabaabaaba, jj, jaja, jbjb"},
        new string[] {"H", "The Heisenberg group", "x, y, z", "zyxYX, xzXZ, yzYZ"},
        new string[] {"BS(1, 2)", "A Baumslag-Solitar group", "a, b", "aabAb"},
        new string[] {"²F₄(2)'", "Tits group", "a, b", "aa, bbb, ababababababababababababab, abABabABabABabABabAB, ababABABababABABababABABababABAB, ababababaBababababaBababababaBababababaBababababaBababababaBababababaB"},
        new string[] {"?", "???", "a, b, c", "aaa, bbb, ccc, abAbA, acab"}
    };
    public GameObject groupPrefab;
    public GameObject groupGallery;
    public GameObject cayleyGraph;


    // Start is called before the first frame update
    void Start()
    {
        // For each Group create a new group object and set it as a child of the gallery.
        foreach (string[] group in groupList)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<Group>().name = group[0];
            newGroup.GetComponent<Group>().description = group[1];
            newGroup.GetComponent<Group>().generators = group[2];
            newGroup.GetComponent<Group>().relators = group[3];
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().setGroupAndStartVisualisation(group[0], group[2], group[3]));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
