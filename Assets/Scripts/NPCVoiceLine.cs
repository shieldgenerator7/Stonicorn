using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVoiceLine: MonoBehaviour {

    public AudioClip voiceLine;
    public string eventReq = null;//the NPC will only say the voice line if the given event has happened
    public bool played = false;
    public string triggerEvent = null;//the event this voiceline will trigger, if any
}
