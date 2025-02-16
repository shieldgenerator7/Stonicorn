using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[NonSolid]
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public abstract class EventTrigger : MonoBehaviour, ISetupable
{
    [Tooltip("The title of the dialogue path to play")]
    public string title;

    public virtual bool Interactable => true;

    public bool HasTitle => !string.IsNullOrWhiteSpace(title);

    [AutoInitialize, SerializeField, HideInInspector]
    private Collider2D coll2d;

    // Start is called before the first frame update
    protected virtual void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            activateTrigger();
        }
    }

    private void triggerEventOnPlay(bool paused)
    {
        if (!paused)
        {
            Managers.Time.onPauseChanged -= triggerEventOnPlay;
            triggerEvent();
        }
    }

    protected abstract void triggerEvent();

    public void ActivateTrigger()
    {
        //early exit: cant confirm the player can trigger it
        if (!coll2d.OverlapPoint(Managers.Player.transform.position))
        {
            return;
        }
        //processing
        activateTrigger();
    }
    public void ForceActivateTrigger()
    {
        activateTrigger();
    }
    private void activateTrigger() {
        if (!Managers.Time.Paused)
        {
            triggerEvent();
        }
        else
        {
            Managers.Time.onPauseChanged += triggerEventOnPlay;
        }
    }

    public virtual int checkForErrors()
    {
        return 0;
    }
    public int setup()
    {
        return 0;
    }
}
