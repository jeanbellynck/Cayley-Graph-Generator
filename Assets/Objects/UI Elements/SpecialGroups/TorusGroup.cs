using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class TorusGroup : Group
{
    public TorusGroup()
    {
        name = "C<sub>m</sub> × C<sub>n</sub>";
        description = "A direct product of two cyclic group, shaped like a torus";
        parameters = new string[][] {new string[] {"m", "5", "Order of the first cyclic group"}, new string[] {"n", "15", "Order of the second cyclic group"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int m) && m >= 1 && int.TryParse(parameters[1][1], out int n) && n >= 1)
        {
            generators = new string[]{"a", "b"};
            relators = new string[]{"abAB", string.Concat(Enumerable.Repeat("a", n)), string.Concat(Enumerable.Repeat("b", m))};
        }
    }
}
