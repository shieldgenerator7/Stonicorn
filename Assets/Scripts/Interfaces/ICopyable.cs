using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICopyable
{
    System.Type CopyableType { get; }
    void copyFrom(GameObject original);
}
