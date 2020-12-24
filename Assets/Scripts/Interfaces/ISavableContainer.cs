using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implement this interface to declare that this object has child objects to save,
/// but is not a savable itself
/// </summary>
public interface ISavableContainer
{
    List<GameObject> Savables { get; }
}
