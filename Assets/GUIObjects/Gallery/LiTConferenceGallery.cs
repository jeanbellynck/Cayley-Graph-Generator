using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiTConferenceGallery : PresentationGallery
{
    LiTConferenceGallery() : base("LiT examples:", litGroups) { }
    
    // List of groups to be displayed, parameters are name, generators and relators.
    static readonly PresentationExample[] litGroups = {
        new CyclicGroup(),
        new SymmetricGroup(),
        new PresentationExample("C<sub>6</sub> * S<sub>3</sub>", "The amalgamation of C<sub>6</sub> * S<sub>3</sub> along C<sub>2</sub>", "a; b; c", "a^6; b^3; c^2; bcbc; aaa=c", "", ""),
        new PresentationExample("PSL(2, ℤ)", "The projective special linear group of 2-matrices", "a; b", "a^2; b^3", "", ""),
        new BraidGroup(),
        new PresentationExample("ℍ²", "A simple hyperbolic group", "a; b", "abab; a^5; b^5", "For a given Cayley graph we define a ball as follows. For a given n the n-ball contains all vertices which are at most n edges away from the identity. In geometric group theory we are interested in how fast the vertices inside the ball grow. For example in the group of the whole numbers Z, the ball grows linearly, same for the infinite dihedral group. Z^2 grows to the square of the radius. And then there are groups which grow exponentially to the radius of the ball. Groups such as these have hyperbolic growth.", "https://en.wikipedia.org/wiki/Hyperbolic_group"),
        new PresentationExample("F", "Thompson group F", "a; b", "[aB, Aba]; [aB, AABaa]", "The Thompson group F is a group with many weird properties. It can be understood as the group of piecewise-linear homeomorphisms of the unit interval [0, 1] where the slope and break points are diadic. Recently, there have been fascinating advances in relating elements of the Thompson group with knots!", "https://en.wikipedia.org/wiki/Thompson_groups"),
        new PresentationExample("F", "Thompson group F (partial infinite presentation)", "a, b, c, d", "Aba=c, Aca=d, Bcb=d", "The Thompson group F has a finite presentation but it can be easier understood by studying its infinite presentation. Here, a finite subset of the generators is taken for illustrative purposes.", "https://en.wikipedia.org/wiki/Thompson_groups"),
        new PresentationExample("ℕ<sub>0</sub>", "The natural numbers", "a", "", "The free monoid on one generator", "https://en.wikipedia.org/wiki/Natural_number", GroupMode.Monoid),
    };

}
