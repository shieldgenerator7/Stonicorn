using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages which bgm to play.
/// It takes "song requests" from multiple sources and decides which one to play
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Range(0, 1)]
    public float maxVolume = 1;

    private AudioSource currentSong;//the current song that is playing
    private AudioSource prevSong;//the previous song that was playing

    /// <summary>
    /// The user-set Volume for use by other scripts. Range: [0, 100]
    /// </summary>
    public float Volume
    {
        get { return Managers.Settings.musicVolume * 100; }
        set
        {
            Managers.Settings.musicVolume = value / 100;
            updateVolume();
        }
    }
    public bool Mute
    {
        get { return Managers.Settings.musicMute; }
        set
        {
            bool mute = value;
            Managers.Settings.musicMute = mute;
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

    public void playFirstSong()
    {
        lockCurrentSong = false;
        //
        FindObjectsOfType<MusicZone>().ToList()
            .ForEach(mz => mz.checkZone(Managers.Player.transform.position));
        //
        fadeStartTime = 0;
        fadePercent = 1;
        currentFadeDuration = 0;
        updateVolume();
    }

    public void processFade()
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
            if (prevSong)
            {
                prevSong.Stop();
            }
            if (!lockCurrentSong)
            {
                prevSong = currentSong;
                currentSong = newSong;
                startSongFade();
                updateVolume();
                if (currentSong)
                {
                    currentSong.Play();
                }
            }
            else
            {
                prevSong = newSong;
            }
            if (currentSong)
            {
                currentSong.pitch = songSpeed;
            }
            if (prevSong)
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
            currentSong.volume = Managers.Settings.musicVolume * fadePercent * volumeScaling * maxVolume;
        }
        if (prevSong)
        {
            prevSong.volume = Managers.Settings.musicVolume * (1 - fadePercent) * volumeScaling * maxVolume;
        }
    }
}
