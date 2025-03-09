using System;
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
    [Tooltip("The minimum amount of width the dialogue box can have")]
    public float minWidth = 1;
    [Tooltip("THe distance between the source position and the box position")]
    public float offsetLength = 3;
    [Tooltip("The scale point at which the NPC quote box should be full screen")]
    public CameraController.CameraScalePoints baseCameraScalePoint;

    [Tooltip("When the player is this far or greater from the current speaking character, the dialogue is canceled without updating variables")]
    private float dialogueCancelDistance = 7;

    [Header("Components")]
    public ParticleSystem talkEffect;//the particle system for the visual part of NPC talking
    public TMP_Text txtDialogue;
    public TMP_Text txtDialogueGoal;
    public Canvas canvas;
    public GameObject quoteBox;
    public GameObject quoteBoxTail;


    private string text = "";
    private string goalText = "";
    [AutoInitialize(Container = "quoteBox"), SerializeField,HideInInspector]
    private SpriteRenderer quoteSR;

    private Transform source;


    // Update is called once per frame
    void Update()
    {
        //check source still exists
        if (!source || ReferenceEquals(source, null))
        {
            //if not, destroy this
            Destroy(gameObject);
            OnSourceDestroyed?.Invoke(true);
            return;
        }
        ////resize dialogue box
        //resizeToCamera();
        //update position
        updatePosition();

        //Check player distance
        if (Vector2.Distance(Managers.Player.transform.position, source.transform.position) >= dialogueCancelDistance)
        {
            Destroy(gameObject);
            OnSourceDestroyed?.Invoke(true);
        }
    }
    public event Action<bool> OnSourceDestroyed;//only have bool here because it wont compile without at least 1

    public void setText(string value)
    {
        text = value;
        txtDialogue.text = text + Utility.repeatCharacter(' ', goalText.Length - text.Length);
        txtDialogue.ForceMeshUpdate();
    }

    public void setGoalText(string value)
    {
        goalText = value;
        txtDialogueGoal.text = goalText;
        txtDialogueGoal.ForceMeshUpdate();

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
        txtDialogue.rectTransform.sizeDelta = textBoxSize * 100;
        quoteSR.size = textBoxSize;
        quoteBoxTail.transform.position = quoteSR.transform.position - (quoteBox.transform.up * quoteSR.size.y / 2);
    }

    private Vector2 getTextSize(bool usePadding = true)
    {
        //assumes canvas scale x and y are the same
        Vector2 size = txtDialogueGoal.GetRenderedValues(true) * canvas.transform.localScale.x;
        if (size.x < minWidth)
        {
            size.x = minWidth;
        }
        if (usePadding)
        {
            size += Vector2.one * padding;
        }
        return size;
    }
}
