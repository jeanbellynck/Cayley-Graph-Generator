using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupGallery : MonoBehaviour
{
    
    // List of groups to be displayed, parameters are name, generators and relators.
    Group[] groupList = {
        new Group("C<sub>5</sub>×C<sub>12</sub>", "Product of two cyclic groups, shaped like a torus", "a, b", "abAB, aaaaa, bbbbbbbbbbbb"),
        new Group("e", "The trivial group", "", ""),
        new CyclicGroup(),
        new SymmetricGroup(),
        new Group("V<sub>4</sub>", "The Klein group", "a, b", "aa, bb, abAB"),
        new DihedralGroup(),
        new Group("D<sub>∞</sub>", "The infinite dihedral group", "r, f", "ff, rfrf"),
        new Group("Q<sub>8</sub>", "The quaternion group", "i, j", "iiii, jijI, ijiJ"),
        new DicyclicGroup(),
        new Group("T ≅ A<sub>4</sub>", "The symmetry group of the tetrahedron", "s, t", "ss, ttt, ststst"),
        new Group("O ≅ S<sub>4</sub>", "The symmetry group of the octahedron", "s, t", "ss, ttt, stststst"),
        new Group("I ≅ A<sub>5</sub>", "The symmetry group of the icosahedron", "s, t", "ss, ttt, ststststst"),
        new LatticeGroup(),
        new Group("ℤ²", "Z² from three generators", "a, b, c", "abAB, abC"),
        new FreeGroup(),
        new BraidGroup(),
        new Group("ℍ²", "A simple hyperbolic group", "a, b", "abab, aaaaa, bbbbb"),
        new Group("SL(2, ℤ)", "The special linear group of 2-matrices", "a, b", "abaBAB, abaabaabaaba"),
        new Group("PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a, b", "aa, bbb"),
        new Group("GL(2, ℤ)", "The general linear group of 2-matrices", "a, b, j", "abaBAB, abaabaabaaba, jj, jaja, jbjb"),
        new Group("H", "The Heisenberg group", "x, y, z", "zyxYX, xzXZ, yzYZ"),
        new Group("BS(1, 2)", "A Baumslag-Solitar group", "a, b", "aabAb"),
        new Group("F", "Thompson group F", "a, b", "aBAbabAABa, aBAAbaabAAABaa"),
        new Group("²F<sub>4</sub>(2)'", "Tits group", "a, b", "aa, bbb, ababababababababababababab, abABabABabABabABabAB, ababABABababABABababABABababABAB, ababababaBababababaBababababaBababababaBababababaBababababaBababababaB"),
        new Group("?", "???", "a, b, c", "aaa, bbb, ccc, abAbA, acab")
    };
    
    
    public GameObject groupPrefab;
    public GameObject groupGallery;
    public GameObject cayleyGraph;


    // Start is called before the first frame update
    void Start()
    {
        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in groupList)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().setGroupAndStartVisualisation(group.name, group.generators, group.relators));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
