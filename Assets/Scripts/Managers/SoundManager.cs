using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages which bgm to play.
/// It takes "song requests" from multiple sources and decides which one to play
/// </summary>
public class SoundManager : MonoBehaviour
{//2018-09-17: copied from MusicManager

    public float Volume//for use by other scripts
    {
        get { return Managers.Settings.soundVolume * 100; }
        set
        {
            Managers.Settings.soundVolume = value / 100;
        }
    }
    public bool Mute
    {
        get { return Managers.Settings.soundMute; }
        set
        {
            bool mute = value;
            Managers.Settings.soundMute = mute;
            enabled = !mute;
        }
    }

    [Range(0, 1)]
    public float quietVolumeScaling = 0.5f;//the scale for when it should be quieter
    private float volumeScaling = 1.0f;//how much to scale the volume by (gets reduced when song should be quieter)

    private void Start()
    {
    }

    public void playSound(AudioClip clip, Vector3 pos, float volume = 1)
    {
        if (!Mute)
        {
            AudioSource.PlayClipAtPoint(clip, pos, volume * Managers.Settings.soundVolume * volumeScaling);
        }
    }

    /// <summary>
    /// Sets it quieter than usual if true, regular volume if false
    /// </summary>
    /// <param name="quiet"></param>
    public void setQuiet(bool quiet)
    {
        if (quiet)
        {
            setVolumeScale(quietVolumeScaling);
        }
        else
        {
            setVolumeScale(1.0f);
        }
    }
    void setVolumeScale(float newVolScale)
    {
        volumeScaling = newVolScale;
    }
}
