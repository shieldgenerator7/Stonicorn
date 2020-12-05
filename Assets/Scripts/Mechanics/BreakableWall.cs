using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : SavableMonoBehaviour, Blastable
{
    public float minForceThreshold = 2.5f;//the minimum amount of force required to crack it
    public float maxForceThreshold = 50;//the maximum amount of force consumed per damage
    [Range(1, 5)]
    public int maxIntegrity = 3;
    [SerializeField]
    [Range(1, 5)]
    private int integrity = 0;
    public int Integrity
    {
        get => integrity;
        set
        {
            integrity = Mathf.Clamp(value, 0, maxIntegrity);
            if (integrity > 0)
            {
                if (sr == null)
                {
                    sr = GetComponent<SpriteRenderer>();
                }
                sr.sprite = crackStages[maxIntegrity - integrity];
                if (!gameObject.activeSelf)
                {
                    Managers.Object.saveForgottenObject(gameObject, false);
                }
            }
            else
            {
                if (gameObject.activeSelf)
                {
                    Managers.Object.saveForgottenObject(gameObject, true);
                    //
                    //Break into pieces
                    //
                    if (crackedPieces == null || ReferenceEquals(crackedPieces, null))
                    {
                        GameObject pieces = Instantiate(crackedPrefab);
                        this.crackedPieces = pieces;
                        pieces.transform.position = transform.position;
                        pieces.transform.rotation = transform.rotation;
                        SceneLoader.moveToScene(pieces, gameObject.scene);
                        string tag = "" + System.DateTime.Now.Ticks;
                        pieces.name = gameObject.name + "---" + tag;
                        CrackedPiece cp = pieces.GetComponent<CrackedPiece>();
                        cp.spawnTag = tag;
                        foreach (Transform t in pieces.transform)
                        {
                            if (t.gameObject.GetComponent<CrackedPiece>() == null)
                            {
                                CrackedPiece tcp = t.gameObject.AddComponent<CrackedPiece>();
                                tcp.prefabName = cp.prefabName;
                                tcp.spawnTag = cp.spawnTag;
                            }
                            t.gameObject.name += tag;
                            t.localScale = transform.localScale;
                            t.localPosition = new Vector2(t.localPosition.x * t.localScale.x, t.localPosition.y * t.localScale.y);
                            //Sprite Renderer Copying
                            SpriteRenderer tsr = t.gameObject.GetComponent<SpriteRenderer>();
                            try
                            {
                                tsr.color = sr.color;
                                tsr.sortingLayerID = sr.sortingLayerID;
                                tsr.sortingOrder = sr.sortingOrder;
                            }
                            catch (MissingComponentException mce)
                            {
                                throw new MissingComponentException(
                                    "HardMaterial (" + gameObject.name + ") broken prefab piece (" + t.gameObject.name + ") is missing a SpriteRenderer: sr: " + tsr,
                                    mce);
                            }
                            //Breakable Wall Copying
                            BreakableWall bw = t.gameObject.GetComponent<BreakableWall>();
                            if (bw)
                            {
                                bw.minForceThreshold = this.minForceThreshold;
                                bw.maxForceThreshold = this.maxForceThreshold;
                                bw.maxIntegrity = this.maxIntegrity;
                                if (bw.integrity == 0)
                                {
                                    bw.integrity = this.maxIntegrity / pieces.transform.childCount;
                                }
                            }
                            //Register new object
                            Managers.Object.addObject(t.gameObject, true);
                        }
                        Managers.Object.addObject(pieces);
                    }
                    //
                    //Reveal hidden areas
                    //
                    foreach (HiddenArea ha in secretHiders)
                    {
                        if (ha != null && !ReferenceEquals(ha, null))
                        {
                            ha.Discovered = true;
                        }
                    }
                }
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

    private GameObject crackedPieces = null;//Not null while broken

    //Components
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
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
            float force = rb2d.velocity.magnitude * rb2d.mass;
            float damage = checkForce(force, rb2d.velocity);
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
            if (integrity == 0)
            {
                foreach (Rigidbody2D rb2d in crackedPieces.GetComponentsInChildren<Rigidbody2D>())
                {
                    rb2d.AddForce(direction.normalized * force);
                }
            }
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
            //If it doesnt have a reference to its cracked pieces,
            if (crackedPieces == null || ReferenceEquals(crackedPieces, null))
            {
                //Find the object
                List<GameObject> gos = Managers.Object.getObjectsWithName(gameObject.name + "---");
                if (gos.Count > 0)
                {
                    crackedPieces = gos[0];
                }
                else
                {
                    crackedPieces = null;
                }
            }
        }
    }
}
