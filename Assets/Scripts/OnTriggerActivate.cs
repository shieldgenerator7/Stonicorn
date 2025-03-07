using System.Collections.Generic;
using UnityEngine;

public class OnTriggerActivate : MonoBehaviour
{

    public List<GameObject> objectsToActivate;
    public bool activeOnPlayerIn = true;
    public bool activeOnPlayerOut = false;
    public bool waitForDialogueFinish = true;

    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;

    private bool playerInTrigger = false;

    private void Start()
    {
        playerInTrigger = coll2d
            .OverlapsCollider(Managers.Player.Collider2D);
        //activate objects
        activateObjects((playerInTrigger) ? activeOnPlayerIn : activeOnPlayerOut);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            playerInTrigger = true;
            if (waitForDialogueFinish)
            {
                Managers.Event.OnDialoguePlayingChanged -= _waitForDialogue;
                Managers.Event.OnDialoguePlayingChanged += _waitForDialogue;
            }
            activateObjects(activeOnPlayerIn);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            playerInTrigger = false;
            if (waitForDialogueFinish && Managers.Event.DialoguePlaying)
            {
                Managers.Event.OnDialoguePlayingChanged -= _waitForDialogue;
                Managers.Event.OnDialoguePlayingChanged += _waitForDialogue;
            }
            else
            {
                activateObjects(activeOnPlayerOut);
            }
        }
    }

    void _waitForDialogue(bool playing)
    {
        //cant trust the passed in "playing" variable bc race condition with DialogueChainer
        playing = Managers.Event.DialoguePlaying;
        //
        if (playing)
        {
            activateObjects(activeOnPlayerIn);
        }
        else { 
            activateObjects(activeOnPlayerOut);
            if (!playerInTrigger)
            {
                Managers.Event.OnDialoguePlayingChanged -= _waitForDialogue;
            }
        }
    }

    void activateObjects(bool active)
    {
        objectsToActivate.ForEach(go => go.SetActive(active));
    }
}
