public class CyclicGroup : Group
{
    public CyclicGroup()
    {
        name = "C<sub>n</sub>";
        description = "The cyclic groups";
        parameters = new string[][] {new string[] {"n", "2", "Order of the cyclic group"}};
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0][1], out int n) && n >= 1)
        {
            generators = new string[]{"a"};
            string relator = "";
            for (int i = 0; i < int.Parse(parameters[0][1]); i++)
            {
                relator += "a";
            }
            relators = new string[]{relator};
        }
    }
}
