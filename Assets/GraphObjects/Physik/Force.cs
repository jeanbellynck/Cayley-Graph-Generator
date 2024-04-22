using System.Collections;

public abstract class Force{
    public abstract IEnumerator ApplyForce(LabelledGraphManager graphManager, float alpha);
}