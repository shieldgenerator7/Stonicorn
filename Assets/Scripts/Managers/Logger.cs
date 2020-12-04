using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger: MonoBehaviour {

    public List<GameObject> logObjects = new List<GameObject>();
    static Logger instance;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void log(MonoBehaviour mb, string message)
    {
        log(mb.gameObject, message);
    }

    public static void log(GameObject go, string message)
    {
        if (instance && instance.logObjects.Contains(go))
        {
            Debug.Log(go.name + " >>> "+ message);
        }
    }
}
