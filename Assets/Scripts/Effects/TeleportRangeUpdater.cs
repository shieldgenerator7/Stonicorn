using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeUpdater : MonoBehaviour
{
    [Header("Settings")]
    public float suggestedSpacing = 3;//radial distance between fragments, not guaranteed
    [Range(0, 1)]
    public float transparency = 1.0f;
    public float normalLength = 0.5f;
    public float timeLength = 0.8f;

    [Header("Prefabs")]
    public GameObject fragmentPrefab;

    private List<GameObject> fragments = new List<GameObject>();

    private Timer timer;

    // Start is called before the first frame update
    void Start()
    {
        //Timer
        timer = GetComponent<Timer>();
        timer.onTimeLeftChanged += updateTimer;
        //Register range update delegate
        PlayerController pc = GetComponent<PlayerController>();
        if (!pc)
        {
            pc = GetComponentInParent<PlayerController>();
        }
        pc.onRangeChanged += updateRange;
        pc.onAbilityActivated += abilityActivated;
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
        //Update colors
        updateColors();
        //Update timers
        updateTimer(timer.TimeLeft, timer.Duration);
    }

    public void updateColors()
    {
        //Set the color to white
        foreach (GameObject fragment in fragments)
        {
            fragment.GetComponent<SpriteRenderer>().color = Color.white;
        }
        //Segment consulting
        foreach (PlayerAbility ability in GetComponents<PlayerAbility>())
        {
            if (ability.enabled)
            {
                ability.teleportRangeSegment?.processFragments(fragments, transform.up);
            }
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

    public void updateTimer(float timeLeft, float duration)
    {
        Vector2 upVector = transform.up;
        float angleMin = 0;
        float angleMax = 360 * timeLeft / duration;
        foreach (GameObject fragment in fragments)
        {
            //Set the length to standard
            Vector3 scale = fragment.transform.localScale;
            scale.y = normalLength;
            if (timeLeft > 0)
            {
                //Check to see if it's in the timer range
                if (Utility.between(
                    Utility.RotationZ(upVector, fragment.transform.up),
                    angleMin,
                    angleMax
                    )
                    )
                {
                    //If so, set the length to the time length
                    scale.y = timeLength;
                }
            }
            //Put the size back in the fragment
            fragment.transform.localScale = scale;
        }
    }

    public void abilityActivated(PlayerAbility ability, bool active)
    {
        updateColors();
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
