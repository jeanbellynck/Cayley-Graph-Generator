using System;
using System.Collections.Generic;
using System.Globalization;

public class RandomGroup : Group
{
    public RandomGroup()
    {
        name = "R<sub>n, m, p</sub>";
        description = "A random group";
        parameters = new string[][] {new string[] {"n", "3", "Amount of generators used"}, new string[] {"m", "4", "Maximal Size of relators"}, new string[] {"p", "0.5", "Probability that a relator word is included in the presentation"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 1 && int.TryParse(parameters[1][1], out int m) && m >= 1 && float.TryParse(parameters[2][1], out float p) && p >= 0 && p <= 1)
        {
            List<string> gen = new List<string>();
            List<string> Gen = new List<string>(); // Uppercase
            List<string> rel = new List<string>();
            
            for (int i = 0; i < n; i++)
            {
                gen.Add(((char) ('a' + i)).ToString()) ;
                Gen.Add(((char) ('A' + i)).ToString()) ;
            }

            // All possible words of length of length i
            List<List<string>> possibleWords = new List<List<string>>(m);
            // Add Generators to 1-length-words
            possibleWords.Add(new List<string>());
            possibleWords[0].AddRange(gen);
            possibleWords[0].AddRange(Gen);
            for(int i = 1; i <= m; i++)
            {
                possibleWords.Add(new List<string>());
                foreach(string word in possibleWords[i-1])
                {
                    for(int k = 0; k < gen.Count; k++)
                    {
                        if(word[i-1] != Gen[k][0]) possibleWords[i].Add(word + gen[k]);
                        if(word[i-1] != gen[k][0]) possibleWords[i].Add(word + Gen[k]);
                    }
                }
            }
            List<string> words = new List<string>();
            foreach(List<string> list in possibleWords)
            {
                words.AddRange(list);
            }

            Random random = new Random();
            // Add relators
            for(int i = 1; i < words.Count; i++)
            {
                if(random.NextDouble() < p)
                {
                    rel.Add(words[i]);
                }
            }

            generators = gen.ToArray();
            relators = rel.ToArray();
        }
    }
}
