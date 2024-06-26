using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteGroupGallery : PresentationGallery
{
    InfiniteGroupGallery() : base("Infinite Groups:", infiniteGroups) { }

    static readonly PresentationExample[] infiniteGroups = {
        new FreeGroup(),
        new PresentationExample("ℤ²", "Z² from three generators", "a; b; c", "[a, b]; abC", "This group represents all 2-dimensional vectors with whole number components. The group operation is addition of the vectors. We usually think of this group as being generated by the vectors (1, 0) and (0, 1). However in this example we also added (1, 1) to the generators.", ""),
        new LatticeGroup(),
        new PresentationExample("D<sub>∞</sub>", "The infinite dihedral group", "r; f", "f^2; (rf)^2", "The finite dihedral groups are the symmetries of regular polygons. If we increase the number of vertices of the polygon to infinity, while keeping the length of the edges at 1, the resulting shape looks like the numberline of the whole numbers. The rotation r of the polygon then becomes a shift of the numberline to the left. The other generator s is the reflection at 0.", "https://en.wikipedia.org/wiki/Infinite_dihedral_group"),
        new SurfaceFundamentalGroup(),
        new SurfaceMappingClassGroup(),
        new PresentationExample("SL(2, ℤ)", "The special linear group of 2-matrices", "a; b", "abaBAB; (aba)^4", "When two whole-number matrices with determinant 1 are multiplied the result is again a matrix with determinant 1. The set of all matrices with determinant 1 forms a group and is called the special linear group. The generators a, b are the matrices ((1, 1), (1, 0)) and ((1, 0), (1, 1)).", "https://en.wikipedia.org/wiki/Special_linear_group"),
        //new Group("PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a; b", "a^2; b^3"),
        new PresentationExample("GL(2, ℤ)", "The general linear group of 2-matrices", "a; b; j", "abaBAB; (aba)^4; j^2; (ja)^2; (jb)^2", "The general linear group is the set of all invertable whole-number 2x2 matrices. Since the inverse of a matrix has a reciprocal determinant, a whole-number matrix is only invertable if its determinant is either 1 or -1. By adding a reflection ((-1, 0), (0, 1)) to the generators of the special linear group we can generate the general linear group.", "https://en.wikipedia.org/wiki/General_linear_group"),
        new BraidGroup(),
        new PresentationExample("ℍ²", "A simple hyperbolic group", "a; b", "abab; a^5; b^5", "For a given Cayley graph we define a ball as follows. For a given n the n-ball contains all vertices which are at most n edges away from the identity. In geometric group theory we are interested in how fast the vertices inside the ball grow. For example in the group of the whole numbers Z, the ball grows linearly, same for the infinite dihedral group. Z^2 grows to the square of the radius. And then there are groups which grow exponentially to the radius of the ball. Groups such as these have hyperbolic growth.", "https://en.wikipedia.org/wiki/Hyperbolic_group"),
        new PresentationExample("H", "The Heisenberg group", "x; y; z", "zyxYX; xzXZ; yzYZ", "Th Heisenberg group is a group of 3x3 matrices, fulfilkling some conditions. The group is used quantum mechanics to describe one-diensional quantum mechanical systems.", "https://en.wikipedia.org/wiki/Heisenberg_group"),
        new BSGroup(),
        new PresentationExample("F", "Thompson group F", "a; b", "[aB, Aba]; [aB, AABaa]", "The Thompson group F is a group with many weird properties. It can be understood as the group of piecewise-linear homeomorphisms of the unit interval [0, 1] where the slope and break points are diadic. Recently, there have been fascinating advances in relating elements of the Thompson group with knots!", "https://en.wikipedia.org/wiki/Thompson_groups"),
        new PresentationExample("F", "Thompson group F (partial infinite presentation)", "a, b, c, d", "Aba=c, Aca=d, Bcb=d", "The Thompson group F has a finite presentation but it can be easier understood by studying its infinite presentation. Here, a finite subset of the generators is taken for illustrative purposes.", "https://en.wikipedia.org/wiki/Thompson_groups"),
        new PresentationExample("²F<sub>4</sub>(2)'", "Tits group", "a; b", "a^2; b^3; (ab)^13; [a, b]^5, [a, bab]^4, ((ab)^4aB)^6"),
        //new RandomGroup(),
        //new RandomGraphGroup(),
    };

}
