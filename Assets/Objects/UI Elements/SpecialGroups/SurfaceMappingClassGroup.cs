using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SurfaceMappingClassGroup : Group
{
    public SurfaceMappingClassGroup()
    {
        name = "MCG(S<sub>g</sub>)";
        description = "The mapping class group of the genus-g-surface";
        parameters = new GroupParameter[] {new() { name = "g", value = "2", description = "Genus"}};
        tooltipInfo = "This is Wajnryb's presentation in terms of the Humphries generators which are certain Dehn twists. [Farb and Margalit - \"A primer on mapping class groups\", Thm. 5.3]";

        updatePresentation();
    }


    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override void updatePresentation() {
        // A non-negative number integer 
        if (!int.TryParse(parameters[0].value, out int g) || g < 0 || g > 10) return;

        switch (g) {
            case 0: {
                generators = relators = Array.Empty<string>();
                return;
            }
            case 1: {
                generators = new[] { "a", "b" };
                var braidRelation = "aba=bab";
                var twoChainRelation = "(ab)^6";
                relators = new[] { braidRelation, twoChainRelation };
                return;
            }
            case 2: {
                var range = Enumerable.Range(0, 2 * g + 1);
                var a = generators = (from i in range
                    select ((char)('a' + i)).ToString()
                    ).ToArray();
                var disjointnessRelations = (
                    from i in range
                    from j in range
                    where i < j - 1 // curves c_i and c_j are disjoint
                    select $"[{a[i]},{a[j]}]"
                );

                var braidRelations = (
                    from i in Enumerable.Range(0, 2 * g)
                    let j = i + 1 // curves c_i and c_(i+1) intersect once
                    let ai = a[i]
                    let aj = a[j]
                    select $"{ai}{aj}{ai}={aj}{ai}{aj}"
                );
                const string threeChainRelation = "(abc)^3=f^2";
                const string hyperEllipticRelation = "(edcbaabcde)^2";
                const string hyperEllipticRelation2 = "[edcbaabcde,a]";

                relators = disjointnessRelations
                        .Concat(braidRelations)
                        .Append(threeChainRelation)
                        .Append(hyperEllipticRelation)
                        .Append(hyperEllipticRelation2)
                        .ToArray();
                return;
            }
            default: {
                var range = Enumerable.Range(1, 2 * g);

                var a = generators = new[] { "z" }.Concat(
                    from i in range
                    select ((char)('a' - 1 + i)).ToString()
                ).ToArray();

                var disjointnessRelations = (
                    from i in range
                    from j in range
                    where i < j - 1 // curves c_i and c_j are disjoint
                    select $"[{a[i]},{a[j]}]"
                ).Concat(
                    from i in range
                    where i != 4 // curves c_i and c_0 = z are disjoint
                    select "[" + a[i] + "," + "z" + "]"
                );

                var braidRelations = (
                    from i in Enumerable.Range(1, 2 * g - 1)
                    let j = i + 1 // curves c_i and c_(i+1) intersect once
                    let ai = a[i]
                    let aj = a[j]
                    select $"{ai}{aj}{ai}={aj}{ai}{aj}"
                ).Append("zdz=dzd"); // curves c_0 = z and c_4 = d intersect once

                // bi are Dehn twists along other curves, expressed in terms of the Humphries generators
                var b0 = Conjugate("z", "dcbaabcd");

                var b1 = ConjugateReverse("z", "bcab");
                var b2 = ConjugateReverse(b1, "decd");
                var u = ConjugateReverse(b1, "fe");
                var b3 = Conjugate("z", $"fedcb{u}ABCD");
                
                var threeChainRelation = $"(abc)^3 = z{b0}";
                var lanternRelation = $"z{b2}{b1} = acf{b3}";

                var topTwists = new List<string>(g) { "a", b0 };
                // these are the Dehn twists along the "top" curves, expressed in terms of the Humphries generators
                foreach (int i in Enumerable.Range(1, g - 2)) {
                    var zi = 2 * i;
                    var s0 = a[zi + 0];
                    var s1 = a[zi + 1];
                    var s2 = a[zi + 2];
                    var s3 = a[zi + 3];
                    var s4 = a[zi + 4];
                    var w = $"({s4}{s3}{s2}{topTwists[i]})" +
                            $"({s1}{s0}{s2}{s1})" +
                            $"({s3}{s2}{s4}{s3})" +
                            $"({topTwists[i]}{s2}{s1}{s0})";
                    var twist = Conjugate(topTwists[i - 1], w);
                    topTwists.Add(twist);
                }

                var d = topTwists[^1];
                var m = string.Concat(a[1..]) + string.Concat(a[1..].Reverse());
                var hyperEllipticRelation = $"{m}{d} = {d}{m}";

                relators = disjointnessRelations
                        .Concat(braidRelations)
                        .Append(threeChainRelation)
                        .Append(lanternRelation)
                        .Append(hyperEllipticRelation)
                        .ToArray();
                return;
            }
        }
        string Invert(string s) => $"({s})^-1";
        string Conjugate(string s, string t) => $"({t}){s}{Invert(t)}";
        string ConjugateReverse(string s, string t) => $"{Invert(t)}{s}({t})";
    }
}
