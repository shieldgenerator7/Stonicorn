using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class NPCController : SavableMonoBehaviour
{
    AudioSource source;

    public string lineFileName;//the file that has the list of voice lines in it
    public List<NPCVoiceLine> voiceLines;

    private GameObject playerObject;
    private static GUIStyle npcTextStyle;

    //State
    /// <summary>
    /// The index of the voiceline that is currently playing
    /// -1 if none
    /// </summary>
    public int currentVoiceLineIndex = -1;
    /// <summary>
    /// The index of the checkpoint voiceline that last played
    /// </summary>
    public int lastPlayedCheckPointLineIndex = -1;

    // Use this for initialization
    protected virtual void Start()
    {
        source = GetComponent<AudioSource>();
        playerObject = GameManager.getPlayerObject();
        if (source == null)
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }
        if (npcTextStyle == null)
        {
            npcTextStyle = new GUIStyle();
            npcTextStyle.fontSize = (int)(Camera.main.pixelHeight * 0.05f);
            npcTextStyle.wordWrap = true;
            npcTextStyle.alignment = TextAnchor.UpperCenter;
        }
        if (lineFileName != null && lineFileName != "")
        {
            voiceLines = new List<NPCVoiceLine>();//2017-09-05 ommitted until text files are filled out
            int writeIndex = -1;
            //2017-09-05: copied from an answer by Drakestar: http://answers.unity3d.com/questions/279750/loading-data-from-a-txt-file-c.html
            try
            {
                string line;
                StreamReader theReader = new StreamReader("Assets/Resources/Dialogue/"+lineFileName, Encoding.Default);
                using (theReader)
                {
                    do
                    {
                        line = theReader.ReadLine();

                        if (line != null)
                        {
                            if (line.StartsWith(":"))
                            {
                                writeIndex++;
                                NPCVoiceLine npcvl = new NPCVoiceLine();
                                voiceLines.Add(npcvl);
                            }
                            else if (line.StartsWith("audio:"))
                            {
                                string audioPath = line.Substring("audio:".Length).Trim();
                                voiceLines[writeIndex].voiceLine = Resources.Load<AudioClip>("Dialogue/"+audioPath);
                            }
                            else if (line.StartsWith("text:"))
                            {
                                string text = line.Substring("text:".Length).Trim();
                                voiceLines[writeIndex].voiceLineText = text;
                                if (voiceLines[writeIndex].lineSegments.Count == 0)
                                {
                                    voiceLines[writeIndex].lineSegments.Add(new NPCVoiceLine.Line(text));
                                }
                            }
                            else if (line.StartsWith("segments:"))
                            {
                                string segmentText = line.Substring("segments:".Length).Trim();
                                voiceLines[writeIndex].lineSegments.Clear();
                                string voiceLineText = voiceLines[writeIndex].voiceLineText;
                                foreach (string s in segmentText.Split('>'))
                                {
                                    string[] strs = s.Trim().Split(' ');
                                    NPCVoiceLine.Line lineSegment = new NPCVoiceLine.Line(strs[0], float.Parse(strs[1]));
                                    voiceLines[writeIndex].lineSegments.Add(lineSegment);
                                    voiceLineText = lineSegment.bite(voiceLineText);
                                }
                                //Add a dummy line segment for text animation purposes
                                voiceLines[writeIndex].lineSegments.Add(new NPCVoiceLine.Line(null, voiceLines[writeIndex].voiceLine.length));
                            }
                            else if (line.StartsWith("req:"))
                            {
                                string eventName = line.Substring("req:".Length).Trim();
                                voiceLines[writeIndex].eventReq = eventName;
                            }
                            else if (line.StartsWith("exclude:"))
                            {
                                string eventName = line.Substring("exclude:".Length).Trim();
                                voiceLines[writeIndex].eventReqExclude = eventName;
                            }
                            else if (line.StartsWith("event:"))
                            {
                                string eventName = line.Substring("event:".Length).Trim();
                                voiceLines[writeIndex].triggerEvent = eventName;
                            }
                            else if (line.StartsWith("cpl:"))
                            {
                                bool cpSetting = bool.Parse(line.Substring("cpl:".Length).Trim());
                                voiceLines[writeIndex].checkPointLine = cpSetting;
                            }
                            else if (line.StartsWith("trigger:"))
                            {
                                bool triggerSetting = bool.Parse(line.Substring("trigger:".Length).Trim());
                                voiceLines[writeIndex].triggerLine = triggerSetting;
                            }
                        }
                    }
                    while (line != null);
                    theReader.Close();
                }
            }
            // If anything broke in the try block, we throw an exception with information
            // on what didn't work
            catch (System.Exception e)
            {
                Debug.LogError("{0} " + e.Message+ "\n"+e.StackTrace);
            }
        }
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "currentVoiceLineIndex", currentVoiceLineIndex,
            "playBackTime", source.time,
            "lastPlayedCheckPointLineIndex", lastPlayedCheckPointLineIndex);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        currentVoiceLineIndex = (int)savObj.data["currentVoiceLineIndex"];
        float playBackTime = (float)savObj.data["playBackTime"];
        setVoiceLine(currentVoiceLineIndex, playBackTime);
        lastPlayedCheckPointLineIndex = (int)savObj.data["lastPlayedCheckPointLineIndex"];
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
                int mrvli = getMostRelevantVoiceLineIndex();
                if (mrvli >= 0)
                {
                    setVoiceLine(mrvli);
                    NPCVoiceLine npcvl = voiceLines[mrvli];
                    npcvl.played = true;
                    lastPlayedCheckPointLineIndex = mrvli;
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
        else
        {
            currentVoiceLineIndex = -1;
            GameManager.speakNPC(gameObject, false);
        }
    }

    private void OnGUI()
    {
        if (source.isPlaying)
        {
            GUI.backgroundColor = Color.clear;
            float bufferWidth = Camera.main.pixelWidth * 0.25f;
            float bufferHeight = Camera.main.pixelHeight * 0.25f;
            Vector2 posOnCam = Camera.main.WorldToScreenPoint(transform.position+Camera.main.transform.up.normalized*2);
            bufferWidth = bufferHeight = Mathf.Min(bufferWidth, bufferHeight);
            string voicelinetext = voiceLines[currentVoiceLineIndex].getVoiceLineText(source.time);
            float rectWidth = Camera.main.pixelWidth - bufferWidth * 2;
            Rect bufferRect = new Rect(posOnCam.x-(rectWidth/2), Camera.main.pixelHeight-posOnCam.y, rectWidth, Camera.main.pixelHeight - bufferHeight * 2);
            GUI.Label(bufferRect, voicelinetext, npcTextStyle);
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
        if (distance > 5)
        {
            return false;
        }
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, playerObject.transform.position - transform.position, distance);
        int thingsFound = hits.Length;
        //If only 2 things, there's nothing in between
        if (thingsFound > 2)
        {
            foreach (RaycastHit2D rch2d in hits)
            {
                //If the thing in between is just a trigger, don't worry about it
                if (!rch2d.collider.isTrigger
                    && rch2d.collider.gameObject != playerObject
                    && rch2d.collider.gameObject != gameObject)
                {
                    return false;
                }
            }
        }
        return true;
    }

    protected virtual bool shouldStop()
    {
        if (currentVoiceLineIndex >= 0
            && voiceLines[currentVoiceLineIndex].triggerLine)
        {
            return false;
        }
        return Vector3.Distance(playerObject.transform.position, transform.position) > 10;
    }

    public NPCVoiceLine getMostRelevantVoiceLine()
    {
        int mrvli = getMostRelevantVoiceLineIndex();
        if (mrvli < 0)
        {
            return null;
        }
        return voiceLines[mrvli];
    }
    public int getMostRelevantVoiceLineIndex()
    {
        for (int i = voiceLines.Count - 1; i > lastPlayedCheckPointLineIndex; i--)
        {
            NPCVoiceLine npcvl = voiceLines[i];
            if (!npcvl.triggerLine && !npcvl.played
                && GameEventManager.eventHappened(npcvl.eventReq)
                && (!npcvl.hasExcludeRequirement() || !GameEventManager.eventHappened(npcvl.eventReqExclude)))
            {
                return i;
            }
            else if (npcvl.played && npcvl.checkPointLine)
            {
                return -1;
            }
        }
        return -1;
    }

    /// <summary>
    /// Sets the current voiceline and the playback time
    /// </summary>
    /// <param name="index">The index in the voiceLines array of the voiceline to play</param>
    /// <param name="timePos">The playback time</param>
    public void setVoiceLine(int index, float timePos = 0)
    {
        if (index >= 0 && index < voiceLines.Count)
        {
            currentVoiceLineIndex = index;
            source.clip = voiceLines[index].voiceLine;
            source.time = timePos;
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
        else
        {
            if (index != -1)
            {
                Debug.LogError(gameObject.name + ".setVoiceLine: invalid index: " + index);
            }
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    /// <summary>
    /// Used by outside scripts to set voicelines of NPC's reaction to an event
    /// Particularly events such as the player entering a certain area
    /// </summary>
    /// <param name="npcvl"></param>
    public void setTriggerVoiceLine(NPCVoiceLine npcvl)
    {
        int index = voiceLines.IndexOf(npcvl);
        if (currentVoiceLineIndex != index || !source.isPlaying)
        {
            setVoiceLine(index);
        }
    }
}
