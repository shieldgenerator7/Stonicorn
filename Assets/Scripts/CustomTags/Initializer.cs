using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class Initializer:Attribute
{
    //the name of the variable that will be initialized by the return value of this method
    public string name;
    public int order = 0;

    public Initializer()
    {
        this.name = null;
    }
    public Initializer(string name)
    {
        this.name = name;
    }
    public Initializer(int order)
    {
        this.order = order;
    }
}
