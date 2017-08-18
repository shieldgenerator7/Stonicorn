using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {

    //Effects
    [Header("Collision Effect")]
    public GameObject collisionEffectPrefab;//the object that holds the special effect for collision
    public float particleStartSpeed = 7.0f;
    public float particleAmount = 50.0f;
    [Header("Tap Target Highlighting")]
    public ParticleSystem tapTargetHighlight;
    [Header("Force Wave Shadows")]
    //Supporting Lists
    private List<ParticleSystem> collisionEffectList = new List<ParticleSystem>();
    private Dictionary<GameObject, GameObject> forceWaveShadows = new Dictionary<GameObject, GameObject>();

    private static EffectManager instance;

    // Use this for initialization
    void Start() {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Shows sparks coming from the point of collision
    /// </summary>
    /// <param name="position">Position of collision</param>
    /// <param name="damagePercent">How much percent of total HP of damage was inflicted, between 0 and 100</param>
    public static void collisionEffect(Vector2 position, float damagePercent = 100.0f)
    {
        ParticleSystem chosenPS = null;
        //Find existing particle system
        foreach (ParticleSystem ps in instance.collisionEffectList)
        {
            if (!ps.isPlaying)
            {
                chosenPS = ps;
                break;
            }
        }
        //Else make a new one
        if (chosenPS == null)
        {
            GameObject ce = GameObject.Instantiate(instance.collisionEffectPrefab);
            ce.transform.parent = instance.transform;
            ParticleSystem ceps = ce.GetComponent<ParticleSystem>();
            instance.collisionEffectList.Add(ceps);
            chosenPS = ceps;
        }
        //Start Speed
        {
            ParticleSystem.MainModule psmm = chosenPS.main;
            ParticleSystem.MinMaxCurve psmmc = psmm.startSpeed;
            float speed = (damagePercent * instance.particleStartSpeed) / 100;
            speed = Mathf.Max(speed, 0.5f);//make speed at least 1.0f
            psmmc.constant = speed;
            psmm.startSpeed = psmmc;
        }
        //Particle Amount
        {
            int amountOfParticles = (int)((damagePercent * instance.particleAmount) / 100);
            amountOfParticles = Mathf.Max(amountOfParticles, 3);//make speed at least 1.0f
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1]
            {
                new ParticleSystem.Burst(0, (short)amountOfParticles)
            };
            chosenPS.emission.SetBursts(bursts);
        }
        //
        chosenPS.gameObject.transform.position = position;
        chosenPS.Play();
    }

    public static void highlightTapArea(Vector2 pos, bool play = true)
    {
        if (play)
        {
            instance.tapTargetHighlight.transform.position = pos;
            instance.tapTargetHighlight.Play();
        }
        else
        {
            instance.tapTargetHighlight.Stop();
        }
    }
    
    /// <summary>
    /// Shows a force wave shadow for the given projectile from the given center
    /// </summary>
    /// <param name="center">The center of the force wave</param>
    /// <param name="range">The range of the force wave</param>
    /// <param name="projectile">The projectile about to be forced away</param>
    public static void showForceWaveShadows(Vector2 center, float range, GameObject projectile)
    {
        GameObject forceWaveShadow;
        if (instance.forceWaveShadows.ContainsKey(projectile))
        {
            forceWaveShadow = instance.forceWaveShadows[projectile];
        }
        else
        {
            forceWaveShadow = new GameObject();
            forceWaveShadow.name = "ForceWaveShadow of " + projectile.name;
            forceWaveShadow.AddComponent<SpriteRenderer>();
            instance.forceWaveShadows.Add(projectile, forceWaveShadow);
            SpriteRenderer sr = forceWaveShadow.GetComponent<SpriteRenderer>();
            SpriteRenderer psr = projectile.GetComponent<SpriteRenderer>();
            sr.sprite = psr.sprite;
            sr.color = new Color(psr.color.r, psr.color.g, psr.color.b, 0.7f);
        }
        Vector2 ppos = (Vector2)projectile.transform.position;
        Vector2 dir = ppos - center;
        float magnitude = Mathf.Max(0, range - dir.magnitude);
        forceWaveShadow.transform.position = ppos + magnitude*dir.normalized;
        forceWaveShadow.transform.rotation = projectile.transform.rotation;
        forceWaveShadow.transform.localScale = projectile.transform.localScale;
    }
    /// <summary>
    /// Deletes all currently existing force wave shadows
    /// </summary>
    public static void clearForceWaveShadows()
    {
        foreach (GameObject shadow in instance.forceWaveShadows.Values)
        {
            Destroy(shadow);
        }
        instance.forceWaveShadows.Clear();
    }
}
