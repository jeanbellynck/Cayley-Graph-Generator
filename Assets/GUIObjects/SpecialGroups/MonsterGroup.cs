using System.Collections.Generic;

public class MonsterGroup : PresentationExample
{
    public MonsterGroup()
    {
        name = "M × C<sub>2</sub>";
        description = "Product of the monster group (very big) with a cyclic group, a very long relator is not included";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        List<string> gen = new List<string>();
        List<string> Gen = new List<string>(); // Uppercase
        List<string> rel = new List<string>();
        
        for (int i = 0; i < 12; i++)
        {
            gen.Add(((char) ('a' + i)).ToString()) ;
            Gen.Add(((char) ('A' + i)).ToString()) ;
        }
        
        for (int i = 0; i < 11; i++)
        {
            rel.Add(gen[i] + gen[i+1] + gen[i] + gen[i+1] + gen[i] + gen[i+1]);
        }
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < i-1; j++)
            {
                rel.Add(gen[i]+gen[j]+Gen[i]+Gen[j]);
            }
        }
        for (int i = 0; i < 12; i++)
        {
            rel.Add(gen[i]+gen[i]);
        }
    
        generators = gen.ToArray();
        relators = rel.ToArray();
    }
}
