using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class BSGroup : Group
{
    public BSGroup()
    {
        name = "BS(m, n)";
        description = "The Baumslag-Solitar group";
        parameters = new string[][] {new string[] {"m", "1", "First exponent of the relation a^n = ba^mB"}, new string[] {"n", "2", "Second exponent of the relation a^n = ba^mB"}};
        tooltipInfo = "Baumslag-Solitar groups are a st of groups with simple presentations. They are counterexamples to many intuitions, making them ideal candidates to test new conjectures.";
        tooltipURL = "https://en.wikipedia.org/wiki/Baumslag%E2%80%93Solitar_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && int.TryParse(parameters[1][1], out int m) && m >= 1)
        {
            string relator = "a^" + n.ToString() + "=ba^" + m.ToString() + "B";    
            generators = new string[]{"a", "b"};
            relators = new string[]{relator};
        }
    }
}
