using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCManager : MonoBehaviour
{

    public GameObject npcTalkEffect;//the particle system for the visual part of NPC talking
    private static GameObject lastTalkingNPC;//the last NPC to talk
    public Text npcDialogueText;
    public Canvas canvas;
    public GameObject npcQuoteBox;
    public GameObject npcQuoteBoxTail;

    private static NPCManager instance;
    private static MusicManager musicManager;

    // Use this for initialization
    void Start()
    {
        //instance
        if (instance == null)
        {
            instance = this;

            npcDialogueText.fontSize = (int)(Camera.main.pixelHeight * 0.05f);
            musicManager = FindObjectOfType<MusicManager>();
            if (!instance.npcTalkEffect.GetComponent<ParticleSystem>().isPlaying)
            {
                instance.canvas.gameObject.SetActive(false);
                instance.enabled = false;
            }
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        RectTransform canTrans = ((RectTransform)canvas.transform);
        canTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Camera.main.pixelWidth);
        canTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Camera.main.pixelHeight);
        Vector2 size = cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth, cam.pixelHeight)) - cam.ScreenToWorldPoint(Vector2.zero);
        float newDim = Mathf.Max(Mathf.Abs(size.x) / canTrans.rect.width, Mathf.Abs(size.y) / canTrans.rect.height);
        Vector3 newSize = new Vector3(newDim, newDim, 1);
        canvas.transform.localScale = newSize;
        ((RectTransform)npcDialogueText.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Camera.main.pixelWidth * 3 / 4);
        ((RectTransform)npcDialogueText.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Camera.main.pixelHeight * 3 / 4);
        canvas.transform.rotation = cam.transform.rotation;
    }

    /// <summary>
    /// Activates the visual effects for the given npc talking
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="talking">Whether to activate or deactivate the visual effects</param>
    public static void speakNPC(GameObject npc, bool talking, string message, string eventualMessage)
    {
        instance.canvas.gameObject.SetActive(talking);
        instance.npcQuoteBox.SetActive(talking);
        instance.npcDialogueText.text = message;
        instance.enabled = talking;
        if (talking)
        {
            instance.npcTalkEffect.transform.position = npc.transform.position;
            //Show text
            float textHeight = getTextHeight(instance.canvas, instance.npcDialogueText);
            float buffer = textHeight / 2;
            Vector2 textBoxSize =
                getMessageDimensions(instance.canvas, instance.npcDialogueText, eventualMessage)
                + (Vector2.one * buffer * 2);
            int maxTextLength = getMaxTextLength(instance.canvas, instance.npcDialogueText);
            instance.canvas.transform.position = npc.transform.position + Camera.main.transform.up.normalized * (textHeight * 3 + npc.GetComponent<SpriteRenderer>().bounds.extents.y);
            instance.npcDialogueText.text = processMessage(instance.canvas, instance.npcDialogueText, message, maxTextLength);
            //Show quote box
            instance.npcQuoteBox.transform.position = instance.canvas.transform.position;
            SpriteRenderer quoteSR = instance.npcQuoteBox.GetComponent<SpriteRenderer>();
            quoteSR.size = textBoxSize;
            instance.npcQuoteBoxTail.transform.position = instance.npcQuoteBox.transform.position - (Vector3.up * quoteSR.size.y / 2);
            //Show speaking particles
            if (!instance.npcTalkEffect.GetComponent<ParticleSystem>().isPlaying)
            {
                instance.npcTalkEffect.GetComponent<ParticleSystem>().Play();
            }
            if (lastTalkingNPC != npc)
            {
                lastTalkingNPC = npc;
                musicManager.setQuiet(true);
            }
        }
        else
        {
            if (npc == lastTalkingNPC)
            {
                musicManager.setQuiet(false);
                instance.npcTalkEffect.GetComponent<ParticleSystem>().Stop();
            }
        }
    }

    static float getTextWidth(Canvas canvas, Text text, int length)
    {
        return text.fontSize * 0.5f * length * canvas.transform.localScale.x;
    }
    static float getTextHeight(Canvas canvas, Text text, int lines = 1)
    {
        return text.fontSize * lines * canvas.transform.localScale.y;
    }
    static float getMaxWidth(Canvas canvas, Text text)
    {
        return Mathf.Min(Screen.width / 2, text.rectTransform.rect.width * canvas.transform.localScale.x);
    }

    static int getTextLength(Canvas canvas, Text text, float width)
    {
        return Mathf.FloorToInt(width / (text.fontSize * 0.5f * canvas.transform.localScale.x));
    }
    static int getMaxTextLength(Canvas canvas, Text text)
    {
        return getTextLength(canvas, text, getMaxWidth(canvas, text));
    }


    static Vector2 getMessageDimensions(Canvas canvas, Text text, string message, int maxTextLength = 0)
    {
        string[] strings = splitIntoSegments(canvas, text, message, maxTextLength);
        int foundMaxLength = 0;
        foreach (string s in strings)
        {
            if (s.Length > foundMaxLength)
            {
                foundMaxLength = s.Length;
            }
        }
        float textWidth = getTextWidth(canvas, text, foundMaxLength);
        float textHeight = getTextHeight(canvas, text, strings.Length);
        return new Vector2(textWidth, textHeight);
    }

    static string processMessage(Canvas canvas, Text text, string message, int maxTextLength)
    {
        string[] strings = splitIntoSegments(canvas, text, message, maxTextLength);
        string buildString = addSpaces(strings[0], maxTextLength - strings[0].Length);
        for (int i = 1; i < strings.Length; i++)
        {
            strings[i] = addSpaces(strings[i], maxTextLength - strings[i].Length);
            buildString += "\n" + strings[i];
        }
        return buildString;
    }
    static string addSpaces(string original, int spaceCount)
    {
        string buildString = original;
        for (int i = 0; i < spaceCount; i++)
        {
            buildString += " ";
        }
        Debug.Log("adding " + spaceCount + " spaces to: " + buildString + ".");
        return buildString;
    }

    static string[] splitIntoSegments(Canvas canvas, Text text, string message, int maxTextLength = 0)
    {
        float textWidth = getTextWidth(canvas, text, message.Length);
        float maxWidth = getMaxWidth(canvas, text);
        int segmentLength = message.Length;
        if (textWidth > maxWidth)
        {
            if (maxTextLength == 0)
            {
                maxTextLength = getMaxTextLength(canvas, text);
            }
            segmentLength = maxTextLength;
        }
        return splitIntoSegments(message, segmentLength);
    }
    static string[] splitIntoSegments(string message, int maxLength)
    {
        string[] split = message.Split(' ');
        List<string> strings = new List<string>();
        int sumLength = split[0].Length;
        string buildString = split[0];
        for (int i = 1; i < split.Length; i++)
        {
            if (sumLength + split[i].Length + 1 > maxLength)
            {
                sumLength = 0;
                strings.Add(buildString);
            }
            sumLength += split[i].Length + 1;
            buildString += " " + split[i];
        }
        strings.Add(buildString);
        return strings.ToArray();
    }
}
