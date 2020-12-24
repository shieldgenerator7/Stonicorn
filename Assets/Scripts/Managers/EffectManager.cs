using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{

    //Effects
    [Header("Teleport Star Effect")]
    public GameObject teleportStarPrefab;
    [Header("Teleport Streak Effect")]
    public GameObject teleportStreakPrefab;
    [Header("Lightning Static Effect")]
    public GameObject lightningStaticPrefab;
    [Header("Swap Effects")]
    public GameObject swapCirclePrefab;
    public GameObject swapRotatePrefab;
    public GameObject swapStasisPrefab;
    [Header("Collision Effect")]
    public GameObject collisionEffectPrefab;//the object that holds the special effect for collision
    public float particleStartSpeed = 7.0f;
    public float particleAmount = 50.0f;
    [Header("Tap Target Highlighting")]
    public ParticleSystem tapTargetHighlight;
    [Header("Rewind Stripe Effect")]
    public GameObject rewindCanvas;
    //Supporting Lists
    private List<Fader> teleportStarList = new List<Fader>();
    private List<Fader> teleportStreakList = new List<Fader>();
    private List<SpriteRenderer> lightningStaticList = new List<SpriteRenderer>();
    private List<SpriteRenderer> swapCircleList = new List<SpriteRenderer>();
    private List<SpriteRenderer> swapRotateList = new List<SpriteRenderer>();
    private List<SpriteRenderer> swapStasisList = new List<SpriteRenderer>();
    private List<ParticleSystem> collisionEffectList = new List<ParticleSystem>();

    /// <summary>
    /// Shows the teleport star effect
    /// 2017-10-31: copied from PlayerController.showTeleportStar()
    /// </summary>
    /// <param name="pos"></param>
    public void showTeleportStar(Vector3 pos)
    {
        //Find existing particle system
        Fader chosenTSU = teleportStarList.FirstOrDefault(
            tsu => !tsu.enabled
            );
        //Else make a new one
        if (chosenTSU == null)
        {
            GameObject newTS = GameObject.Instantiate(teleportStarPrefab);
            newTS.transform.parent = transform;
            Fader newTSU = newTS.GetComponent<Fader>();
            teleportStarList.Add(newTSU);
            chosenTSU = newTSU;
        }
        chosenTSU.transform.position = pos;
        chosenTSU.enabled = true;
    }

    /// <summary>
    /// Shows the teleport streak effect
    /// 2020-12-16: copied from showTeleportStar()
    /// </summary>
    /// <param name="pos"></param>
    public void showTeleportStreak(Vector2 startPos, Vector2 endPos)
    {
        //Find existing particle system
        Fader chosenTSU = teleportStreakList.FirstOrDefault(
            tsu => !tsu.enabled
            );
        //Else make a new one
        if (chosenTSU == null)
        {
            GameObject newTS = GameObject.Instantiate(teleportStreakPrefab);
            newTS.transform.parent = transform;
            Fader newTSU = newTS.GetComponent<Fader>();
            teleportStreakList.Add(newTSU);
            chosenTSU = newTSU;
        }
        //Set position
        chosenTSU.transform.position = startPos;
        //Set angle
        Vector2 dir = endPos - startPos;
        chosenTSU.transform.up = dir;
        //Set size
        chosenTSU.transform.localScale = Vector2.one * dir.magnitude;
        //Enable
        chosenTSU.enabled = true;
    }

    /// <summary>
    /// Returns true if there's already an effect from this list on this game object
    /// </summary>
    /// <param name="srs"></param>
    /// <param name="go"></param>
    /// <returns></returns>
    bool isEffectOnObject(List<SpriteRenderer> srs, GameObject go)
        => go.GetComponentsInChildren<SpriteRenderer>().ToList()
            .FirstOrDefault(sr1 => srs.Contains(sr1));

    SpriteRenderer getAvailableEffect(List<SpriteRenderer> srs, GameObject prefab)
    {
        //Find existing effect
        SpriteRenderer sr = srs.FirstOrDefault(
            tsu => !tsu.enabled
            );
        //Else make a new one
        if (sr == null)
        {
            GameObject newLS = GameObject.Instantiate(prefab);
            SpriteRenderer newSR = newLS.GetComponent<SpriteRenderer>();
            srs.Add(newSR);
            sr = newSR;
            //Fader (if any)
            Fader fader = newLS.GetComponent<Fader>();
            if (fader)
            {
                fader.onFadeFinished += () => newLS.transform.parent = transform;
            }
        }
        return sr;
    }

    void parentSpriteRendererToObject(SpriteRenderer sr, GameObject go, bool useSizeSR = false)
    {
        sr.transform.parent = go.transform;
        sr.transform.localPosition = Vector2.zero;
        sr.transform.up = go.transform.up;
        Vector2 size = go.getSize();
        if (useSizeSR)
        {
            sr.size = size;
        }
        else
        {
            Vector3 scale = Vector3.one * Mathf.Max(size.x, size.y);
            Vector3 goScale = go.transform.localScale;
            sr.transform.localScale = new Vector3(
                scale.x / goScale.x,
                scale.y / goScale.y,
                scale.z / goScale.z
                );
        }
    }

    /// <summary>
    /// Shows the lightning static effect on the given GameObject
    /// 2020-12-17: copied from showTeleportStar()
    /// </summary>
    /// <param name="pos"></param>
    public void showLightningStatic(GameObject go, bool show = true)
    {
        if (show)
        {
            //Check to make sure there isn't already an effect on it
            if (isEffectOnObject(lightningStaticList, go))
            {
                return;
            }
            //Process effect
            SpriteRenderer sr = getAvailableEffect(
                lightningStaticList,
                lightningStaticPrefab
                );
            parentSpriteRendererToObject(sr, go, true);
            sr.enabled = true;
        }
        else
        {
            SpriteRenderer sr = go.GetComponentsInChildren<SpriteRenderer>().ToList()
                .FirstOrDefault(sr1 => lightningStaticList.Contains(sr1));
            if (sr)
            {
                //Parent it under EffectManager
                sr.transform.parent = transform;
                //Hide it
                sr.enabled = false;
            }
            else
            {
                Debug.LogWarning(
                    "Tried to remove lightning static effect from GameObject " + go.name
                    + " that does not have the effect on it.",
                    go
                    );
            }
        }
    }

    /// <summary>
    /// Shows the swap circle effect on the given GameObject
    /// 2020-12-21: copied from showSwapStasis()
    /// </summary>
    /// <param name="pos"></param>
    public void showSwapCircle(GameObject go, bool show = true)
    {
        if (show)
        {
            //Check to make sure there isn't already an effect on it
            if (isEffectOnObject(swapCircleList, go))
            {
                return;
            }
            //Process effect
            SpriteRenderer sr = getAvailableEffect(
                swapCircleList,
                swapCirclePrefab
                );
            parentSpriteRendererToObject(sr, go);
            sr.enabled = true;
        }
        else
        {
            SpriteRenderer sr = go.GetComponentsInChildren<SpriteRenderer>().ToList()
                .FirstOrDefault(sr1 => swapCircleList.Contains(sr1));
            if (sr)
            {
                //Parent it under EffectManager
                sr.transform.parent = transform;
                //Hide it
                sr.enabled = false;
            }
            else
            {
                Debug.LogWarning(
                    "Tried to remove swap circle effect from GameObject " + go.name
                    + " that does not have the effect on it.",
                    go
                    );
            }
        }
    }

    public void hideSwapCircleEffects(List<GameObject> exceptions)
    {
        swapCircleList.RemoveAll(sr => sr == null);
        swapCircleList
            .FindAll(sr => !exceptions.Contains(sr.transform.parent.gameObject))
            .ForEach(sr =>
            {
                //Parent it under EffectManager
                sr.transform.parent = transform;
                //Hide it
                sr.enabled = false;
            });
    }

    /// <summary>
    /// Shows the swap rotate effect on the given GameObject
    /// 2020-12-18: copied from showLightningStatic()
    /// </summary>
    /// <param name="pos"></param>
    public void showSwapRotate(GameObject go, bool flip = false)
    {
        //Clean null values from list
        //(in case an object takes the effect with it when it unloads)
        swapRotateList.RemoveAll(tsu => tsu == null);
        //Check to make sure there isn't already an effect on it
        if (isEffectOnObject(swapRotateList, go))
        {
            return;
        }
        //Process effect
        SpriteRenderer sr = getAvailableEffect(
            swapRotateList,
            swapRotatePrefab
            );
        parentSpriteRendererToObject(sr, go);
        Vector3 scale = sr.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * ((flip) ? -1 : 1);
        sr.transform.localScale = scale;
        SimpleRotation rot = sr.GetComponent<SimpleRotation>();
        rot.turnSpeed = Mathf.Abs(rot.turnSpeed) * ((flip) ? 1 : -1);
        sr.GetComponent<Fader>().enabled = true;
        sr.enabled = true;
    }

    /// <summary>
    /// Shows the swap stasis effect on the given GameObject
    /// 2020-12-18: copied from showLightningStatic()
    /// </summary>
    /// <param name="pos"></param>
    public void showSwapStasis(GameObject go, bool show = true)
    {
        if (show)
        {
            //Clean null values from list
            //(in case an object takes the effect with it when it unloads)
            swapStasisList.RemoveAll(tsu => tsu == null);
            //Check to make sure there isn't already an effect on it
            if (isEffectOnObject(swapStasisList, go))
            {
                return;
            }
            //Process effect
            SpriteRenderer sr = getAvailableEffect(
                swapStasisList,
                swapStasisPrefab
                );
            parentSpriteRendererToObject(sr, go);
            sr.enabled = true;
        }
        else
        {
            SpriteRenderer sr = go.GetComponentsInChildren<SpriteRenderer>().ToList()
                .FirstOrDefault(sr1 => swapStasisList.Contains(sr1));
            if (sr)
            {
                //Parent it under EffectManager
                sr.transform.parent = transform;
                //Hide it
                sr.enabled = false;
            }
            else
            {
                Debug.LogWarning(
                    "Tried to remove swap stasis effect from GameObject " + go.name
                    + " that does not have the effect on it.",
                    go
                    );
            }
        }
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

    public void showRewindEffect(bool show)
    {
        rewindCanvas.SetActive(show);
    }
}
