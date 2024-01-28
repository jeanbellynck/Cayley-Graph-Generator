using System.Collections.Generic;
using System.Globalization;

public class FreeGroup : Group
{
    public FreeGroup()
    {
        name = "F<sup>n</sup>";
        description = "The free group";
        parameters = new string[][] {new string[] {"n", "2", "Symbols in the free group"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 0)
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
