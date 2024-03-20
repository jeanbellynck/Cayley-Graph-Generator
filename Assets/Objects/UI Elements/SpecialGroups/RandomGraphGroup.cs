using System.Globalization;
using System.Linq;

public class RandomGraphGroup : Group {
    public RandomGraphGroup() {
        name = "RG<sub>v, e, g</sub>";
        description = "A group defined by loops in a random graph with labels in the generating set";
        parameters = new GroupParameter[] {
            new(){
                name = "v",
                value = "5",
                description = "Number of vertices"
            },
            new () {
                name = "e",
                value = "12",
                description = "Number of edges"
            },
            new() {
                name = "g",
                value = "0.8",
                description = "#Generators = max. valency of graph * this; >= 0.5"
            }
        };
        updatePresentation();
    }


    public override void updatePresentation() {
        char separator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.First();
        if (!int.TryParse(parameters[0].value, out int v) || 
            v < 1 ||
            !int.TryParse(parameters[1].value, out int e) ||
            e < 0 ||
            !float.TryParse(parameters[2].value.FixDecimalPoint(), out float g) ||
            g < 0.5) 
            return;

        (generators, relators) = RandomGroups.RandomPresentation(v,e, g);

        //var Gen = (from gen in generators select gen.ToUpper()).ToArray();
    }
}