using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressManager
{
    private readonly Dictionary<string, int> data = new Dictionary<string, int>();
    private readonly List<string> activatedTriggers = new List<string>();

    public void set(string varName, int value = 0)
    {
        verify(varName);
        int oldValue = data[varName];
        data[varName] = value;
        onVariableChange?.Invoke(varName, oldValue, data[varName]);
    }

    public int get(string varName)
    {
        verify(varName);
        return data[varName];
    }

    public void add(string varName, int value = 1)
    {
        verify(varName);
        int oldValue = data[varName];
        data[varName] += value;
        onVariableChange?.Invoke(varName, oldValue, data[varName]);
    }

    public void multiply(string varName, int value = 2)
    {
        verify(varName);
        int oldValue = data[varName];
        data[varName] *= value;
        onVariableChange?.Invoke(varName, oldValue, data[varName]);
    }

    /// <summary>
    /// Makes sure the given variable is in the list
    /// If not, it initializes it to 0
    /// </summary>
    /// <param name="varName"></param>
    private void verify(string varName)
    {
        if (!data.ContainsKey(varName))
        {
            data[varName] = 0;
        }
    }

    public delegate void OnVariableChange(string varName, int oldValue, int newValue);
    public OnVariableChange onVariableChange;

    public void markActivated(string id, bool mark = true)
    {
        if (mark)
        {
            if (!activatedTriggers.Contains(id))
            {
                activatedTriggers.Add(id);
            }
        }
        else
        {
            activatedTriggers.Remove(id);
        }
    }

    public bool hasActivated(string id)
        => activatedTriggers.Contains(id);

    //TODO: modify so it works here in context
    //public List<string> events = new List<string>();//the list of events that have happened
    //public override SavableObject CurrentState
    //{
    //    get
    //    {
    //        SavableObject so = new SavableObject(this);
    //        int counter = 0;
    //        foreach (string str in events)
    //        {
    //            so.more("event" + counter, str);
    //            counter++;
    //        }
    //        so.more("eventCount", counter);
    //        return so;
    //    }
    //    set
    //    {
    //        events = new List<string>();
    //        for (int i = 0; i < value.Int("eventCount"); i++)
    //        {
    //            events.Add(value.String("event" + i));
    //        }
    //    }
    //}

    public List<string> getVariableValues()
    {
        List<string> varVals = data.ToArray().ToList()
            .ConvertAll(kvp => $"{kvp.Key} = {kvp.Value}");
        return varVals;
    }

    public void printVariables()
    {
        foreach (KeyValuePair<string, int> pair in data)
        {
            Debug.Log($"Data[\"{pair.Key}\"] = {pair.Value}");
        }
    }
}
