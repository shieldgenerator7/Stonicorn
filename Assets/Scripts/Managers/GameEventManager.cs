using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : SavableMonoBehaviour
{

    public List<string> events = new List<string>();//the list of events that have happened

    private static GameEventManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public override SavableObject CurrentState
    {
        get
        {
            SavableObject so = new SavableObject(this);
            int counter = 0;
            foreach (string str in events)
            {
                so.more("event" + counter, str);
                counter++;
            }
            so.more("eventCount", counter);
            return so;
        }
        set
        {
            events = new List<string>();
            for (int i = 0; i < value.Int("eventCount"); i++)
            {
                events.Add(value.String("event" + i));
            }
        }
    }

    public static void addEvent(string newEvent)
    {
        if (newEvent != null
            && newEvent != ""
            && !instance.events.Contains(newEvent))
        {
            instance.events.Add(newEvent.Trim());
        }
    }
    public static bool eventHappened(string eventName)
    {
        if (eventName == null || eventName.Trim() == "")
        {
            return true;
        }
        return instance.events.Contains(eventName.Trim());
    }
}
