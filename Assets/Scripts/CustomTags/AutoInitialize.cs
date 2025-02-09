using System;
//2025-02-08: made with help from https://stackoverflow.com/a/4879579/2336212

[AttributeUsage(AttributeTargets.Field)]
public class AutoInitialize:Attribute
{
    public bool SearchParent = false;
    public bool SearchChildren = false;
    public bool SearchScene = false;
    public bool AllowUnfound = false;
}
