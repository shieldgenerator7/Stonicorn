using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : SavableMonoBehaviour, IBlastable
{
    public float minForceThreshold = 2.5f;//the minimum amount of force required to crack it
    public float maxForceThreshold = 50;//the maximum amount of force consumed per damage
    [Range(1, 10)]
    public int maxIntegrity = 3;
    [SerializeField]
    [Range(0, 10)]
    private int integrity = 0;
    public int Integrity
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
                    int index = Mathf.Clamp(
                        maxIntegrity - integrity,
                        0,
                        crackStages.Count - 1
                        );
                    sr.sprite = crackStages[index];
                }
            }
            else
            {
                //Break into pieces
                if (crackedPrefab)
                {
                    GameObject pieces = Utility.Instantiate(crackedPrefab);
                    BrokenPiece brokenPiece = pieces.GetComponent<BrokenPiece>();
                    brokenPiece.unpack(gameObject);
                    //change color of broken pieces
                    Color color = GetComponent<SpriteRenderer>().color;
                    brokenPiece.Savables.ForEach(
                        bp => bp.GetComponent<SpriteRenderer>().color = color
                        );
                }

                //Reveal hidden areas
                secretHiders
                    .FindAll(ha => ha != null && !ReferenceEquals(ha, null))
                    .ForEach(ha => ha.Discovered = true);

                //Destroy object
                Managers.Object.destroyObject(gameObject);
            }
        }
    }

    [Header("Cracked Components")]
    public GameObject crackedPrefab;
    public List<Sprite> crackStages;
    public AudioClip soundDamageNone;
    public AudioClip soundDamageOne;
    public AudioClip soundDamageTwoOrMore;
    public List<HiddenArea> secretHiders;

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

    public float checkForce(float force, Vector2 direction)
    {
        if (force >= minForceThreshold)
        {
            int damage = Mathf.Max(1, Mathf.FloorToInt(force / maxForceThreshold));
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
        get => new SavableObject(this, "integrity", integrity);
        set
        {
            Integrity = value.Int("integrity");
        }
    }
}
