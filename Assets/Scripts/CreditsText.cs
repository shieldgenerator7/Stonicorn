using TMPro;
using UnityEngine;

public class CreditsText : MonoBehaviour, ISetupable
{
    public TextAsset _textAsset;
    public TMP_Text txtCredits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        load();
    }

    public void load()
    {
        txtCredits.text = _textAsset.text;
    }

    public int setup()
    {
        int changeCount = 0;

        if (txtCredits.text != _textAsset.text)
        {
            load();
            changeCount++;
        }

        return changeCount;
    }
}
