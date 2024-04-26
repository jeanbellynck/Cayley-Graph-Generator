using System.Collections.Generic;
using System.Linq;

public class PlacticMonoid : PresentationExample
{
    public PlacticMonoid()
    {
        name = "P<sub>n</sub>";
        description = "The plactic monoid with n generators";
        parameters = new GroupParameter[] {new() {name = "n", value = "2", description = "Number of generators"}};
        tooltipURL = "https://en.wikipedia.org/wiki/Plactic_monoid";
        groupMode = GroupMode.Monoid;
        updatePresentation();
    }


    public override void updatePresentation() {
        
        if (!int.TryParse(parameters[0].value, out int n) || n < 0) return;

        List<string> gen = new();
        HashSet<string> rel = new();
            
        for (int i = 0; i < n; i++)
            gen.Add(((char) ('a' + i)).ToString()) ;

        for (int z = 0; z < n; z++)
        for (int y = 0; y <= z; y++)
        for (int x = 0; x < y; x++)
            rel.Add($"{gen[y]}{gen[z]}{gen[x]}={gen[y]}{gen[x]}{gen[z]}");

        for (int z = 0; z < n; z++)
        for (int y = 0; y < z; y++)
        for (int x = 0; x <= y; x++)
            rel.Add($"{gen[x]}{gen[z]}{gen[y]}={gen[z]}{gen[x]}{gen[y]}");
       
        generators = gen.ToArray();
        relators = rel.ToArray();
    }
}
