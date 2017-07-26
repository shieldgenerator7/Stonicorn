using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour {

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

    public static void addEvent(string newEvent){
        instance.events.Add(newEvent);
    }
    public static bool eventHappened(string eventName)
    {
        if (eventName == null || eventName == "")
        {
            return true;
        }
        return instance.events.Contains(eventName);
    }
}
