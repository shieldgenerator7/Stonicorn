using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public abstract class EventTrigger : MemoryMonoBehaviour
{
    public virtual bool Interactable => true;

    private Collider2D coll2d;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        checkErrors();
    }

    protected virtual void checkErrors()
    {
        coll2d = GetComponents<Collider2D>().FirstOrDefault(c2d => c2d.isTrigger == true);
        if (!coll2d)
        {
            Debug.LogError(
                $"{this.GetType().Name} requires a Collider2D with isTrigger set to true. " +
                $"This one on GameObject {gameObject.name} has none.",
                this
                );
        }
    }

    protected override void nowDiscovered()
    {
        triggerEvent();
    }

    protected override void previouslyDiscovered()
    {
    }

    protected abstract void triggerEvent();
}
