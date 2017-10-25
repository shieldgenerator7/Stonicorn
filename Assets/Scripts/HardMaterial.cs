using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HardMaterial : SavableMonoBehaviour
{

    public static float MINIMUM_CRACKSOUND_THRESHOLD = 1.0f;//the minimum percent of damage done to make a sound

    public float hardness = 1.0f;
    public float forceThreshold = 50.0f;//how much force it can withstand without cracking
    public float maxIntegrity = 100f;
    public bool disappearsIfNoBrokenPrefab = true;//true = if no broken prefab is supplied, this hard material should just disappear
    [Range(0, 100)]
    [SerializeField]
    private float integrity;//how intact it is. Material breaks apart when it reaches 0
    private bool alreadyBroken = false;//used to determine if pieces should spawn or not
    public GameObject crackedPrefab;//the prefab for the object broken into pieces
    public List<GameObject> crackStages;
    private List<SpriteRenderer> crackSprites = new List<SpriteRenderer>();
    public List<AudioClip> crackSounds;
    public List<HiddenArea> secretHiders;//the hidden areas to show when cracked

    public Shattered shattered;

    void Start()
    {
        foreach (GameObject crackStage in crackStages)
        {
            crackSprites.Add(crackStage.GetComponent<SpriteRenderer>());
        }
        if (integrity == 0)
        {
            setIntegrity(maxIntegrity);
        }
        else
        {//show cracks from the start
            setIntegrity(integrity);
        }
    }

    //void Update()
    //{
    //    setIntegrity(integrity);
    //}

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (GameManager.isRewinding())
        {
            return;//don't process collisions while rewinding
        }
        HardMaterial hm = coll.gameObject.GetComponent<HardMaterial>();
        if (hm != null)
        {
            float hitHardness = hm.hardness / hardness * coll.relativeVelocity.magnitude;
            addIntegrity(-1 * hitHardness);
            float hitPercentage = hitHardness * 100 / maxIntegrity;
            EffectManager.collisionEffect(coll.contacts[0].point, hitPercentage);
            for (int i = crackSounds.Count - 1; i >= 0; i--)
            {
                float crackThreshold = 100 / (crackSprites.Count + 1 - i) - 20;
                if (i == 0)
                {
                    crackThreshold = MINIMUM_CRACKSOUND_THRESHOLD;
                }
                if (hitPercentage > crackThreshold)
                {
                    AudioSource.PlayClipAtPoint(crackSounds[i], coll.contacts[0].point, hitPercentage / 400 + 0.75f);
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
                float damage = checkForce(force);
                float hitPercentage = damage * 100 / maxIntegrity;
                EffectManager.collisionEffect(coll.contacts[0].point, hitPercentage);
            }
        }
    }

    /// <summary>
    /// Checks to see if a given force cracks it
    /// </summary>
    /// <returns>The amount of damage done
    /// (positive value means damage dealt, negative means HP healed)</returns>
    public float checkForce(float force)
    {
        if (force > forceThreshold)
        {
            float damage = 100 * (force - forceThreshold) / forceThreshold;
            addIntegrity(-1 * damage);
            return damage;
        }
        return 0;
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
            float baseAlpha = 1.0f - (integrity / maxIntegrity);
            for (int i = 0; i < crackSprites.Count; i++)
            {
                float thresholdLower = i * 1 / (float)crackSprites.Count;
                float alpha = 0;
                if (baseAlpha > thresholdLower)
                {
                    alpha = (baseAlpha - thresholdLower) * crackSprites.Count;
                }
                SpriteRenderer sr = crackSprites[i];
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, alpha);
            }
            if (alreadyBroken || !gameObject.activeInHierarchy || oldIntegrity < 0)
            {
                GameManager.saveForgottenObject(gameObject, false);
            }
        }
        else if (oldIntegrity > 0 || gameObject.activeInHierarchy)
        {
            bool shouldRefresh = false;
            if (!alreadyBroken && !GameManager.isRewinding())
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
                    }
                    shouldRefresh = true;
                }
                else if (!disappearsIfNoBrokenPrefab)
                {
                    Debug.Log("/!\\ HardMaterial " + gameObject.name + " has no broken prefab! (Scene: " + gameObject.scene.name + ")");
                }
                alreadyBroken = true;
            }
            foreach (HiddenArea ha in secretHiders)
            {
                //2017-06-08: copied from CrackedGroundChecker.setCracked()
                if (ha != null && !ReferenceEquals(ha, null))//2016-11-26: reference equal null test copied from an answer by sindrijo: http://answers.unity3d.com/questions/13840/how-to-detect-if-a-gameobject-has-been-destroyed.html
                {
                    ha.nowDiscovered();
                }
            }
            if (crackedPrefab != null || disappearsIfNoBrokenPrefab)
            {
                GameManager.saveForgottenObject(gameObject);
                shouldRefresh = true;
            }
            if (!GameManager.isRewinding())
            {
                if (shouldRefresh)
                {
                    GameManager.refresh();
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

    /// <summary>
    /// Gets called when integrity reaches 0
    /// </summary>
    public delegate void Shattered();

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this, "integrity", integrity, "alreadyBroken", alreadyBroken);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        integrity = (float)savObj.data["integrity"];
        alreadyBroken = (bool)savObj.data["alreadyBroken"];
        setIntegrity(integrity);
    }
}
