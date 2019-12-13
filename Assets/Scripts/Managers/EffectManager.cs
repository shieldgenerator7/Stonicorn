using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{

    //Effects
    [Header("Teleport Star Effect")]
    public GameObject teleportStarPrefab;//the object that holds the special effect for collision
    public float teleportStarDuration = 2.0f;//how long the teleport star will stay on screen (in sec)
    public Color teleportStarColor = new Color(1, 1, 1);
    [Header("Collision Effect")]
    public GameObject collisionEffectPrefab;//the object that holds the special effect for collision
    public float particleStartSpeed = 7.0f;
    public float particleAmount = 50.0f;
    [Header("Tap Target Highlighting")]
    public ParticleSystem tapTargetHighlight;
    [Header("Force Wave Shadows")]
    //Supporting Lists
    private List<TeleportStarUpdater> teleportStarList = new List<TeleportStarUpdater>();
    private List<ParticleSystem> collisionEffectList = new List<ParticleSystem>();
    private Dictionary<GameObject, GameObject> forceWaveShadows = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        for (int i = 0; i < teleportStarList.Count; i++)
        {
            if (teleportStarList[i].TurnedOn)
            {
                teleportStarList[i].updateStar();
            }
        }
    }
    /// <summary>
    /// Shows the teleport star effect
    /// 2017-10-31: copied from PlayerController.showTeleportStar()
    /// </summary>
    /// <param name="pos"></param>
    public void showTeleportStar(Vector3 pos)
    {
        TeleportStarUpdater chosenTSU = null;
        //Find existing particle system
        foreach (TeleportStarUpdater tsu in teleportStarList)
        {
            if (tsu != null && !tsu.TurnedOn)
            {
                chosenTSU = tsu;
                break;
            }
        }
        //Else make a new one
        if (chosenTSU == null)
        {
            GameObject newTS = GameObject.Instantiate(teleportStarPrefab);
            newTS.transform.parent = transform;
            TeleportStarUpdater newTSU = newTS.GetComponent<TeleportStarUpdater>();
            newTSU.init();
            newTSU.duration = teleportStarDuration;
            newTSU.baseColor = teleportStarColor;
            teleportStarList.Add(newTSU);
            chosenTSU = newTSU;
        }
        //Set values
        chosenTSU.position(pos);
        chosenTSU.TurnedOn = true;
    }

    /// <summary>
    /// Shows sparks coming from the point of collision
    /// </summary>
    /// <param name="position">Position of collision</param>
    /// <param name="damagePercent">How much percent of total HP of damage was inflicted, between 0 and 100</param>
    public void collisionEffect(Vector2 position, float damagePercent = 100.0f)
    {
        if (!Managers.Camera.inView(position))
        {
            return;//don't display effect if it's not going to show
        }
        ParticleSystem chosenPS = null;
        //Find existing particle system
        foreach (ParticleSystem ps in collisionEffectList)
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
            GameObject ce = GameObject.Instantiate(collisionEffectPrefab);
            ce.transform.parent = transform;
            ParticleSystem ceps = ce.GetComponent<ParticleSystem>();
            collisionEffectList.Add(ceps);
            chosenPS = ceps;
        }
        //Start Speed
        {
            ParticleSystem.MainModule psmm = chosenPS.main;
            ParticleSystem.MinMaxCurve psmmc = psmm.startSpeed;
            float speed = (damagePercent * particleStartSpeed) / 100;
            speed = Mathf.Max(speed, 0.5f);//make speed at least 1.0f
            psmmc.constant = speed;
            psmm.startSpeed = psmmc;
        }
        //Particle Amount
        {
            int amountOfParticles = (int)((damagePercent * particleAmount) / 100);
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

    public void highlightTapArea(Vector2 pos, bool play = true)
    {
        if (play)
        {
            tapTargetHighlight.transform.position = pos;
            tapTargetHighlight.Play();
        }
        else
        {
            tapTargetHighlight.Stop();
        }
    }

    public void showPointEffect(string name, Vector2 pos, bool play = true)
    {
        GameObject effect = null;
        foreach (Transform t in transform)
        {
            if (t.gameObject.name == name)
            {
                effect = t.gameObject;
                break;
            }
        }
        if (effect == null)
        {
            throw new UnityException("Effect \"" + name + "\" not found!");
        }
        effect.SetActive(play);
        effect.transform.position = pos;
    }

    /// <summary>
    /// Shows a force wave shadow for the given projectile from the given center
    /// </summary>
    /// <param name="center">The center of the force wave</param>
    /// <param name="range">The range of the force wave</param>
    /// <param name="projectile">The projectile about to be forced away</param>
    public void showForceWaveShadows(Vector2 center, float range, GameObject projectile)
    {
        GameObject forceWaveShadow;
        if (forceWaveShadows.ContainsKey(projectile))
        {
            forceWaveShadow = forceWaveShadows[projectile];
        }
        else
        {
            forceWaveShadow = new GameObject();
            forceWaveShadow.name = "ForceWaveShadow of " + projectile.name;
            forceWaveShadow.AddComponent<SpriteRenderer>();
            forceWaveShadows.Add(projectile, forceWaveShadow);
            SpriteRenderer sr = forceWaveShadow.GetComponent<SpriteRenderer>();
            SpriteRenderer psr = projectile.GetComponent<SpriteRenderer>();
            sr.sprite = psr.sprite;
            sr.color = new Color(psr.color.r, psr.color.g, psr.color.b, 0.7f);
        }
        Vector2 ppos = (Vector2)projectile.transform.position;
        Vector2 dir = ppos - center;
        float magnitude = Mathf.Max(0, range - dir.magnitude);
        forceWaveShadow.transform.position = ppos + magnitude * dir.normalized;
        forceWaveShadow.transform.rotation = projectile.transform.rotation;
        forceWaveShadow.transform.localScale = projectile.transform.localScale;
    }
    /// <summary>
    /// Deletes all currently existing force wave shadows
    /// </summary>
    public void clearForceWaveShadows()
    {
        foreach (GameObject shadow in forceWaveShadows.Values)
        {
            Destroy(shadow);
        }
        forceWaveShadows.Clear();
    }
}
