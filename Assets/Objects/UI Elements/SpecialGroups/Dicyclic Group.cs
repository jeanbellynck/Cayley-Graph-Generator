using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class DicyclicGroup : Group
{
    public DicyclicGroup()
    {
        name = "Dic<sub>n</sub>";
        description = "The dicyclic groups";
        parameters = new string[][] {new string[] {"n", "2", "The order of the cyclic group the dicyclic group is based on"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 1)
        {
            List<string> gen = new List<string>{"r", "f"};
            List<string> rel = new List<string>
            {
                string.Concat(Enumerable.Repeat("r", 2 * n)),
                string.Concat(Enumerable.Repeat("r", n)) + "FF",
                "frFr"
            };
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
