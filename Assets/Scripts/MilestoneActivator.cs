using UnityEngine;

public abstract class MilestoneActivator : MemoryMonoBehaviour
{
    public AudioClip activateSound;

    protected override void nowDiscovered()
    {
        activateEffect();
        if (activateSound)
        {
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }
        Destroy(this);//makes sure it can only be used once
        //Save game
        Managers.File.saveToFile();
    }

    public abstract void activateEffect();

    protected override void previouslyDiscovered()
    {
        Destroy(this);
    }
}
