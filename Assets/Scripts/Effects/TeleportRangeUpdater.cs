using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeUpdater : MonoBehaviour
{
    [Header("Settings")]
    public float suggestedSpacing = 3;//radial distance between fragments, not guaranteed
    [Range(0, 1)]
    public float transparency = 1.0f;
    [Range(0, 1)]
    public float timeTransparency = 0.5f;
    public float normalLength = 0.5f;
    public float timeLength = 0.8f;
    public float timeLeftShake = 10;//when this much time is left, it starts shaking
    public float maxShakeDistance = 0.2f;//how far it moves left and right when shaking
    public float maxShakeAlpha = 0.3f;//the max diff between the timeTransparency and the random alpha

    [Header("Components")]
    public GameObject fragmentPrefab;
    public GameObject parentObj;
    public Timer timer;

    private List<GameObject> fragments = new List<GameObject>();
    private float range;//cached range, gets updated in updateRange()

    public float bounceBackSpeed = 2;
    private Vector2 prevVelocity;
    private Rigidbody2D rb2dParent;
    private Vector2 offset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        //Timer
        timer.onTimeLeftChanged += updateTimer;
        timer.onTimeLeftChanged += updateTimerAlpha;
        //Register range update delegate
        PlayerController pc = parentObj.GetComponent<PlayerController>();
        pc.onRangeChanged += updateRange;
        pc.onAbilityActivated += abilityActivated;
        updateRange(pc.Range);
        //rb2dParent
        rb2dParent = parentObj.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Check if needs to shake
        if (rb2dParent)
        {
            if (rb2dParent.velocity != prevVelocity
                && rb2dParent.velocity.magnitude < 0.1f)
            {
                offset += (prevVelocity - rb2dParent.velocity) * 0.1f;
            }
            prevVelocity = rb2dParent.velocity;
        }
        //Update position and rotation
        transform.position = parentObj.transform.position + (Vector3)offset;
        transform.up = Managers.Camera.transform.up;
        //Decrease offset
        offset = Vector2.Lerp(offset, Vector2.zero, bounceBackSpeed * Time.deltaTime);
    }

    public void updateRange(float range)
    {
        clear();
        this.range = range;
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
        updateTimerAlpha(timer.TimeLeft, timer.Duration);
    }

    public void updateColors()
    {
        //Set the color to white
        foreach (GameObject fragment in fragments)
        {
            fragment.GetComponent<SpriteRenderer>().color = Color.white;
        }
        //Segment consulting
        foreach (PlayerAbility ability in parentObj.GetComponents<PlayerAbility>())
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
        float maxShake = maxShakeDistance * (timeLeftShake - timeLeft) / timeLeftShake;
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
                else
                {
                    //Check to see if it should shake
                    if (timeLeft <= timeLeftShake)
                    {
                        //Shake each fragment individually
                        float randomRange = Random.Range(-maxShake, maxShake);
                        fragment.transform.localPosition = fragment.transform.localPosition.normalized * (range + randomRange);
                    }
                }
            }
            //Put the size back in the fragment
            fragment.transform.localScale = scale;
        }
    }
    public void updateTimerAlpha(float timeLeft, float duration)
    {
        Vector2 upVector = transform.up;
        float angleMin = 0;
        float angleMax = 360 * timeLeft / duration;
        float maxShake = maxShakeAlpha * (timeLeftShake - timeLeft) / timeLeftShake;
        foreach (GameObject fragment in fragments)
        {
            //Set the alpha to standard
            SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
            Color color = sr.color;
            color.a = transparency;
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
                    color.a = timeTransparency;
                }
                else
                {
                    //Check to see if it should shake
                    if (timeLeft <= timeLeftShake)
                    {
                        //Shake each fragment alpha individually
                        float randomRange = Random.Range(-maxShake, maxShake);
                        color.a = transparency + randomRange;
                    }
                }
            }
            //Put the color back in the fragment
            sr.color = color;
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
