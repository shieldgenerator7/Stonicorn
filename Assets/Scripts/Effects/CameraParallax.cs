using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParallax : MonoBehaviour
{
    public bool startIsAlwaysAtOrigin = true;

    public List<string> parallaxLayers = new List<string>();
    public float closeOrder = 0;
    public float farOrder = -100;

    private List<CameraParallaxData> cpds;

    private Vector3 startPosition;
    private Vector3 prevPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (startIsAlwaysAtOrigin)
        {
            startPosition = Vector3.zero;
            startPosition.z = transform.position.z;
        }
        else
        {
            startPosition = transform.position;
        }
        updateDataList();
    }

    // Update is called once per frame
    void Update()
    {
        if (prevPosition != transform.position)
        {
            prevPosition = transform.position;
            //Update all background objects
            Vector3 delta = transform.position - startPosition;
            foreach (CameraParallaxData cpd in cpds)
            {
                cpd.transform.position = (delta * cpd.DistancePercent) + cpd.StartPosition;
            }
        }
    }

    void updateDataList()
    {
        cpds = new List<CameraParallaxData>();
        foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>())
        {
            //Check to make sure the SpriteRenderer is on a parallaxing layer
            foreach (string layer in parallaxLayers)
            {
                //If the layer name matches a parallaxing layer,
                if (sr.sortingLayerName == layer)
                {
                    //And if the sorting order is within the range,
                    if (Utility.between(sr.sortingOrder, closeOrder, farOrder))
                    {
                        //Make sure each SpriteRenderer has a data object
                        CameraParallaxData cpd = sr.GetComponent<CameraParallaxData>();
                        if (cpd == null)
                        {
                            cpd = sr.gameObject.AddComponent<CameraParallaxData>();
                            cpd.Start();
                        }
                        //If distance percent is 0,
                        if (cpd.DistancePercent == 0)
                        {
                            //calculate the distance percent
                            cpd.calculateData(closeOrder, farOrder);
                        }
                        //If distance percent is still 0,
                        if (cpd.DistancePercent == 0)
                        {
                            //object won't move, so don't process it
                            break;
                        }
                        //Add the data object to the list
                        cpds.Add(cpd);
                        break;
                    }
                }
            }
        }
        //Error checking
        if (cpds.Count == 0)
        {
            throw new UnityException("CameraParallax found no SpriteRenderers with a matching layer name!");
        }
    }
}
