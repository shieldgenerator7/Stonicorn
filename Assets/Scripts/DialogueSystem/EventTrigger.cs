using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class EventTrigger : MonoBehaviour
{
    [Tooltip(
        "The unique id for this individual trigger. " +
        "Used to determine whether this has ever been triggered."
        )]
    public int id = -1;
    public string IdString => $"{gameObject.scene.name}_{id}";

    public AudioClip triggerSound;

    public virtual bool Interactable => true;

    private Collider2D coll2d;
    protected DialogueManager dialogueManager;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        coll2d = GetComponents<Collider2D>().FirstOrDefault(c2d => c2d.isTrigger == true);
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (!coll2d)
        {
            Debug.LogError(
                $"{this.GetType().Name} requires a Collider2D with isTrigger set to true. " +
                $"This one on GameObject {gameObject.name} has none.",
                this
                );
        }
        //Id must be valid
        if (id < 0)
        {
            Debug.LogError(
                $"EventTrigger Id is invalid on object {gameObject.name}" +
                $" in scene {gameObject.scene.name}. " +
                $"Id must be 0 or greater (Assign it an Id). " +
                $"Invalid value: {id}",
                this
                );
        }
        //Id must be unique
        EventTrigger dupIdTrigger = FindObjectsOfType<EventTrigger>()
            .FirstOrDefault(
                t => t.gameObject.scene == this.gameObject.scene
                    && t.id == this.id
                    && t != this
            );
        if (dupIdTrigger)
        {
            Debug.LogError(
                $"EventTrigger has a duplicate Id! " +
                $"GameObject {gameObject.name} and {dupIdTrigger.name}" +
                $" in scene {gameObject.scene.name}" +
                $" have the same Id: {this.id}. " +
                $"Assign these EventTriggers unique Ids.",
                this
                );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (this.Interactable)
            {
                InteractUI.instance.registerTrigger(this, true);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InteractUI.instance.registerTrigger(this, false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InteractUI.instance.checkClosestTrigger();
        }
    }

    private void OnDestroy()
    {
        InteractUI.instance.registerTrigger(this, false);
    }

    public void processTrigger()
    {
        dialogueManager.progressManager.markActivated(this);
        AudioSource.PlayClipAtPoint(triggerSound, transform.position);
        triggerEvent();
    }

    protected abstract void triggerEvent();
}
