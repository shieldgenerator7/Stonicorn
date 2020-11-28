using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class NPCController : SavableMonoBehaviour
{
    private AudioSource audioSource;
    AudioSource Source
    {
        get
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            return audioSource;
        }
    }

    public string lineFileName;//the file that has the list of voice lines in it
    public List<NPCVoiceLine> voiceLines;

    public float interruptDistance = 10.0f;//if Merky goes further than this distance, the NPC stops talking

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
        //Read in the NPC's lines
        if (voiceLines == null)
        {
            refreshVoiceLines();
        }
    }
    void refreshVoiceLines()
    {
        if (lineFileName != null && lineFileName != "")
        {
            voiceLines = new List<NPCVoiceLine>();//2017-09-05 ommitted until text files are filled out
            int writeIndex = -1;
            //2017-09-05: copied from an answer by Drakestar: http://answers.unity3d.com/questions/279750/loading-data-from-a-txt-file-c.html
            try
            {
                string lineFileFSPath = "Assets/Resources/Dialogue/" + lineFileName; // Relative path to linefile on the filesystem.
                List<string> fileLines; // Array of script file lines.

                // Attempt to read the data file from the local filesystem.  If it doesn't exist fall back to the
                // packed assets.  This is to allow overriding base behaviors with custom client-side ones.
                if (File.Exists(lineFileFSPath))
                {
                    StreamReader theReader = new StreamReader(lineFileFSPath, Encoding.Default);
                    fileLines = new List<string>(theReader.ReadToEnd().Split('\n'));
                    theReader.Close();
                }
                else
                {
                    TextAsset internalFile = Resources.Load<TextAsset>("Dialogue/" + lineFileName.Split('.')[0]);

                    Debug.Assert(internalFile != null, "Could not load fallback NPC text script assumed to be located at Dialogue/" + lineFileName + "!");

                    fileLines = new List<string>(internalFile.ToString().Split('\n'));
                }

                foreach (string line in fileLines)
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
                        voiceLines[writeIndex].voiceLine = Resources.Load<AudioClip>("Dialogue/" + audioPath);
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
            // If anything broke in the try block, we throw an exception with information
            // on what didn't work
            catch (System.Exception e)
            {
                Debug.LogError("{0} lineFileName: " + lineFileName + "\n>>>" + e.Message + "\n" + e.StackTrace);
            }
        }
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "currentVoiceLineIndex", currentVoiceLineIndex,
            "playBackTime", Source.time,
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
        Source.transform.position = transform.position;
        if (canGreet())
        {
            if (!Source.isPlaying)
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
                Source.Stop();
            }
        }
        if (Source.isPlaying)
        {
            string voicelinetext = voiceLines[currentVoiceLineIndex].getVoiceLineText(Source.time);
            string voicelinetextWhole = voiceLines[currentVoiceLineIndex].getVoiceLineText(Source.time, true);
            if (voicelinetext.Length > voicelinetextWhole.Length)
            {
                throw new UnityException("Voice line text greater than whole! part: " + voicelinetext.Length + "; whole: " + voicelinetextWhole.Length);
            }
            NPCManager.speakNPC(gameObject, true, voicelinetext, voicelinetextWhole);
        }
        else if (currentVoiceLineIndex >= 0 && !Managers.Time.Paused)
        {
            currentVoiceLineIndex = -1;
            NPCManager.speakNPC(gameObject, false, "", "");
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
        Vector3 playerPos = Managers.Player.transform.position;
        float distance = Vector3.Distance(playerPos, transform.position);
        if (distance > 5)
        {
            return false;
        }
        Utility.RaycastAnswer answer = Utility.RaycastAll(transform.position, playerPos - transform.position, distance);
        int thingsFound = answer.count;
        //If only 2 things, there's nothing in between
        if (thingsFound > 2)
        {
            for (int i = 0; i < answer.count; i++)
            {
                RaycastHit2D rch2d = answer.rch2ds[i];
                //If the thing in between is just a trigger, don't worry about it
                if (!rch2d.collider.isTrigger
                    && !rch2d.collider.gameObject.isPlayer()
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
        return Vector3.Distance(Managers.Player.transform.position, transform.position) > interruptDistance;
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
        if (voiceLines == null)
        {
            refreshVoiceLines();
        }
        if (index >= 0 && index < voiceLines.Count)
        {
            currentVoiceLineIndex = index;
            Source.clip = voiceLines[index].voiceLine;
            Source.time = timePos;
            if (!Source.isPlaying)
            {
                Source.Play();
            }
        }
        else
        {
            if (index != -1)
            {
                Debug.LogError(gameObject.name + ".setVoiceLine: invalid index: " + index);
            }
            if (Source != null && Source.isPlaying)
            {
                Source.Stop();
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
        if (currentVoiceLineIndex != index || !Source.isPlaying)
        {
            setVoiceLine(index);
        }
    }

    /// <summary>
    /// Called from TimeManager.onPausedChanged() delegate
    /// </summary>
    /// <param name="paused"></param>
    internal void pauseDialogue(bool paused)
    {
        if (paused)
        {
            if (Source.isPlaying)
            {
                Source.Pause();
            }
        }
        else
        {
            if (currentVoiceLineIndex >= 0)
            {
                Source.Play();
            }
        }
    }
}
