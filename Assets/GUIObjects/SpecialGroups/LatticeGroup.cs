using System.Collections.Generic;

public class LatticeGroup : PresentationExample
{
    public LatticeGroup()
    {
        name = "ℤ<sup>n</sup>";
        description = "The additive group of the whole number lattice";
        parameters = new GroupParameter[] {new() {name = "n", value = "2", description = "Dimension of the lattice"}};
        tooltipInfo = "This group represents all n-dimensional vectors with whole number components. The group operation is addition of the vectors. We usually think of this group as being generated by the standard basis vectors (1, 0, 0, ...), (0, 1, 0,...), ... \nThe standard basis vectors are linearly independent therefore we can't represent one basis vector as a linear combination of the other. Groups like these are called \"free Abelian\", since they are those Abelian where the generators are free in a linear algebra sense. Notice though that because the relationship ab=ba holds, meaning this is not an actual free group.";
        tooltipURL = "https://en.wikipedia.org/wiki/Free_abelian_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n >= 0)
        {
            List<string> gen = new List<string>();
            //List<string> Gen = new List<string>(); // Uppercase
            List<string> rel = new List<string>();
            
            for (int i = 0; i < n; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                //Gen.Add(((char) ('A' + i)).ToString()) ;
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
