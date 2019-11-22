using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeUpdater : MonoBehaviour
{
    [Header("Settings")]
    public float suggestedSpacing = 3;//radial distance between fragments, not guaranteed
    [Range(0, 1)]
    public float transparency = 1.0f;

    [Header("Prefabs")]
    public GameObject fragmentPrefab;

    private List<GameObject> fragments = new List<GameObject>();
    private List<TeleportRangeSegment> segments;

    // Start is called before the first frame update
    void Start()
    {
        //Segments
        segments = new List<TeleportRangeSegment>(
            GetComponents<TeleportRangeSegment>()
            );
        //Register range update delegate
        PlayerController pc = GetComponent<PlayerController>();
        if (!pc)
        {
            pc = GetComponentInParent<PlayerController>();
        }
        pc.onRangeChanged += updateRange;
        updateRange(pc.Range);
    }

    public void updateRange(float range)
    {
        clear();
        float circumference = 2 * range * Mathf.PI;
        float spacing = suggestedSpacing;// * fragmentPrefab.GetComponent<SpriteRenderer>().size.x;
        int fragmentCount = Mathf.RoundToInt(circumference / spacing);
        float angleSpacing = 2 * Mathf.PI / (float)fragmentCount;
        Vector2 placer = Vector2.up * range;
        for (int i = 0; i < fragmentCount; i++)
        {
            //Instantiate
            GameObject fragment = Instantiate(fragmentPrefab);
            //Parent it to this object
            fragment.transform.parent = transform;
            //Place the fragment in the right position and rotation
            fragment.transform.localPosition = placer;
            fragment.transform.up = fragment.transform.position - transform.position;
            //Add it to the list
            fragments.Add(fragment);
            //Update placer
            placer = Utility.RotateZ(placer, angleSpacing);
        }
        //Segment consulting
        foreach (TeleportRangeSegment segment in segments)
        {
            segment.processFragments(fragments, transform.up);
        }
        //Set the transparency
        foreach (GameObject fragment in fragments)
        {
            //Set the fragment transparency
            SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
            Color color = sr.color;
            color.a = transparency;
            sr.color = color;
        }
    }

    public void clear()
    {
        foreach (GameObject fragment in fragments)
        {
            Destroy(fragment);
        }
        fragments.Clear();
    }
}
