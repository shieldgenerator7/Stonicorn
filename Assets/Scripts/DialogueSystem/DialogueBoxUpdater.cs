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
        txtDialogue.text = text;
        txtDialogue.ForceMeshUpdate();
        updateSize();
        updatePosition();
    }

    public void setSource(Transform source)
    {
        this.source = source;
        updatePosition();
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
        transform.rotation = rotation;
        //position relative to source
        Vector2 position = source.position + Camera.main.transform.up.normalized * offsetLength;
        transform.position = position;
        quoteBoxTail.transform.position = quoteBox.transform.position - (quoteBox.transform.up * quoteSR.size.y / 2);
        //position talk effect
        talkEffect.transform.position = source.position;
    }

    private void updateSize()
    {
        Vector2 textBoxSize = getTextSize();
        quoteSR.size = textBoxSize;
        quoteBoxTail.transform.position = quoteSR.transform.position - (quoteBox.transform.up * quoteSR.size.y / 2);
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
}
