using System.Collections.Generic;
using UnityEngine;

public class CactusBlossomController : MonoBehaviour
{
    [Tooltip("How far to the left to open to. 0 is pointing to its left")]
    public float openLeft = 0;
    [Tooltip("How far to the right to open to. 0 is pointing to its left")]
    public float openRight = 180;

    public List<Transform> petals;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void placePetals(float openPercent)
    {

    }
}
