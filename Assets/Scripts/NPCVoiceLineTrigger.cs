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
            && Managers.Progress.hasActivated(lineToTrigger.eventReq)
            && (!lineToTrigger.hasExcludeRequirement() || !Managers.Progress.hasActivated(lineToTrigger.eventReqExclude)))
        {
            controller.setTriggerVoiceLine(lineToTrigger);
        }
    }
}
