using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxUpdater : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The scale point at which the NPC quote box should be full screen")]
    public CameraController.CameraScalePoints baseCameraScalePoint;

    [Header("Components")]
    public ParticleSystem talkEffect;//the particle system for the visual part of NPC talking
    public Text txtDialogue;
    public Canvas canvas;
    public GameObject quoteBox;
    public GameObject quoteBoxTail;

    private string rawText;
    private List<string> textLines;
    private Transform source;
    private Vector2 offset = Vector2.up * 1;

    // Start is called before the first frame update
    void Start()
    {
        txtDialogue.fontSize = (int)(Camera.main.pixelHeight * 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        //resize dialogue box
        resizeToCamera();
        //rotate dialogue box
        Quaternion rotation = Camera.main.transform.rotation;
        canvas.transform.rotation = rotation;
        quoteBox.transform.rotation = rotation;
    }

    public void setText(string text)
    {
        this.rawText = text;
    }

    public void setSource(Transform source)
    {
        this.source = source;
    }

    public void resizeToCamera()
    {
        //set canvas rect size
        Camera cam = Camera.main;
        CameraController camCtr = Managers.Camera;
        RectTransform canvasRect = ((RectTransform)canvas.transform);
        canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Camera.main.pixelWidth);
        canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Camera.main.pixelHeight);
        //set canvas localscale
        Vector2 size = camCtr.CamSizeWorld;
        size *= (camCtr.toZoomLevel(baseCameraScalePoint) / camCtr.ZoomLevel);
        float newDim = Mathf.Max(Mathf.Abs(size.x) / canvasRect.rect.width, Mathf.Abs(size.y) / canvasRect.rect.height);
        Vector3 newSize = new Vector3(newDim, newDim, 1);
        canvas.transform.localScale = newSize;
        //set text rect size
        RectTransform dialogueRect = ((RectTransform)txtDialogue.transform);
        dialogueRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cam.pixelWidth * 3 / 4);
        dialogueRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cam.pixelHeight * 3 / 4);
    }

    /// <summary>
    /// Activates the visual effects for the given npc talking
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="talking">Whether to activate or deactivate the visual effects</param>
    public void speakNPC(NPCController npc, bool talking, string message, string eventualMessage)
    {
        canvas.gameObject.SetActive(talking);
        quoteBox.SetActive(talking);
        txtDialogue.text = message;
        enabled = talking;
        if (talking)
        {
            talkEffect.transform.position = npc.transform.position;
            //Show text
            float textHeight = getTextHeight(canvas, txtDialogue);
            float buffer = textHeight / 2;
            int maxTextLength = getMaxTextLength(canvas, txtDialogue);
            Vector2 messageDimensions = getMessageDimensions(canvas, txtDialogue, eventualMessage);
            maxTextLength = getTextLength(canvas, txtDialogue, messageDimensions.x);
            if (messageDimensions.y > textHeight)
            {
                float textWidth = getTextWidth(canvas, txtDialogue, eventualMessage);
                int lineCount = Mathf.CeilToInt(textWidth / getMaxWidth(canvas, txtDialogue));
                maxTextLength = (maxTextLength + (eventualMessage.Length / lineCount)) / 2;
                messageDimensions = getMessageDimensions(canvas, txtDialogue, eventualMessage, maxTextLength);
                maxTextLength = getTextLength(canvas, txtDialogue, messageDimensions.x);
            }
            Vector2 textBoxSize = messageDimensions + (Vector2.one * buffer * 2);
            canvas.transform.position = npc.transform.position + Camera.main.transform.up.normalized * (textHeight * 3 + npc.GetComponent<SpriteRenderer>().bounds.extents.y);
            txtDialogue.text = processMessage(canvas, txtDialogue, message, maxTextLength);
            //Show quote box
            quoteBox.transform.position = canvas.transform.position;
            SpriteRenderer quoteSR = quoteBox.GetComponent<SpriteRenderer>();
            quoteSR.size = textBoxSize;
            quoteBoxTail.transform.position = quoteBox.transform.position - (quoteBox.transform.up * quoteSR.size.y / 2);
            //Show speaking particles
            if (!talkEffect.isPlaying)
            {
                talkEffect.Play();
            }
        }
    }

    static float getTextWidth(Canvas canvas, Text text, string stringToMeasure)
    {
        return getSumOfCharacterOffsets(text, stringToMeasure) * canvas.transform.localScale.x;
        /*
        string prevString = text.text;
        //text.text = stringToMeasure;
        //2019-02-28: copied from an answer by pineda100: https://answers.unity.com/questions/921726/how-to-get-the-size-of-a-unityengineuitext-for-whi.html
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
        float width = textGen.GetPreferredWidth(stringToMeasure, generationSettings);
        float height = textGen.GetPreferredHeight(stringToMeasure, generationSettings);
        Vector2 size = new Vector2(width, height);
        size = Camera.main.ScreenToWorldPoint(size) - Camera.main.ScreenToWorldPoint(Vector2.zero);
        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);
        text.text = prevString;
        return size.x;*/
        //return text.fontSize * 0.5f * length * canvas.transform.localScale.x;
    }
    /// <summary>
    /// Adds up all the character offsets for the given string using the supplied text's font settings.
    /// Should ideally return the total width, in pixels, that the string takes up when rendered.
    /// </summary>
    /// <param name="text">Text object used to get font settings.</param>
    /// <param name="textString">String from which the actual characters to measure are extracted.</param>
    /// <returns></returns>
    static float getSumOfCharacterOffsets(Text text, string textString)
    {
        int totalMaxWidthLength = 0;

        for (int i = 0; i < textString.Length; i++)
        {
            CharacterInfo chInfo;
            if (text.font.GetCharacterInfo(textString[i], out chInfo, text.fontSize))
            {
                totalMaxWidthLength += chInfo.advance;
            }
            else
            {
                // Tried to access invalid character!  Stick a warning here if you care about that.
            }
        }

        return totalMaxWidthLength;
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
        string foundMaxString = "";
        foreach (string s in strings)
        {
            if (s.Length > foundMaxString.Length)
            {
                foundMaxString = s;
            }
        }
        float textWidth = getTextWidth(canvas, text, foundMaxString);
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
        return buildString;
    }

    static string[] splitIntoSegments(Canvas canvas, Text text, string message, int maxTextLength = 0)
    {
        float textWidth = getTextWidth(canvas, text, message);
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
                buildString = "";
            }
            sumLength += split[i].Length + 1;
            buildString += $" {split[i]}";
        }
        strings.Add(buildString);
        return strings.ToArray();
    }
}
