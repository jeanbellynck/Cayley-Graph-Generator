public class CyclicGroup : PresentationExample
{
    public CyclicGroup()
    {
        name = "C<sub>n</sub>";
        description = "The cyclic groups";
        parameters = new GroupParameter[] {new() {name = "n", value = "2", description = "Order of the cyclic group"}};
        tooltipInfo = "The cyclic group of order n is the group of integers modulo n under addition. It can be understood as numbers on a clock.";
        tooltipURL = "https://en.wikipedia.org/wiki/Cyclic_group";
        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int n) && n >= 1)
        {
            generators = new string[]{"a"};
            string relator = "a^" + n.ToString();
            relators = new string[]{relator};
        }
    }
}
