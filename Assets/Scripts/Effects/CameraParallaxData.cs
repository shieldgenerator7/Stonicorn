using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParallaxData : MonoBehaviour
{
    public int sortingOrder = 0;

    private Vector3 startPosition;
    public Vector3 StartPosition
    {
        get { return startPosition; }
    }

    private float distancePercent = 0;
    /// <summary>
    /// How close this object's sorting order is to the camera relative to the camera's parallax range
    /// </summary>
    public float DistancePercent
    {
        get { return distancePercent; }
    }

    // Start is called before the first frame update
    public void Start()
    {
        sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
        startPosition = transform.position;
    }

    public void calculateData(float closeOrder, float farOrder)
    {
        distancePercent = Mathf.Abs(sortingOrder - closeOrder) / Mathf.Abs(farOrder - closeOrder);
    }
}
