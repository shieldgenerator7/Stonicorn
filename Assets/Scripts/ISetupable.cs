using UnityEngine;

/// <summary>
/// A class is setupable if it has code that needs to run during the pre-build steps
/// </summary>
public interface ISetupable
{
#if UNITY_EDITOR

    /// <summary>
    /// Checks for errors, and reports how many there were
    /// </summary>
    public int checkForErrors() { return 0; }

    /// <summary>
    /// Setup, returns how many changes were made, 0 if no changes were made
    /// </summary>
    /// <returns></returns>
    public int setup();

    /// <summary>
    /// Checks for errors after setup is complete, and reports how many there were
    /// </summary>
    public int checkForErrorsPostSetup() { return 0; }

#endif
}
