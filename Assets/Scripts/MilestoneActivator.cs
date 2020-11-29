using UnityEngine;

public abstract class MilestoneActivator : MemoryMonoBehaviour
{
    protected override void nowDiscovered()
    {
        activateEffect();
        Destroy(this);//makes sure it can only be used once
    }

    public abstract void activateEffect();

    protected override void previouslyDiscovered()
    {
        Destroy(this);
    }
}
