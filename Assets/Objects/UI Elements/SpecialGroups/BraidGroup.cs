using System.Collections.Generic;
using System.Globalization;

public class BraidGroup : Group
{
    public BraidGroup()
    {
        name = "B<sub>n</sub>";
        description = "The briad groups";
        parameters = new string[][] {new string[] {"n", "3", "Amount of strands the braid groups acts on"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 1)
        {
            List<string> gen = new List<string>();
            List<string> Gen = new List<string>(); // Uppercase
            List<string> rel = new List<string>();
            
            for (int i = 0; i < n-1; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                Gen.Add(((char) ('A' + i)).ToString()) ;
            }
            for (int i = 0; i < n-1; i++)
            {
                for (int j = 0; j < i-1; j++)
                {
                    rel.Add(gen[i]+gen[j]+Gen[i]+Gen[j]);
                }
            }
            for (int i = 0; i < n-2; i++)
            {
                rel.Add(gen[i]+gen[i+1]+gen[i]+Gen[i+1]+Gen[i]+Gen[i+1]);
            }
       
            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
