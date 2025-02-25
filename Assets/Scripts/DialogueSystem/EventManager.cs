using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : Manager
{
    [SerializeField]
    private float talkSpeedMultiplier = 1f;
    public float TalkSpeedMultiplier
    {
        get => talkSpeedMultiplier;
        set
        {
            talkSpeedMultiplier = value;
            updateExistingDIaloguesPostSettingChange();
        }
    }
    [SerializeField]
    private float talkWaitDuration = 1f;
    public float TalkWaitDuration
    {
        get => talkWaitDuration;
        set
        {
            talkWaitDuration = value;
            updateExistingDIaloguesPostSettingChange();
        }
    }

    public GameObject dialogueBoxPrefab;

    private DialogueBoxUpdater dialogueBox;

    private List<DialoguePlayer> dialoguePlayingList = new List<DialoguePlayer>();
    public bool DialoguePlaying => dialoguePlayingList.Count > 0;

    public override string ID => "EventManager";

    public override SettingObject Setting
    {
        get => new SettingObject(ID,
            "talkSpeedMultiplier", talkSpeedMultiplier,
            "talkWaitDuration", talkWaitDuration
            );
        set
        {
            talkSpeedMultiplier = (float)value.data["talkSpeedMultiplier"];
            talkWaitDuration = (float)value.data["talkWaitDuration"];
        }
    }

    public Action<bool> OnDialoguePlayingChanged;


    public void processEventTrigger(EventTrigger trigger)
    {
        if (!dialogueBox)
        {
            if (trigger.HasTitle)
            {
                Debug.Log($"Event: playing trigger with title: {trigger.title}");
                playDialogue(Managers.Dialogue.getDialogue(trigger.title));
            }
            else
            {
                if (trigger is DialogueTrigger)
                {
                    DialogueTrigger dialogueTrigger = ((DialogueTrigger)trigger);
                    string charStr = "";
                    dialogueTrigger.characters.ForEach(chr => charStr += $"{chr}, ");
                    Debug.Log($"Event: playing trigger with characters: {charStr}");
                    DialoguePath path = Managers.Dialogue.getDialogue(
                        dialogueTrigger.characters
                        );
                    Debug.Log($"Event: path: {path?.title ?? "[none]"}");
                    playDialogue(path);
                }
            }
        }
        else
        {
            Debug.Log($"Event: not playing trigger because dialogueBox already exists");
        }
    }

    ////TODO: move this to future EventManager
    //public void playDialogue(string title = null)
    //{
    //    //if (dialoguePlayer.Playing)
    //    //{
    //    //    return;
    //    //}
    //    DialoguePath path = null;
    //    if (String.IsNullOrEmpty(title))
    //    {
    //        //2020-09-24: TODO: make it search for characters
    //        //path = dialogueData.selectSuitableDialoguePath();

    //        //can't do anything (for now)
    //        throw new NullReferenceException($"Title must be non-null and must not be the empty string! title: {title}");
    //    }
    //    else
    //    {
    //        path = Managers.Dialogue.getDialogue(title);
    //        if (path == null)
    //        {
    //            throw new Exception($"Dialogue with this title cannot be found: {title}");
    //        }
    //    }
    //    playDialogue(path);
    //}

    ////TODO: move this to future EventManager
    //public void playDialogue(List<string> characters)
    //{
    //    //if (dialoguePlayer.Playing)
    //    //{
    //    //    return;
    //    //}
    //    DialoguePath path = Managers.Dialogue.getDialogue(characters);
    //    if (path == null)
    //    {
    //        string characterString = "";
    //        characters.ForEach(
    //            c => characterString += $"{c}, "
    //            );
    //        throw new Exception($"Dialogue with these characters cannot be found: {characterString}");
    //    }
    //    playDialogue(path);
    //}

    public void playDialogue(DialoguePath path)
    {
        if (path == null)
        {
            return;
        }
        //Setup dbu
        DialogueBoxUpdater dbu = Instantiate(dialogueBoxPrefab).GetComponent<DialogueBoxUpdater>();
        this.dialogueBox = dbu;
        Quote q = path.quotes[0];
        Character ch = Character.getCharacterByName(q.characterName);
        if (ch == null)
        {
            Debug.LogError($"Character not found: {q.characterName}! {ch}");
        }
        dbu.setSource(ch.transform);
        //Setup dp
        DialoguePlayer dp = dbu.GetComponent<DialoguePlayer>();
        dp.charsPerSecond *= talkSpeedMultiplier;
        dp.autoAdvanceDelay = talkWaitDuration;
        dp.onDialogueChanged += dbu.setText;
        dp.onDialogueAdvanced += dbu.setGoalText;
        //dp.onDialogueAdvanced += (quote) => dbu.setSource(Character.getCharacterByName(quote.characterName));
        dp.playDialogue(path);
        dp.onDialogueEnded += (path) =>
        {
            dbu.setText("");
            Destroy(dbu.gameObject);
            Managers.Dialogue.takeActions(path);
            dialoguePlayingList.Remove(dp);
            OnDialoguePlayingChanged?.Invoke(DialoguePlaying);
        };
        //
        dbu.OnSourceDestroyed += (destroyed) =>
        {
            if (destroyed)
            {
                dp.stopDialogue();
            }
        };
        //
        if (!dialoguePlayingList.Contains(dp))
        {
            dialoguePlayingList.Add(dp);
        }
        OnDialoguePlayingChanged?.Invoke(DialoguePlaying);
    }

    void updateExistingDIaloguesPostSettingChange()
    {
        FindObjectsByType<DialoguePlayer>(FindObjectsSortMode.None).ToList().ForEach(dp =>
        {
            dp.charsPerSecond *= talkSpeedMultiplier;
            dp.autoAdvanceDelay = talkWaitDuration;
        });
    }
}
