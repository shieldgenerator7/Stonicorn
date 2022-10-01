using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxUpdater : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The amount of extra space around the letters inside the box")]
    public float padding = 1;
    [Tooltip("The scale point at which the NPC quote box should be full screen")]
    public float offsetLength = 3;
    public CameraController.CameraScalePoints baseCameraScalePoint;

    [Header("Components")]
    public ParticleSystem talkEffect;//the particle system for the visual part of NPC talking
    public TMP_Text txtDialogue;
    public Canvas canvas;
    public GameObject quoteBox;
    public GameObject quoteBoxTail;

    SpriteRenderer quoteSR;

    private string rawText;
    private List<string> textLines;
    [SerializeField]
    private Transform source;

    // Start is called before the first frame update
    void Start()
    {
        txtDialogue.fontSize = (int)(Camera.main.pixelHeight * 0.05f);
        quoteSR = quoteBox.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ////resize dialogue box
        //resizeToCamera();
        //update position
        updatePosition();
    }

    public void setText(string text)
    {
#if UNITY_EDITOR
        Start();
#endif
        this.rawText = text;
        
        //txtDialogue.text = text;
        //Vector2 messageDimensions = getMessageDimensions(canvas, txtDialogue, text);
        //int maxTextLength = getTextLength(canvas, txtDialogue, messageDimensions.x);
        //text = processMessage(canvas, txtDialogue, text, maxTextLength);
        txtDialogue.text = text;
        txtDialogue.ForceMeshUpdate();
        updateSize();
        updatePosition();
    }

    public void setSource(Transform source)
    {
        this.source = source;
    }

    public bool Active
    {
        get => gameObject.activeSelf;
        set
        {
            gameObject.SetActive(value);
            canvas.gameObject.SetActive(value);
            quoteBox.SetActive(value);
            enabled = value;
            //Show speaking particles
            if (value && !talkEffect.isPlaying)
            {
                talkEffect.Play();
            }
            else if (!value && talkEffect.isPlaying)
            {
                talkEffect.Stop();
            }
        }
    }

    private void updatePosition()
    {
        //rotate dialogue box
        Quaternion rotation = Camera.main.transform.rotation;
        canvas.transform.rotation = rotation;
        quoteBox.transform.rotation = rotation;
        //position relative to source
        Vector2 position = source.position + Camera.main.transform.up.normalized * offsetLength;
        canvas.transform.position = position;
        quoteBox.transform.position = position;
        quoteBoxTail.transform.position = position - (Vector2)(quoteBox.transform.up * quoteSR.size.y / 2);
        talkEffect.transform.position = source.position;
    }

    private void updateSize()
    {
        //string text = this.rawText;
        ////size textbox
        //float textHeight = getTextHeight(canvas, txtDialogue);
        //float buffer = textHeight / 2;
        //int maxTextLength = getMaxTextLength(canvas, txtDialogue);
        //Vector2 messageDimensions = getMessageDimensions(canvas, txtDialogue, text);
        //maxTextLength = getTextLength(canvas, txtDialogue, messageDimensions.x);
        //if (messageDimensions.y > textHeight)
        //{
        //    float textWidth = getTextWidth(canvas, txtDialogue, text);
        //    int lineCount = Mathf.CeilToInt(textWidth / getMaxWidth(canvas, txtDialogue));
        //    maxTextLength = (maxTextLength + (text.Length / lineCount)) / 2;
        //    messageDimensions = getMessageDimensions(canvas, txtDialogue, text, maxTextLength);
        //    maxTextLength = getTextLength(canvas, txtDialogue, messageDimensions.x);
        //}
        Vector2 textBoxSize = getTextSize();
        quoteSR.size = textBoxSize;
        quoteBoxTail.transform.position = quoteSR.transform.position - (quoteBox.transform.up * quoteSR.size.y / 2);
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

    static float getMaxWidth(Canvas canvas, TMP_Text text)
    {
        return Mathf.Min(Screen.width / 2, text.rectTransform.rect.width * canvas.transform.localScale.x);
    }

    static int getTextLength(Canvas canvas, TMP_Text text, float width)
    {
        return Mathf.FloorToInt(width / (text.fontSize * 0.5f * canvas.transform.localScale.x));
    }
    static int getMaxTextLength(Canvas canvas, TMP_Text text)
    {
        return getTextLength(canvas, text, getMaxWidth(canvas, text));
    }

    private Vector2 getTextSize(bool usePadding = true)
    {
        //assumes canvas scale x and y are the same
        Vector2 size = txtDialogue.GetRenderedValues(true) * canvas.transform.localScale.x;
        if (usePadding)
        {
            size += Vector2.one * padding;
        }
        return size;
    }

    //static string processMessage(Canvas canvas, TMP_Text text, string message, int maxTextLength)
    //{
    //    List<string> strings = splitIntoSegments(canvas, text, message, maxTextLength);
    //    string buildString = "";
    //    foreach (string str in strings)
    //    {
    //        buildString += $"{str}\n";
    //    }
    //    buildString.Trim();
    //    return buildString;
    //}

    //static List<string> splitIntoSegments(Canvas canvas, TMP_Text text, int maxTextLength = 0)
    //{
    //    float textWidth = getTextSize(canvas, text).x;
    //    float maxWidth = getMaxWidth(canvas, text);
    //    int segmentLength = message.Length;
    //    if (textWidth > maxWidth)
    //    {
    //        if (maxTextLength == 0)
    //        {
    //            maxTextLength = getMaxTextLength(canvas, text);
    //        }
    //        segmentLength = maxTextLength;
    //    }
    //    return splitIntoSegments(message, segmentLength);
    //}
    static List<string> splitIntoSegments(string message, int maxLength)
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
        return strings;
    }
}
