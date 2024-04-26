using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonoidGallery : PresentationGallery {
    MonoidGallery() : base("Some Monoids <size=50%>(experimental)</size>:", monoids) { }

    static readonly PresentationExample[] monoids = {
        new PresentationExample("ℕ<sub>0</sub>", "The natural numbers", "a", "", "The free monoid on one generator", "https://en.wikipedia.org/wiki/Natural_number", GroupMode.Monoid),
        new PresentationExample("B", "The bicyclic monoid", "a,b", "ab", "In monoids there is no automatic cancellation ab=b => a=1 and in particular, being a right inverse (ab=1) doesn't imply being a left inverse (ba=1).", "https://en.wikipedia.org/wiki/Bicyclic_semigroup", GroupMode.Monoid),
        new FreeGroup() { groupMode = GroupMode.Monoid, name = "A*", description = "The free monoid over n generators", tooltipInfo = "The free monoid over the alphabet A = {a, b, ...} consists of words in these generators. The free group can be understood as the free monoid over the generators and formal inverses together with the relations aA=Aa=1, and so on. All monoids and groups defined from presentations like here arise as quotients of the free monoid, which forms a rooted tree.", tooltipURL = "https://en.wikipedia.org/wiki/Free_monoid"},
        new LatticeGroup() { groupMode = GroupMode.Monoid, name = "ℕ<sub>0</sub><sup>n</sup>", description = "The free abelian monoid with n generators", tooltipURL = ""},
        new PlacticMonoid()
    };

}
