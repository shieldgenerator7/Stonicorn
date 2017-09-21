using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger: MonoBehaviour {

    public List<string> logObjects = new List<string>();
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
    
    public static void log(GameObject go, string message)
    {
        if (instance.logObjects.Contains(go.name))
        {
            Debug.Log(message + "   (" + go.name + ")");
        }
    }
}
