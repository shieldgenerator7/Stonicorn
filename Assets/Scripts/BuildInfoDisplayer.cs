using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildInfoDisplayer : MonoBehaviour
{
    public TMP_Text txtBuildVersion;
    public List<string> buildMessages;

    private void Start()
    {
        updateBuildInfoTexts();
    }

    public void updateBuildInfoTexts()
    {
        string text = Application.productName.ToUpper() + " " + Application.version;
        buildMessages.ForEach(m => text += "\n[" + m + "]");
        txtBuildVersion.text = text;
    }
}
