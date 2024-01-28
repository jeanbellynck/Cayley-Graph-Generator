using System.Collections.Generic;
using System.Globalization;

public class LatticeGroup : Group
{
    public LatticeGroup()
    {
        name = "ℤ<sup>n</sup>";
        description = "The additive group of the whole number lattice";
        parameters = new string[][] {new string[] {"n", "2", "Dimension of the lattice"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 0)
        {
            List<string> gen = new List<string>();
            List<string> Gen = new List<string>(); // Uppercase
            List<string> rel = new List<string>();
            
            for (int i = 0; i < n; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                Gen.Add(((char) ('A' + i)).ToString()) ;
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    rel.Add("[" + gen[j] +","+ gen[i]+ "]");
                }
            }
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
