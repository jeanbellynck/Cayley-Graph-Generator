using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupGallery : MonoBehaviour
{
    
    // List of groups to be displayed, parameters are name, generators and relators.
    Group[] finiteGroups = {
        new Group("e", "The trivial group", "", ""),
        new CyclicGroup(),
        new Group("V<sub>4</sub>", "The Klein group", "a; b", "a^2; b^2; abAB"),
        new TorusGroup(),
        new DihedralGroup(),
        new SymmetricGroup(),
        new Group("Q<sub>8</sub>", "The quaternion group", "i; j", "i^4; jijI; ijiJ"),
        new DicyclicGroup(),
        new Group("T ≅ A<sub>4</sub>", "The symmetry group of the tetrahedron", "s; t", "s^2; t^3; (st)^3"),
        new Group("O ≅ S<sub>4</sub>", "The symmetry group of the octahedron", "s; t", "s^2; t^3; (st)^4"),
        new Group("I ≅ A<sub>5</sub>", "The symmetry group of the icosahedron", "s; t", "s^2; t^3; (st)^5"),
        new Group("Rub<sub>2×2×2</sub>", "The group of the 2x2x2 Rubiks cube", "a; b; c", "a^4; b^4; c^4; ababa=babab; ababa = babab; bcbcb = cbcbc; abcba = bcbac; bcacb = cacba; cabac = abacb; (ac)^2(ab)^3(cb)^2"),
        //new MonsterGroup()
    };

    Group[] infiniteGroups = {
        new FreeGroup(),
        new Group("ℤ²", "Z² from three generators", "a; b; c", "[a, b]; abC"),
        new LatticeGroup(),
        new Group("D<sub>∞</sub>", "The infinite dihedral group", "r; f", "f^2; (rf)^2"),
        new Group("SL(2, ℤ)", "The special linear group of 2-matrices", "a; b", "abaBAB; (aba)^4"),
        new Group("PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a; b", "a^2; b^3"),
        new Group("GL(2, ℤ)", "The general linear group of 2-matrices", "a; b; j", "abaBAB; (aba)^4; j^2; (ja)^2; (jb)^2"),
        new BraidGroup(),
        new Group("ℍ²", "A simple hyperbolic group", "a; b", "abab; a^5; b^5"),
        new Group("H", "The Heisenberg group", "x; y; z", "zyxYX; xzXZ; yzYZ"),
        new BSGroup(),
        new Group("F", "Thompson group F", "a; b", "[aB, Aba]; [aB, AABaa]"),
        new Group("²F<sub>4</sub>(2)'", "Tits group", "a; b", "a^2; b^3; (ab)^13; [a, b]^5, [a, bab]^4, ((ab)^4aB)^6"),
        //new RandomGroup(),
        new RandomGraphGroup(),
    };

    Group[] eacgt = {
        new FreeGroup(),
        new Group("SL(2, ℤ)", "Special Linear group (different presentation than above)", "a, b", "b^2=(ab)^3, b^4"),
        new Group("SL(2, ℤ[1/2])", "Special Linear group over a bigger ring", "a, b, u", "(ab)^3B^2, (ub)^2B^2, (bua^2)^3B^2, b^4, Uau=a^4"),
        new Group("SL(2, ℤ[1/3])", "Special Linear group over a bigger ring", "a, b, u", "(ab)^3B^2, (ub)^2B^2, (bua^3)^3B^2, b^4, Uau=a^9"),
        new Group("ℍ²", "A simple hyperbolic group", "a; b", "abab; a^5; b^5"),
        new BraidGroup(),
        new Group("F", "Thompson group F", "a; b", "[aB, Aba]; [aB, AABaa]"),
        new Group("F", "Thompson group F (partial infinite presentation)", "a, b, c, d", "Aba=c, Aca=d, Bcb=d"),
        new Group("F", "Thompson-Brown group F3 (partial infinite presentation)", "a, b, c, d, e", "Aba=d, Aca=e, Bcb=e"),
        new Group("G<sub>4</sub><sup>2</sup>", "Manturov-Kim group", "a, b, c, d, e, f", "aa, bb, cc, dd, ee, ff, afaf, bfbf, abab, acac, bcbc, adad, bebe, cdcd, cece, dfdf, efef, dede, abfabf, defdef, acdacd, bcebce")
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
        finiteGroupLabel.GetComponent<TMP_Text>().text = "Finite Group Examples";
        finiteGroupLabel.transform.SetParent(groupGallery.transform);


        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in finiteGroups)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().SelectGroupOption(group.name, string.Join(',', group.generators), string.Join(',', group.relators)));
        }
        
        // Add an infinite group label
        GameObject infiniteGroupLabel = Instantiate(labelPrefab, transform);
        infiniteGroupLabel.GetComponent<TMP_Text>().text = "Infinite Group Examples";
        infiniteGroupLabel.transform.SetParent(groupGallery.transform);

        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in infiniteGroups) {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().SelectGroupOption(group.name, string.Join(',', group.generators), string.Join(',', group.relators)));
        }

        // Add an infinite group label
        GameObject eacgtGroupLabel = Instantiate(labelPrefab, transform);
        eacgtGroupLabel.GetComponent<TMP_Text>().text = "EACGT Group Examples";
        eacgtGroupLabel.transform.SetParent(groupGallery.transform);

        // For each Group create a new group object and set it as a child of the gallery.
        foreach (Group group in eacgt)
        {
            GameObject newGroup = Instantiate(groupPrefab, transform);
            newGroup.GetComponent<GroupOption>().group = group;
            newGroup.transform.SetParent(groupGallery.transform);

            // When the button is clicked the setGroupAndStartVisualisation() method of CayleyGraph is called
            newGroup.GetComponent<Button>().onClick.AddListener(() => cayleyGraph.GetComponent<CayleyGraph>().SelectGroupOption(group.name, string.Join(',', group.generators), string.Join(',', group.relators)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
