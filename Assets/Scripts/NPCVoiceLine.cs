using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVoiceLine {

    //Settings
    public AudioClip voiceLine;
    public string voiceLineText;//the text that will show up as subtitles
    public string eventReq = null;//the NPC will only say the voice line if the given event has happened
    public string eventReqExclude = null;//this line will not be said after this event has happened
    public string triggerEvent = null;//the event this voiceline will trigger, if any
    public bool checkPointLine = true;//true: if this line has played, prev lines can not be played
    public bool triggerLine = false;//true: this line only plays when the player enters a trigger
    /// <summary>
    /// Each segment of text is separated by a comma or period,
    /// and is associated with a time in the audio when it begins
    /// </summary>
    public class Line
    {
        string endCharacter;//the last character in this line segment
        public string lineText;//the contents of this line segment
        public float audioBeginTime = 0.0f;//when this line segment begins

        public Line(string text)
        {
            this.lineText = text;
        }
        public Line(string endChar, float beginTime)
        {
            this.endCharacter = endChar;
            this.audioBeginTime = beginTime;
        }
        /// <summary>
        /// "Bites off" a piece of the given string, and returns the left over chunk
        /// </summary>
        /// <param name="voiceLineText"></param>
        /// <returns></returns>
        public string bite(string voiceLineText)
        {
            int lastCharIndex = voiceLineText.IndexOf(endCharacter);
            lineText = voiceLineText.Substring(0, lastCharIndex + 1).Trim();
            return voiceLineText.Substring(lastCharIndex + 1);
        }
    }
    public List<Line> lineSegments = new List<Line>();
    //State
    public bool played = false;
    private int prevCurrentLine = 0;//for efficiency: the last appropriate line segment for getVoiceLineText()

    /// <summary>
    /// Returns the appropriate line segment based on the current audio playback time
    /// </summary>
    /// <param name="playtime"></param>
    /// <returns></returns>
    public string getVoiceLineText(float playtime)
    {
        if (lineSegments.Count == 0)
        {
            return voiceLineText;
        }
        if (lineSegments[prevCurrentLine].audioBeginTime <= playtime)
        {
            while (prevCurrentLine <= lineSegments.Count-2 && lineSegments[prevCurrentLine + 1].audioBeginTime <= playtime)
            {
                prevCurrentLine++;
            }
        }
        else
        {
            for (int i = prevCurrentLine; i >= 0; i--)
            {
                if (lineSegments[i].audioBeginTime <= playtime)
                {
                    prevCurrentLine = i;
                }
            }
        }
        if (prevCurrentLine < lineSegments.Count-1)
        {
            float percentage = (playtime - lineSegments[prevCurrentLine].audioBeginTime) / (lineSegments[prevCurrentLine + 1].audioBeginTime - lineSegments[prevCurrentLine].audioBeginTime);
            string text = lineSegments[prevCurrentLine].lineText;
            int index = (int)(percentage * text.Length);
            index = Mathf.Min(index + 7, text.Length);
            return text.Substring(0, index);
        }
        return lineSegments[prevCurrentLine].lineText;
    }

    /// <summary>
    /// Returns whether this voiceline has a valid exclude requirement
    /// </summary>
    /// <returns></returns>
    public bool hasExcludeRequirement()
    {
        return eventReqExclude != null && eventReqExclude.Trim() != "";
    }
}
