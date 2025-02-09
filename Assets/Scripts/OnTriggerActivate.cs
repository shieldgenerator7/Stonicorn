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

    private void Start()
    {
        bool playerInTrigger = coll2d
            .OverlapsCollider(Managers.Player.Collider2D);//dirty: assumes player is using PolygonCollider2D
        //activate objects
        activateObjects((playerInTrigger) ? activeOnPlayerIn : activeOnPlayerOut);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
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
        if (!playing)
        {
            activateObjects(activeOnPlayerOut);
            Managers.Event.OnDialoguePlayingChanged -= _waitForDialogue;
        }
    }

    void activateObjects(bool active)
    {
        objectsToActivate.ForEach(go => go.SetActive(active));
    }
}
