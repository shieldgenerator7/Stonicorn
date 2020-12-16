using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HardMaterial : SavableMonoBehaviour, IBlastable
{

    public static float MINIMUM_CRACKSOUND_THRESHOLD = 1.0f;//the minimum percent of damage done to make a sound

    public string material;//the name of the thing this material is made of
    public float hardness = 1.0f;
    public float forceThreshold = 50.0f;//how much force it can withstand without cracking
    public float maxIntegrity = 100f;
    [Range(0, 100)]
    [SerializeField]
    private float integrity;//how intact it is. Material breaks apart when it reaches 0
    public bool dealsDamage = true;
    public bool disappearsIfNoBrokenPrefab = true;//true = if no broken prefab is supplied, this hard material should just disappear
    private bool alreadyBroken = false;//used to determine if pieces should spawn or not
    [Header("Cracking Settings")]
    public GameObject crackedPrefab;//the prefab for the object broken into pieces
    /// <summary>
    /// true: the cracks are an overlay
    /// false: the cracks replace the original sprite (original sprite must be in list)
    /// </summary>
    public bool crackedOverlay = true;
    public List<SpriteRenderer> crackStages = new List<SpriteRenderer>();
    public List<AudioClip> crackSounds;
    public List<HiddenArea> secretHiders;//the hidden areas to show when cracked

    public event Shattered shattered;
    public event HardCollision hardCollision;

    void Start()
    {
        if (integrity == 0)
        {
            setIntegrity(maxIntegrity);
        }
        else
        {//show cracks from the start
            setIntegrity(integrity);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        ContactPoint2D[] cp2ds = new ContactPoint2D[1];
        coll.GetContacts(cp2ds);
        HardMaterial hm = coll.gameObject.GetComponent<HardMaterial>();
        if (hm != null)
        {
            //Take damage
            float hitHardness = (hm.dealsDamage)
                ? hm.hardness / hardness * coll.relativeVelocity.magnitude
                : 0;
            addIntegrity(-1 * hitHardness);
            //Calculate damage to other
            float hitHardnessOther = (this.dealsDamage)
                ? hardness / hm.hardness * coll.relativeVelocity.magnitude
                : 0;
            //Call delegates
            hardCollision?.Invoke(hitHardness, hitHardnessOther, coll.contacts[0].point);
            //Play Crack Sound
            float hitPercentage = hitHardness * 100 / maxIntegrity;
            Managers.Effect.collisionEffect(cp2ds[0].point, hitPercentage);
            for (int i = crackSounds.Count - 1; i >= 0; i--)
            {
                float crackThreshold = 100 / (crackSounds.Count + 1 - i) - 20;
                if (i == 0)
                {
                    crackThreshold = MINIMUM_CRACKSOUND_THRESHOLD;
                }
                if (hitPercentage > crackThreshold)
                {
                    Managers.Sound.playSound(crackSounds[i], cp2ds[0].point, (hitPercentage / 400) + 0.75f);
                    break;
                }
            }
        }
        else
        {
            GameObject other = coll.gameObject;
            Rigidbody2D rb2d = other.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                float force = rb2d.velocity.magnitude * rb2d.mass;
                float damage = checkForce(force, rb2d.velocity);
                float hitPercentage = damage * 100 / maxIntegrity;
                Managers.Effect.collisionEffect(cp2ds[0].point, hitPercentage);
            }
        }
    }

    /// <summary>
    /// Checks to see if a given force cracks it
    /// </summary>
    /// <returns>The amount of damage done
    /// (positive value means damage dealt, negative means HP healed)</returns>
    public float checkForce(float force, Vector2 direction)
    {
        if (force > forceThreshold)
        {
            float damage = 100 * (force - forceThreshold) / forceThreshold;
            addIntegrity(-1 * damage);
            return damage;
        }
        return 0;
    }
    public float getDistanceFromExplosion(Vector2 explosionPos)
    {
        return explosionPos.distanceToObject(gameObject);
    }

    public bool isIntact()
    {
        return integrity > 0;
    }

    public void addIntegrity(float addend)
    {
        setIntegrity(integrity + addend);
    }

    private void setIntegrity(float newIntegrity)
    {
        float oldIntegrity = integrity;
        integrity = Mathf.Clamp(newIntegrity, 0, maxIntegrity);
        if (integrity > 0)
        {
            //Display cracked sprites
            updateCrackingDisplay(integrity);
            //Forgotten Objects
            if (alreadyBroken || !gameObject.activeInHierarchy || oldIntegrity < 0)
            {
                Managers.Object.saveForgottenObject(gameObject, false);
            }
        }
        else if (oldIntegrity > 0 || gameObject.activeInHierarchy)
        {
            bool shouldRefresh = false;
            if (!alreadyBroken && !Managers.Rewind.Rewinding)
            {
                if (crackedPrefab != null)
                {
                    GameObject pieces = Instantiate(crackedPrefab);
                    pieces.transform.position = transform.position;
                    pieces.transform.rotation = transform.rotation;
                    SceneManager.MoveGameObjectToScene(pieces, gameObject.scene);
                    string tag = "" + System.DateTime.Now.Ticks;
                    pieces.name += tag;
                    CrackedPiece cp = pieces.GetComponent<CrackedPiece>();
                    cp.spawnTag = tag;
                    SpriteRenderer origSR = gameObject.GetComponent<SpriteRenderer>();
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
                        SpriteRenderer sr = t.gameObject.GetComponent<SpriteRenderer>();
                        try
                        {
                            Color color = origSR.color;
                            if (!crackedOverlay)
                            {
                                color.a = 1;
                            }
                            sr.color = color;
                            sr.sortingLayerID = origSR.sortingLayerID;
                            sr.sortingOrder = origSR.sortingOrder;
                        }
                        catch (MissingComponentException mce)
                        {
                            throw new MissingComponentException(
                                "HardMaterial (" + gameObject.name + ") broken prefab piece (" + t.gameObject.name + ") is missing a SpriteRenderer: sr: " + sr,
                                mce);
                        }
                        //Hard Material Copying
                        HardMaterial hm = t.gameObject.GetComponent<HardMaterial>();
                        if (hm)
                        {
                            hm.material = this.material;
                            hm.hardness = this.hardness;
                            hm.forceThreshold = this.forceThreshold;
                            hm.maxIntegrity = this.maxIntegrity;
                            if (hm.integrity == 0)
                            {
                                hm.integrity = this.maxIntegrity / pieces.transform.childCount;
                            }
                        }
                    }
                    shouldRefresh = true;
                }
                else if (!disappearsIfNoBrokenPrefab)
                {
                    Debug.LogError(
                        "/!\\ HardMaterial " + gameObject.name
                        + " has no broken prefab! (Scene: "
                        + gameObject.scene.name + ")"
                        );
                }
                alreadyBroken = true;
            }
            foreach (HiddenArea ha in secretHiders)
            {
                //2017-06-08: copied from CrackedGroundChecker.setCracked()
                if (ha != null && !ReferenceEquals(ha, null))//2016-11-26: reference equal null test copied from an answer by sindrijo: http://answers.unity3d.com/questions/13840/how-to-detect-if-a-gameobject-has-been-destroyed.html
                {
                    ha.Discovered = true;
                }
            }
            if (crackedPrefab != null || disappearsIfNoBrokenPrefab)
            {
                Managers.Object.saveForgottenObject(gameObject);
                shouldRefresh = true;
            }
            if (!Managers.Rewind.Rewinding)
            {
                if (shouldRefresh)
                {
                    Managers.Object.refreshGameObjects();
                }
                if (shattered != null)
                {
                    shattered();//call delegate method
                }
            }
        }
    }

    public float getIntegrity()
    {
        return integrity;
    }

    private void updateCrackingDisplay(float currentIntegrity)
    {
        if (crackedOverlay)
        {
            float baseAlpha = 1.0f - (currentIntegrity / maxIntegrity);
            for (int i = 0; i < crackStages.Count; i++)
            {
                float thresholdLower = i * 1 / (float)crackStages.Count;
                float alpha = 0;
                if (baseAlpha > thresholdLower)
                {
                    alpha = (baseAlpha - thresholdLower) * crackStages.Count;
                }
                SpriteRenderer sr = crackStages[i];
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
        else
        {
            float baseAlpha = 1.0f - (currentIntegrity / maxIntegrity);
            for (int i = 0; i < crackStages.Count; i++)
            {
                float thresholdUpper = (i + 2) * 1 / (float)crackStages.Count;
                float alpha = 0;
                if (thresholdUpper >= baseAlpha)
                {
                    alpha = (thresholdUpper - baseAlpha) * crackStages.Count;
                }
                SpriteRenderer sr = crackStages[i];
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    /// <summary>
    /// Gets called when integrity reaches 0
    /// </summary>
    public delegate void Shattered();
    /// <summary>
    /// Gets called when it collides with another HardMaterial
    /// </summary>
    public delegate void HardCollision(float damageToSelf, float damageToOther, Vector2 contactPoint);


    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "integrity", integrity,
            "alreadyBroken", alreadyBroken
            );
        set
        {
            integrity = value.Float("integrity");
            alreadyBroken = value.Bool("alreadyBroken");
            setIntegrity(integrity);
        }
    }
}
