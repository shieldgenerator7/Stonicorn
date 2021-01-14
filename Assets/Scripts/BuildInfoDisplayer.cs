using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BuildInfoDisplayer : MonoBehaviour
{
    public TMP_Text txtBuildVersion;

#if UNITY_EDITOR
    public void updateBuildInfoTexts()
    {
        txtBuildVersion.text = "STONICORN " + PlayerSettings.bundleVersion;
    }
#endif
}
