using System.Collections;

public abstract class Force{
    public abstract IEnumerator ApplyForce(GraphManager graphManager, float alpha);
}