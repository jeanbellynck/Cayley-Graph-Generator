using System.Collections.Generic;

public class DicyclicGroup : PresentationExample
{
    public DicyclicGroup()
    {
        name = "Dic<sub>n</sub>";
        description = "The dicyclic groups";
        parameters = new GroupParameter[] {
            new() {
                name = "n", value = "2", description = "The order of the cyclic group the dicyclic group is based on"
            }
        };
        tooltipInfo = "The dicyclic groups generalization of the finite quaternion group."; 
        tooltipURL = "https://en.wikipedia.org/wiki/Dicyclic_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n is >= 0 and <= 10)
        {
            List<string> gen = new List<string>{"r", "f"};
            List<string> rel = new List<string>
            {
                "r^" + (2*n),
                "r^" + (n) + "F^2",
                "frFr"
            };
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
