using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoManager : MonoBehaviour
{
    public int Quality
    {
        get => Managers.Settings.videoQuality;
        set
        {
            int quality = value;
            Managers.Settings.videoQuality = quality;
            QualitySettings.SetQualityLevel(quality);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.SetQualityLevel(Quality);
    }
}
