using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public GameObject dialogueBoxPrefab;

    private DialogueBoxUpdater dialogueBox;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void processEventTrigger(EventTrigger trigger)
    {
        if (!dialogueBox)
        {
            if (trigger.HasTitle)
            {
                playDialogue(Managers.Dialogue.getDialogue(trigger.title));
            }
            else
            {
                if (trigger is DialogueTrigger)
                {
                    playDialogue(Managers.Dialogue.getDialogue(
                        ((DialogueTrigger)trigger).characters
                        ));
                }
            }
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
        //Setup dbu
        DialogueBoxUpdater dbu = Instantiate(dialogueBoxPrefab).GetComponent<DialogueBoxUpdater>();
        dbu.Start();
        this.dialogueBox = dbu;
        Quote q = path.quotes[0];
        Character ch = Character.getCharacterByName(q.characterName);
        dbu.setSource(ch.transform);
        //Setup dp
        DialoguePlayer dp = dbu.GetComponent<DialoguePlayer>();
        dp.onDialogueChanged += dbu.setText;
        //dp.onDialogueAdvanced += (quote) => dbu.setSource(Character.getCharacterByName(quote.characterName));
        dp.playDialogue(path);
        dp.onDialogueEnded += (path) =>
        {
            Destroy(dbu.gameObject);
        };
    }
}
