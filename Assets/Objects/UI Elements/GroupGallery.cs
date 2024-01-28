using UnityEngine;
using UnityEngine.UI;

public class GroupGallery : MonoBehaviour
{
    
    // List of groups to be displayed, parameters are name, generators and relators.
    Group[] finiteGroups = {
        new Group("e", "The trivial group", "", ""),
        new CyclicGroup(),
        new Group("V<sub>4</sub>", "The Klein group", "a, b", "aa, bb, abAB"),
        new TorusGroup(),
        new DihedralGroup(),
        new SymmetricGroup(),
        new Group("Q<sub>8</sub>", "The quaternion group", "i, j", "iiii, jijI, ijiJ"),
        new DicyclicGroup(),
        new Group("T ≅ A<sub>4</sub>", "The symmetry group of the tetrahedron", "s, t", "ss, ttt, ststst"),
        new Group("O ≅ S<sub>4</sub>", "The symmetry group of the octahedron", "s, t", "ss, ttt, stststst"),
        new Group("I ≅ A<sub>5</sub>", "The symmetry group of the icosahedron", "s, t", "ss, ttt, ststststst"),
        new Group("Rub<sub>2×2×2</sub>", "The group of the 2x2x2 Rubiks cube", "a, b, c", "aaaa, bbbb, cccc, ababaBABAB, bcbcbCBCBC, abcbaCABCB, bcacbABCAC, cabacBCABA, acacabababcbcb"),
        //new MonsterGroup()
    };

    Group[] infiniteGroups = {
        new FreeGroup(),
        new Group("ℤ²", "Z² from three generators", "a, b, c", "abAB, abC"),
        new LatticeGroup(),
        new Group("D<sub>∞</sub>", "The infinite dihedral group", "r, f", "ff, rfrf"),
        new Group("SL(2, ℤ)", "The special linear group of 2-matrices", "a, b", "abaBAB, abaabaabaaba"),
        new Group("PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a, b", "aa, bbb"),
        new Group("GL(2, ℤ)", "The general linear group of 2-matrices", "a, b, j", "abaBAB, abaabaabaaba, jj, jaja, jbjb"),
        new BraidGroup(),
        new Group("ℍ²", "A simple hyperbolic group", "a, b", "abab, aaaaa, bbbbb"),
        new Group("H", "The Heisenberg group", "x, y, z", "zyxYX, xzXZ, yzYZ"),
        new BSGroup(),
        new Group("F", "Thompson group F", "a, b", "aBAbabAABa, aBAAbaabAAABaa"),
        new Group("²F<sub>4</sub>(2)'", "Tits group", "a, b", "aa, bbb, ababababababababababababab, abABabABabABabABabAB, ababABABababABABababABABababABAB, ababababaBababababaBababababaBababababaBababababaBababababaBababababaB"),
        //new RandomGroup(),
        new Group("?", "???", "a, b, c", "aaa, bbb, ccc, abAbA, acab")
    };
    
    
    public GameObject groupPrefab;
    public GameObject labelPrefab;
    public GameObject groupGallery;
    public GameObject cayleyGraph;


    // Start is called before the first frame update
    void Start()
    {
        // Add a finite group label
        GameObject finiteGroupLabel = Instantiate(labelPrefab, transform);
        finiteGroupLabel.GetComponent<Text>().text = "Finite Group Examples";
        finiteGroupLabel.transform.SetParent(groupGallery.transform);


        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in finiteGroups)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().setGroupAndStartVisualisation(group.name, group.generators, group.relators));
        }
        
        // Add an infinite group label
        GameObject infiniteGroupLabel = Instantiate(labelPrefab, transform);
        infiniteGroupLabel.GetComponent<Text>().text = "Infinite Group Examples";
        infiniteGroupLabel.transform.SetParent(groupGallery.transform);

        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in infiniteGroups)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().setGroupAndStartVisualisation(group.name, group.generators, group.relators));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
