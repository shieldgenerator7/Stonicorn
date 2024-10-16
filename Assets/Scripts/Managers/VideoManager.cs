﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    public int QualityLevel
    {
        get => Managers.Settings.videoQuality;
        set
        {
            int quality = value;
            Managers.Settings.videoQuality = quality;
            QualitySettings.SetQualityLevel(quality);
        }
    }

    public int ResolutionIndex
    {
        get => Managers.Settings.videoResolution;
        set
        {
            int resolutionIndex = Mathf.Clamp(
                value,
                0,
                Screen.resolutions.Length - 1
                );
            Managers.Settings.videoResolution = resolutionIndex;
            Resolution resolution = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Managers.Settings.videoFullScreen);
        }
    }

    public bool Windowed
    {
        get => !Managers.Settings.videoFullScreen;
        set
        {
            bool full = !value;
            Managers.Settings.videoFullScreen = full;
            Screen.fullScreen = full;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set the graphics settings to the stored values
        QualityLevel = QualityLevel;
        ResolutionIndex = ResolutionIndex;
        Windowed = Windowed;
    }
}
