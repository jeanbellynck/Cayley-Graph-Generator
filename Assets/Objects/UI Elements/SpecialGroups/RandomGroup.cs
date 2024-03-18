using System.Collections.Generic;

public class RandomGroup : Group
{
    public RandomGroup()
    {
        name = "R<sub>n, m, p</sub>";
        description = "A random group";
        parameters = new GroupParameter[] {new() {name = "n", value = "3", description = "Amount of generators used"}, new() {name = "m", value = "4", description = "Maximal Size of relators"}, new() {name = "p", value = "0.5", description = "Probability that a relator word is included in the presentation"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n >= 1 && int.TryParse(parameters[1].value, out int m) && m >= 1 && float.TryParse(parameters[2].value, out float p) && p >= 0 && p <= 1)
        {
            List<string> gen = new List<string>(); // Lowercase Generators 
            List<string> Gen = new List<string>(); // Uppercase Generators
            List<string> rel = new List<string>(); // Relators
            
            for (int i = 0; i < n; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                Gen.Add(((char) ('A' + i)).ToString()) ;
            }

            // Generates a random set of relators. The Relators have length <= m and are included with probability p
            // Copy the Python script as C# code here:

            
             

            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
