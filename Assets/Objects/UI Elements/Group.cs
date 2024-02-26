
using Unity.VisualScripting;

public class Group {
    
    public string name;
    public string description;
    public string[] generators;
    public string[] relators;
    public string tooltipInfo = "";
    public string tooltipURL = "";

    public string[][] parameters;

    public Group() {
        this.name = "Group";
        this.description = "Group description";
        this.generators = new string[]{"a; b"};
        this.relators = new string[]{"abAB"};
        this.parameters = new string[0][];
    }
    
    public Group(string name, string description, string generators, string relators, string tooltipInfo, string tooltipURL) : this (name, description, generators, relators){
        this.tooltipInfo = tooltipInfo;
        this.tooltipURL = tooltipURL;
    }      

    public Group(string name, string description, string generators, string relators)
    {
        this.name = name;
        this.description = description;
        this.generators = generators.Replace(" ", "").Split(';');
        this.relators = relators.Replace(" ", "").Split(';');
        this.parameters = new string[0][];
    }
    
    public Group(string name, string description, string generators, string relators, string[][] parameters)
    {
        this.name = name;
        this.description = description;
        this.generators = generators.Replace(" ", "").Split(';');
        this.relators = relators.Replace(" ", "").Split(';');
        this.parameters = parameters;
    }
 

    /**
     * This method recalculates the presentation of the group based on the parameters.
     * This is only used for groups with parameters.
     **/
    public virtual void updatePresentation() {
        
    }
}
