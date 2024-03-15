using System.Collections.Generic;
using System.Globalization;

public class FreeGroup : Group
{
    public FreeGroup()
    {
        name = "F<sup>n</sup>";
        description = "The free group";
        parameters = new GroupParameter[] {new() {name = "n", value = "2", description = "Symbols in the free group"}};
        tooltipInfo = "The free group where the generating elements have no relationship to each other. This means it is not possible to write an equation such as ab = ba. Due to that reason its Cayley graph has no loop it the group looks like a tree.";
        tooltipURL = "https://en.wikipedia.org/wiki/Free_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n >= 0)
        {
            List<string> gen = new List<string>();
            List<string> Gen = new List<string>(); // Uppercase
            
            for (int i = 0; i < n; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                Gen.Add(((char) ('A' + i)).ToString()) ;
            }
       
            generators = gen.ToArray();
            relators = new string[]{};
        }
    }
}
