using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages which bgm to play.
/// It takes "song requests" from multiple sources and decides which one to play
/// </summary>
public class SoundManager : MonoBehaviour
{//2018-09-17: copied from MusicManager

    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float userVolume = 1;//the volume that the user sets
    public float Volume//for use by other scripts
    {
        get { return userVolume * 100; }
        set
        {
            userVolume = value / 100;
        }
    }
    [SerializeField]
    private bool mute = false;
    public bool Mute
    {
        get { return mute; }
        set
        {
            Debug.Log("SoundManager Mute: " + value);
            mute = value;
            enabled = !mute;
        }
    }

    [Range(0, 1)]
    public float quietVolumeScaling = 0.5f;//the scale for when it should be quieter
    private float volumeScaling = 1.0f;//how much to scale the volume by (gets reduced when song should be quieter)

    private static SoundManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            throw new UnityException("There is more than one SoundManager! this: " + name + ", instance: " + instance.name);
        }
    }

    public static void playSound(AudioClip clip, Vector3 pos, float volume = 1)
    {
        if (!instance.mute)
        {
            AudioSource.PlayClipAtPoint(clip, pos, volume * instance.userVolume * instance.volumeScaling);
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
