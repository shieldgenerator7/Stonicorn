﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class EventTrigger : MonoBehaviour
{
    [Tooltip("The title of the dialogue path to play")]
    public string title;

    public virtual bool Interactable => true;

    public bool HasTitle => !string.IsNullOrWhiteSpace(title);

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

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            if (!Managers.Time.Paused)
            {
                triggerEvent();
            }
            else
            {
                Managers.Time.onPauseChanged += triggerEventOnPlay;
            }
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
}
