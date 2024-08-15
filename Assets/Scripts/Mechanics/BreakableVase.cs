using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreakableVase : SavableMonoBehaviour, IBlastable
{
    public float destroyDelay = 1;
    public float minForceThreshold = 2.5f;//the minimum amount of force required to crack it
    public float maxForceThreshold = 50;//the maximum amount of force consumed per damage
    [Range(1, 100)]
    public float maxIntegrity = 3;
    [SerializeField]
    [Range(0, 100)]
    private float integrity = 0;
    public float Integrity
    {
        get => integrity;
        set
        {
            integrity = Mathf.Clamp(value, 0, maxIntegrity);

            if (integrity > 0)
            {
                if (crackStages.Count > 0)
                {
                    if (sr == null)
                    {
                        sr = GetComponent<SpriteRenderer>();
                    }
                    int index = (int)Mathf.Clamp(
                        maxIntegrity - integrity,
                        0,
                        crackStages.Count - 1
                        );
                    sr.sprite = crackStages[index];
                }
            }
            else
            {
                //Destroy object
                if (destroyDelay == 0)
                {
                    breakApart();
                }
                else
                {
                    Timer.startTimer(destroyDelay, breakApart);
                }
            }
        }
    }

    [Header("Cracked Components")]
    public GameObject crackedPrefab;
    public List<GameObject> contents;
    private List<Sprite> crackStages = new List<Sprite>();
    public AudioClip soundDamageNone;
    public AudioClip soundDamageOne;
    public AudioClip soundDamageTwoOrMore;
    public List<MemoryMonoBehaviour> dicoverables;
    private int contentId;

    //Components
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        init();
    }
    public override void init()
    {
        //Components
        sr = GetComponent<SpriteRenderer>();
        //Initialize integrity
        if (integrity == 0)
        {
            Integrity = maxIntegrity;
        }
        else
        {
            Integrity = Integrity;
        }

        //Find contents, if exists
        if (contentId == 0)
        {
            contentId = contents[0]?.GetComponent<SavableObjectInfo>().Id ?? 0;
        }
        SavableObjectInfo soi = FindObjectsByType<SavableObjectInfo>(FindObjectsSortMode.None)
            .Where(soi => soi.Id == contentId)
            .FirstOrDefault();
        if (soi)
        {
            contents.FindAll(go => go != soi.gameObject).ForEach(go => Destroy(go));
            soi.transform.parent = transform;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        ContactPoint2D[] cp2ds = new ContactPoint2D[1];
        coll.GetContacts(cp2ds);
        Rigidbody2D rb2d = coll.gameObject.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            Vector2 relativeVelocity = coll.relativeVelocity;
            float force = relativeVelocity.magnitude * rb2d.mass;
            float damage = checkForce(force, relativeVelocity);
            //Show Collision Effect
            float hitPercentage = damage * 100 / maxIntegrity;
            Managers.Effect.collisionEffect(cp2ds[0].point, hitPercentage);
            //Play Collision Sound
            if (damage == 0)
            {
                Managers.Sound.playSound(soundDamageNone, cp2ds[0].point, 0.5f);
            }
            else if (damage == 1)
            {
                Managers.Sound.playSound(soundDamageOne, cp2ds[0].point);
            }
            else if (damage >= 2)
            {
                Managers.Sound.playSound(soundDamageTwoOrMore, cp2ds[0].point);
            }
        }
    }

    public void breakApart()
    {
        //Break into pieces
        if (crackedPrefab)
        {
            GameObject pieces = Instantiate(crackedPrefab);
            BrokenPiece brokenPiece = pieces.GetComponent<BrokenPiece>();
            if (brokenPiece)
            {
                brokenPiece.unpack(gameObject);
                //change color of broken pieces
                Color color = GetComponent<SpriteRenderer>().color;
                brokenPiece.Savables.ForEach(
                    bp => bp.GetComponent<SpriteRenderer>().color = color
                    );
            }
            //set id
            MemoryObjectInfo moi = pieces.GetComponent<MemoryObjectInfo>();
            moi.Id = (GetComponent<MemoryObjectInfo>() ?? GetComponentInChildren<MemoryObjectInfo>()).Id;
        }

        //Reveal discoverables
        dicoverables
            .FindAll(ha => ha != null && !ReferenceEquals(ha, null))
            .ForEach(ha => ha.Discovered = true);

        //Deploy contents
        contents.ForEach(go => go.transform.parent = null);

        //Destroy object
        Managers.Object.destroyObject(gameObject);
    }


    public float checkForce(float force, Vector2 direction)
    {
        if (force >= minForceThreshold)
        {
            float thresholdRange = maxForceThreshold - minForceThreshold;
            float damage = maxIntegrity * ((force - minForceThreshold) / thresholdRange);
            Integrity -= damage;
            return damage;
        }
        return 0;
    }

    public float getDistanceFromExplosion(Vector2 explosionPos)
    {
        return explosionPos.distanceToObject(gameObject);
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this, "integrity", integrity, "contentid", contentId);//TODO: make it work for multiple contents
        set
        {
            Integrity = value.Int("integrity");
            contentId = value.Int("contentid");
        }
    }
}
