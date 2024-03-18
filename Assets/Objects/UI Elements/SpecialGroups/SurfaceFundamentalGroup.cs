using System.Linq;

public class SurfaceFundamentalGroup : Group
{
    public SurfaceFundamentalGroup()
    {
        name = "π<sub>1</sub>(S<sub>g</sub>)";
        description = "The fundamental group of the genus-g-surface";
        parameters = new GroupParameter[] {new() { name = "g", value = "2", description = "Genus"}};
        tooltipInfo = "This fundamental group can be found as an amalgamated product by Van-Kampen.";
        tooltipURL = "https://en.wikipedia.org/wiki/Direct_product_of_groups";

        updatePresentation();
    }


    public override void updatePresentation() {
        // A non-negative number integer 
        if (!int.TryParse(parameters[0].value, out int g) || g < 1 || g > 13) return;
        generators = (from i in Enumerable.Range(0, 2*g) select ((char)('a' + i)).ToString()).ToArray();
        relators = new[] {
            string.Join("", from j in Enumerable.Range(0, g) select "[" + (char)('a' + 2*j) + "," + (char)('a' + 2*j + 1) + "]")
        };
    }
}
