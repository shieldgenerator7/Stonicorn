using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeUpdater : MonoBehaviour
{
    [Header("Settings")]
    public float suggestedSpacing = 3;//radial distance between fragments, not guaranteed

    [Header("Prefabs")]
    public GameObject fragmentPrefab;

    private List<GameObject> fragments = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerController pc = GetComponent<PlayerController>();
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
        Debug.Log(circumference + ", " + fragmentCount + ", " + angleSpacing);
        Vector2 placer = Vector2.up * range;
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = Instantiate(fragmentPrefab);
            fragment.transform.parent = transform;
            fragment.transform.localPosition = placer;
            fragment.transform.up = fragment.transform.position - transform.position;
            fragments.Add(fragment);
            //Update placer
            placer = Utility.RotateZ(placer, angleSpacing);
        }
    }

    public void clear()
    {
        foreach(GameObject fragment in fragments)
        {
            Destroy(fragment);
        }
        fragments.Clear();
    }
}
