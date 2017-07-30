using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVoiceLine: SavableMonoBehaviour {

    //Settings
    public AudioClip voiceLine;
    public string eventReq = null;//the NPC will only say the voice line if the given event has happened
    public string triggerEvent = null;//the event this voiceline will trigger, if any
    public bool checkPointLine = true;//true: if this line has played, prev lines can not be played
    //State
    public bool played = false;

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "played", played);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        played = (bool)savObj.data["played"];
    }
}
