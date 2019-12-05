using System.Collections;
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
            int resolutionIndex = value;
            Managers.Settings.videoResolution = resolutionIndex;
            Resolution resolution = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Managers.Settings.videoFullScreen);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set the graphics settings to the stored values
        QualityLevel = QualityLevel;
        ResolutionIndex = ResolutionIndex;
    }
}
