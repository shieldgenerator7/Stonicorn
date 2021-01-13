using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraZoomRange
{
    /// <summary>
    /// The minimum zoom scale point that defines the zoom level activation trigger
    /// </summary>
    public CameraController.CameraScalePoints minZoomScalePoint = 0;
    /// <summary>
    /// The maximum zoom scale point that defines the zoom level activation trigger
    /// </summary>
    public CameraController.CameraScalePoints maxZoomScalePoint = CameraController.CameraScalePoints.TIMEREWIND;
    public enum ClusivityOption
    {
        INCLUSIVE,
        EXCLUSIVE
    }
    public ClusivityOption minZoomClusivity = ClusivityOption.INCLUSIVE;
    public ClusivityOption maxZoomClusivity = ClusivityOption.INCLUSIVE;
}
