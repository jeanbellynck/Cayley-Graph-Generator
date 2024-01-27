using System.Collections.Generic;
using System.Linq;

public class DihedralGroup : Group
{
    public DihedralGroup()
    {
        name = "D<sub>n</sub>";
        description = "The dihedral groups";
        parameters = new string[][] {new string[] {"n", "2", "Edges of the polygon the dihedral group is a symmetry group of"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 1)
        {
            List<string> gen = new List<string>(){"r", "s"};
            List<string> rel = new List<string>
            {
                "rsrs",
                "ss",
                string.Concat(Enumerable.Repeat("r", n))
            };
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
