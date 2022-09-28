using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    public static InteractUI instance { get; private set; }

    private readonly List<EventTrigger> triggers = new List<EventTrigger>();

    private List<MonoBehaviour> _suppressors = new List<MonoBehaviour>();
    public bool Suppressed => _suppressors.Count > 0;


    private EventTrigger currentTrigger;
    private Vector2 offset = Vector2.zero;

    public float distanceFromScreenTop = 0.4f;

    private void Start()
    {
        if (instance != null)
        {
            Debug.LogError("InteractUI is already bound!");
            Destroy(instance.gameObject);
        }
        instance = this;
        gameObject.SetActive(false);
        SceneManager.sceneUnloaded += (s) => triggers.Clear();
        PlayerController.OnPlayerInteract += interactPressed;
    }

    public void interactPressed()
    {
        if (!Suppressed)
        {
            currentTrigger?.processTrigger();
        }
    }

    public void registerTrigger(EventTrigger trigger, bool register = true)
    {
        if (register)
        {
            if (!triggers.Contains(trigger))
            {
                triggers.Add(trigger);
            }
        }
        else
        {
            triggers.Remove(trigger);
        }
        updateInteractUI();
    }

    /// <summary>
    /// Recheck which registered trigger is closer,
    /// and thus should have precedent
    /// </summary>
    public void checkClosestTrigger()
    {
        if (triggers.Count >= 2)
        {
            updateInteractUI();
        }
    }

    private void updateInteractUI()
    {
        currentTrigger = null;
        bool canShow = triggers.Count > 0 && !Suppressed;
        gameObject.SetActive(canShow);
        if (canShow)
        {
            selectTrigger();
            SpriteRenderer triggerSR = findSpriteRenderer(currentTrigger);
            if (triggerSR)
            {
                offset = new Vector2(
                    0,
                    triggerSR.bounds.max.y - currentTrigger.transform.position.y
                    );
            }
        }
    }

    private void Update()
    {
        //Move interact UI to the new position
        transform.position = (Vector2) currentTrigger.transform.position + offset;
        //Keep the interact UI on screen
        float maxY = Camera.main.ViewportToWorldPoint(Vector2.up).y + distanceFromScreenTop;
        if (transform.position.y > maxY)
        {
            transform.position = new Vector2(transform.position.x, maxY);
        }
    }

    private void selectTrigger()
    {
        if (triggers.Count > 0)
        {
            Vector2 playerPos = FindObjectOfType<PlayerController>().transform.position;
            currentTrigger = triggers.OrderBy(
                t => Vector2.Distance(
                    playerPos,
                    t.GetComponent<Collider2D>().bounds.center
                    )
                ).FirstOrDefault();
        }
        else
        {
            currentTrigger = null;
        }
    }

    private SpriteRenderer findSpriteRenderer(EventTrigger trigger)
    {
        SpriteRenderer triggerSR = trigger.GetComponent<SpriteRenderer>();
        if (triggerSR)
        {
            return triggerSR;
        }
        triggerSR = trigger.GetComponentInChildren<SpriteRenderer>();
        if (triggerSR)
        {
            return triggerSR;
        }
        triggerSR = trigger.GetComponentInParent<SpriteRenderer>();
        if (triggerSR)
        {
            return triggerSR;
        }
        return null;
    }

    /// <summary>
    /// Checks each registered EventTrigger to make sure it's interactable
    /// </summary>
    public void refreshTriggerList()
    {
        for (int i = triggers.Count - 1; i >= 0; i--)
        {
            EventTrigger trigger = triggers[i];
            //If trigger isn't interactable anymore,
            if (!trigger.Interactable)
            {
                //Unregister it
                registerTrigger(trigger, false);
            }
        }
    }

    public void suppress(MonoBehaviour suppressor, bool suppress = true)
    {
        if (suppress)
        {
            if (!_suppressors.Contains(suppressor))
            {
                _suppressors.Add(suppressor);
            }
        }
        else
        {
            _suppressors.Remove(suppressor);
        }
        updateInteractUI();
    }

}
