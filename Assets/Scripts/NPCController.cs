using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{

    public AudioClip greeting;
    AudioSource source;
    
    bool greetingPlayed = false;

    public List<NPCVoiceLine> voiceLines;

    private GameObject playerObject;

    // Use this for initialization
    protected virtual void Start()
    {
        source = GetComponent<AudioSource>();
        playerObject = GameManager.getPlayerObject();
    }

    // Update is called once per frame
    void Update()
    {
        source.transform.position = transform.position;
        //Debug.Log("Number things found: " + thingsFound);
        if (canGreet())
        {
            if (!source.isPlaying)
            {
                NPCVoiceLine npcvl = getMostRelevantVoiceLine();
                if (npcvl != null)
                {
                    source.clip = npcvl.voiceLine;
                    source.Play();
                    npcvl.played = true;
                    if (npcvl.triggerEvent != null)
                    {
                        GameEventManager.addEvent(npcvl.triggerEvent);
                    }
                }
            }
        }
        else
        {
            if (shouldStop())
            {
                source.Stop();
            }
        }
        if (source.isPlaying)
        {
            GameManager.speakNPC(gameObject, true);
        }
        else {
            GameManager.speakNPC(gameObject, false);
        }
    }

    /// <summary>
    /// Whether or not this NPC should only greet once
    /// </summary>
    /// <returns></returns>
    protected virtual bool greetOnlyOnce()
    {
        return true;
    }

    /// <summary>
    /// Returns whether or not this NPC can play its greeting voiceline
    /// </summary>
    /// <returns></returns>
    protected virtual bool canGreet()
    {
        float distance = Vector3.Distance(playerObject.transform.position, transform.position);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, playerObject.transform.position - transform.position, distance);
        int thingsFound = hits.Length;
        return distance < 5 && thingsFound == 2;
    }

    protected virtual bool shouldStop()
    {
        return false;
    }

    public NPCVoiceLine getMostRelevantVoiceLine()
    {
        for(int i = voiceLines.Count-1; i >=0; i--)
        {
            NPCVoiceLine npcvl = voiceLines[i];
            if (!npcvl.played && GameEventManager.eventHappened(npcvl.eventReq))
            {
                return npcvl;
            }
        }
        return null;
    }
}
