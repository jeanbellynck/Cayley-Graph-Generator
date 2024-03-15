using System.Collections.Generic;
using System.Linq;

public class DihedralGroup : Group
{
    public DihedralGroup()
    {
        name = "D<sub>n</sub>";
        description = "The dihedral groups";
        parameters = new GroupParameter[] {new() {name = "n", value = "2", description = "Edges of the polygon the dihedral group is a symmetry group of"}};
        tooltipInfo = "The dihedral group of order 2n is the group of symmetries of a regular n-gon. The group element r rotates the n-gon, the group element s induces a reflection. Every symmetry can be recreated by chaining r and s together.";
        tooltipURL = "https://en.wikipedia.org/wiki/Dihedral_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n >= 1)
        {
            List<string> gen = new List<string>(){"r", "s"};
            List<string> rel = new List<string>
            {
                "rsrs",
                "s^2",
                "r^" + n.ToString()
            };
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
