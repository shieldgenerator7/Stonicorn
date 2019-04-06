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
    [SerializeField]
    private float userVolume = 1;//[0,1] the volume the user sets
    /// <summary>
    /// The user-set Volume for use by other scripts. Range: [0, 100]
    /// </summary>
    public float Volume
    {
        get { return userVolume * 100; }
        set
        {
            userVolume = value / 100;
            updateVolume();
        }
    }
    [SerializeField]
    private bool mute = false;
    public bool Mute
    {
        get { return mute; }
        set
        {
            mute = value;
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
    public float fadeDuration = 2.0f;//how long it should take to fade in or out
    public float eventFadeDuration = 0.1f;//how long it takes to fade into and out of event songs
    //Fade Runtime Vars
    private float currentFadeDuration = 0;//bounces between fadeDuration and eventFadeDuration
    private float fadePercent = 0;//how far along the fade it is
    private float fadeStartTime = 0;//the point in time when the fade started
    private bool lockCurrentSong = false;//true to keep the song from being set

    [Range(0, 1)]
    public float quietVolumeScaling = 0.5f;//the scale for when it should be quieter
    private float volumeScaling = 1.0f;//how much to scale the volume by (gets reduced when song should be quieter)
    public float VolumeScaling
    {
        get { return volumeScaling; }
        set
        {
            volumeScaling = value;
            updateVolume();
        }
    }
    [Range(-3, 3)]
    private float songSpeed = 1.0f;//how fast the song should play
    public float SongSpeed
    {
        get { return songSpeed; }
        set
        {
            songSpeed = value;
            if (currentSong)
            {
                currentSong.pitch = songSpeed;
            }
        }
    }
    public float normalSongSpeed = 1;
    public float rewindSongSpeed = -1.5f;

    // Update is called once per frame
    void Update()
    {
        if (fadePercent < 1)
        {
            fadePercent = (Time.time - fadeStartTime) / currentFadeDuration;
            fadePercent = Mathf.Clamp(fadePercent, 0, 1);
            updateVolume();
            if (Time.time > fadeStartTime + currentFadeDuration)
            {
                fadeStartTime = 0;
                fadePercent = 1;
                currentFadeDuration = 0;
                if (prevSong)
                {
                    prevSong.Stop();
                }
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
                startSongFade();
                updateVolume();
                if (currentSong != null)
                {
                    currentSong.Play();
                }
            }
            else
            {
                prevSong = newSong;
            }
            currentSong.pitch = songSpeed;
            if (prevSong != null)
            {
                prevSong.pitch = songSpeed;
            }
        }
    }

    public void setEventSong(AudioSource newSong)
    {
        setCurrentSong(newSong);
        startSongFade(true);
        lockCurrentSong = true;
    }
    public void endEventSong(AudioSource song)
    {
        if (song == currentSong)
        {
            lockCurrentSong = false;
            setCurrentSong(prevSong);
            startSongFade(true);
        }
    }
    void startSongFade(bool eventSong = false)
    {
        fadeStartTime = Time.time;
        currentFadeDuration = (eventSong) ? eventFadeDuration : fadeDuration;
        fadePercent = 0;
    }
    /// <summary>
    /// Sets it quieter than usual if true, regular volume if false
    /// </summary>
    /// <param name="quiet"></param>
    public bool Quiet
    {
        get
        {
            return VolumeScaling == quietVolumeScaling;
        }
        set
        {
            bool quiet = value;
            if (quiet)
            {
                VolumeScaling = quietVolumeScaling;
            }
            else
            {
                VolumeScaling = 1.0f;
            }
        }
    }

    void updateVolume()
    {
        if (currentSong)
        {
            currentSong.volume = userVolume * fadePercent * volumeScaling;
        }
        if (prevSong)
        {
            prevSong.volume = userVolume * (1 - fadePercent) * volumeScaling;
        }
    }
}
