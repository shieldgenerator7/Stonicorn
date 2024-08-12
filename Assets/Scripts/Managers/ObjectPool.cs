using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
    private List<T> loanedList = new List<T>();//list of objects checked out
    private List<T> pool = new List<T>();//list of objects not being used

    private Func<T> createFunc;
    private Action<T, bool> enableFunc;

    public ObjectPool(Func<T> createFunc) : this(createFunc, (obj, enable) => { })
    {
    }
    public ObjectPool(Func<T> createFunc, Action<T, bool> enableFunc)
    {
        this.createFunc = createFunc;
        this.enableFunc = enableFunc;
    }

    public T checkoutObject()
    {
        //Create a new t
        if (pool.Count == 0)
        {
            T t = createFunc();
            loanedList.Add(t);
            return t;
        }
        //Use an existing unused t
        else
        {
            T t = pool[0];
            pool.RemoveAt(0);
            enableFunc(t, true);
            loanedList.Add(t);
            return t;
        }
    }

    public void returnObject(T t)
    {
        loanedList.Remove(t);
        enableFunc(t, false);
        pool.Add(t);
    }
}
