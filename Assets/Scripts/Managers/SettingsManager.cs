using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour, ISetting
{
    //Music Manager
    [Range(0.0f, 1.0f)]
    public float musicVolume = 1;//[0,1] the music volume the user sets
    public bool musicMute = false;
    //Sound Manager
    [Range(0.0f, 1.0f)]
    public float soundVolume = 1;//the sound volume that the user sets
    public bool soundMute = false;
    //NPC Voume
    [Range(0.0f, 1.0f)]
    public float npcVolume = 1;//the npc volume that the user sets
    public bool npcMute = false;
    //Video Settings
    [Range(0, 5)]
    public int videoQuality = 5;
    [Range(0, 100)]
    public int videoResolution = 100;
    public bool videoFullScreen = true;

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
                "npcVolume", npcVolume,
                "npcMute", npcMute,
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
            npcVolume = (float)value.data["npcVolume"];
            npcMute = (bool)value.data["npcMute"];
            videoQuality = (int)value.data["videoQuality"];
            videoResolution = (int)value.data["videoResolution"];
            videoFullScreen = (bool)value.data["videoFullScreen"];
        }
    }
}
