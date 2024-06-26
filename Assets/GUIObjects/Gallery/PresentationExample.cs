public class PresentationExample {

    public struct GroupParameter {
        public string name;
        public string value;
        public string description;
    }

    public string name;
    public string description;
    public string[] generators;
    public string[] relators;
    public string tooltipInfo = "";
    public string tooltipURL = "";

    public GroupParameter[] parameters;
    public GroupMode groupMode = GroupMode.Group;

    public PresentationExample() {
        this.name = "Group";
        this.description = "Group description";
        this.generators = new[]{"a; b"};
        this.relators = new[]{"abAB"};
        this.parameters = new GroupParameter[]{};
    }
    
    public PresentationExample(string name, string description, string generators, string relators, string tooltipInfo, string tooltipURL, GroupMode groupMode = GroupMode.Group) : this (name, description, generators, relators, groupMode){
        this.tooltipInfo = tooltipInfo;
        this.tooltipURL = tooltipURL;
    }      

    public PresentationExample(string name, string description, string generators, string relators, GroupMode groupMode = GroupMode.Group)
    {
        this.name = name;
        this.description = description;
        this.generators = generators.Replace(" ", "").Split(';');
        this.relators = relators.Replace(" ", "").Split(';');
        this.parameters = new GroupParameter[] { };
        this.groupMode = groupMode;
    }
    
    public PresentationExample(string name, string description, string generators, string relators, GroupParameter[] parameters)
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
