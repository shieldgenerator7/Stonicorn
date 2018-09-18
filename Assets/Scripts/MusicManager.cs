using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages which bgm to play.
/// It takes "song requests" from multiple sources and decides which one to play
/// </summary>
public class MusicManager : MonoBehaviour
{

    private AudioSource currentSong;//the current song that is playing
    private AudioSource prevSong;//the previous song that was playing

    [Range(0.0f, 1.0f)]
    public float maxVolume = 0.7f;//the loudest it should be
    [SerializeField]
    private bool mute = false;
    public float fadeTime = 2.0f;//how long it should take to fade in or out
    public float eventFadeTime = 0.1f;//how long it takes to fade into and out of event songs
    private float fadeSpeed = 0;//how fast it fades in or out (determined by fadeTime)    
    private bool lockCurrentSong = false;//true to keep the song from being set

    [Range(0, 1)]
    public float quietVolumeScaling = 0.5f;//the scale for when it should be quieter
    private float volumeScaling = 1.0f;//how much to scale the volume by (gets reduced when song should be quieter)
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (fadeSpeed != 0)
        {
            bool finished = true;
            float shift = fadeSpeed * Time.deltaTime;
            if (currentSong)
            {
                currentSong.volume += shift * volumeScaling;
            }
            if (prevSong)
            {
                prevSong.volume -= shift * volumeScaling;
                if (prevSong.volume <= 0)
                {
                    prevSong.Stop();
                }
                else
                {
                    finished = false;
                }
            }
            else if (currentSong && currentSong.volume < maxVolume)
            {
                finished = false;
            }
            if (currentSong && currentSong.volume > maxVolume)
            {
                currentSong.volume = maxVolume * volumeScaling;
            }
            if (finished)
            {
                fadeSpeed = 0;
            }
        }
    }

    public void setCurrentSong(AudioSource newSong)
    {
        if (newSong != currentSong || newSong == null)
        {
            if (!lockCurrentSong)
            {
                prevSong = currentSong;
                currentSong = newSong;
                if (currentSong != null)
                {
                    currentSong.volume = 0 * volumeScaling;
                    currentSong.Play();
                }
                fadeSpeed = 1.0f / fadeTime;
            }
            else
            {
                prevSong = newSong;
            }
        }
    }

    public void setEventSong(AudioSource newSong)
    {
        setCurrentSong(newSong);
        fadeSpeed = 1.0f / eventFadeTime;
        lockCurrentSong = true;
    }
    public void endEventSong(AudioSource song)
    {
        if (song == currentSong)
        {
            lockCurrentSong = false;
            setCurrentSong(prevSong);
            fadeSpeed = 1.0f / eventFadeTime;
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
            setVolumeScale(volumeScaling, quietVolumeScaling);
        }
        else
        {
            setVolumeScale(volumeScaling, 1.0f);
        }
    }
    void setVolumeScale(float oldVolScale, float newVolScale)
    {
        volumeScaling = newVolScale;
        if (currentSong)
        {
            currentSong.volume = currentSong.volume * newVolScale / oldVolScale;
        }
        if (prevSong)
        {
            prevSong.volume = prevSong.volume * newVolScale / oldVolScale;
        }
    }

    public void muteMusic(bool shouldMute)
    {
        mute = shouldMute;
        enabled = !mute;
        if (mute)
        {
            currentSong.Stop();
        }
        else
        {
            currentSong.Play();
        }
    }
}
