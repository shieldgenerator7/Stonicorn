using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : SavableMonoBehaviour
{
    [SerializeField]
    private string fileName = "merky_settings.txt";
    //Music Manager
    [Range(0.0f, 1.0f)]
    public float musicVolume = 1;//[0,1] the music volume the user sets
    public bool musicMute = false;
    //Sound Manager
    [Range(0.0f, 1.0f)]
    public float soundVolume = 1;//the sound volume that the user sets
    public bool soundMute = false;
    //Video Settings
    [Range(0, 5)]
    public int videoQuality = 0;
    [Range(0, 30)]
    public int videoResolution = 13;
    public bool videoFullScreen = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void saveSettings()
    {
        SavableObject so = getSavableObject();
        ES3.Save<SavableObject>("settings", so, fileName);
    }

    public void loadSettings()
    {
        SavableObject so = ES3.Load<SavableObject>("settings", fileName);
        acceptSavableObject(so);
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "musicVolume", musicVolume,
            "musicMute", musicMute,
            "soundVolume", soundVolume,
            "soundMute", soundMute,
            "videoQuality", videoQuality,
            "videoResolution", videoResolution,
            "videoFullScreen", videoFullScreen
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        musicVolume = (float)savObj.data["musicVolume"];
        musicMute = (bool)savObj.data["musicMute"];
        soundVolume = (float)savObj.data["soundVolume"];
        soundMute = (bool)savObj.data["soundMute"];
        videoQuality = (int)savObj.data["videoQuality"];
        videoResolution = (int)savObj.data["videoResolution"];
        videoFullScreen = (bool)savObj.data["videoFullScreen"];
    }
}
