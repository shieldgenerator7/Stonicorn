using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVoiceLineTrigger : MonoBehaviour {

    public NPCController controller;
    private NPCVoiceLine lineToTrigger;
    public int lineToTriggerIndex;

	// Use this for initialization
	void Start () {
        lineToTrigger = controller.voiceLines[lineToTriggerIndex];
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!lineToTrigger.played
            && GameEventManager.eventHappened(lineToTrigger.eventReq)
            && (lineToTrigger.eventReqExclude != null || !GameEventManager.eventHappened(lineToTrigger.eventReqExclude)))
        {
            controller.setTriggerVoiceLine(lineToTrigger);
        }
    }
}
