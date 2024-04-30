public class TorusGroup : PresentationExample
{
    public TorusGroup()
    {
        name = "C<sub>m</sub> × C<sub>n</sub>";
        description = "A direct product of two cyclic group, shaped like a torus";
        parameters = new GroupParameter[] {new() {name = "m", value = "5", description = "Order of the first cyclic group"}, new() {name = "n", value = "15", description = "Order of the second cyclic group"}};
        tooltipInfo = "The direct product takes two groups and constructs a new one. Similar to a 2-vector its elements can be written as (a, b). Group multiplication is performed independently over both components. Direct products usually have a grid-like appearance.";
        tooltipURL = "https://en.wikipedia.org/wiki/Direct_product_of_groups";

        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (int.TryParse(parameters[0].value, out int m) && m >= 1 && int.TryParse(parameters[1].value, out int n) && n >= 1)
        {
            generators = new string[]{"a", "b"};
            relators = new string[]{"abAB", "a^" + m, "b^" + n};
        }
    }
}
