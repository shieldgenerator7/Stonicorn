using System;

/// <summary>
/// A class implements this interface when it needs to know when it has been swapped.
/// Despite the name's implication, things can be swapped that don't implement this interface.
/// Objects with ISwappable scripts still need to have a RigidBody2D in order to be swappable.
/// </summary>
public interface ISwappable
{
    /// <summary>
    /// This method is called when this object is swapped
    /// </summary>
    void nowSwapped();
}
