using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour, ISetting
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

    public void saveSettings(string filename)
    {
        ES3.Save<SettingObject>("settings", Setting, this.fileName);
    }

    public void loadSettings(string filename)
    {
        if (ES3.FileExists(this.fileName))
        {
            Setting = ES3.Load<SettingObject>("settings", this.fileName);
        }
    }

    public SettingScope Scope
    {
        get => SettingScope.GAME_WHOLE;
    }

    public string ID
    {
        get => GetType().Name;
    }

    public SettingObject Setting
    {
        get
        {
            return new SettingObject(ID,
                "musicVolume", musicVolume,
                "musicMute", musicMute,
                "soundVolume", soundVolume,
                "soundMute", soundMute,
                "videoQuality", videoQuality,
                "videoResolution", videoResolution,
                "videoFullScreen", videoFullScreen
                );
        }
        set
        {
            musicVolume = (float)value.data["musicVolume"];
            musicMute = (bool)value.data["musicMute"];
            soundVolume = (float)value.data["soundVolume"];
            soundMute = (bool)value.data["soundMute"];
            videoQuality = (int)value.data["videoQuality"];
            videoResolution = (int)value.data["videoResolution"];
            videoFullScreen = (bool)value.data["videoFullScreen"];
        }
    }
}
