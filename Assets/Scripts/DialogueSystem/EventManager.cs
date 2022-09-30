using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public GameObject dialogueBoxPrefab;

    private DialoguePath currentDialoguePath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO: move this to future EventManager
    public void playDialogue(string title = null)
    {
        //if (dialoguePlayer.Playing)
        //{
        //    return;
        //}
        DialoguePath path = null;
        if (String.IsNullOrEmpty(title))
        {
            //2020-09-24: TODO: make it search for characters
            //path = dialogueData.selectSuitableDialoguePath();

            //can't do anything (for now)
            throw new NullReferenceException($"Title must be non-null and must not be the empty string! title: {title}");
        }
        else
        {
            path = Managers.Dialogue.getDialogue(title);
            if (path == null)
            {
                throw new Exception($"Dialogue with this title cannot be found: {title}");
            }
        }
        playDialogue(path);
    }

    //TODO: move this to future EventManager
    public void playDialogue(List<string> characters)
    {
        //if (dialoguePlayer.Playing)
        //{
        //    return;
        //}
        DialoguePath path = Managers.Dialogue.getDialogue(characters);
        if (path == null)
        {
            string characterString = "";
            characters.ForEach(
                c => characterString += $"{c}, "
                );
            throw new Exception($"Dialogue with these characters cannot be found: {characterString}");
        }
        playDialogue(path);
    }

    //TODO: move this to future EventManager
    public void playDialogue(DialoguePath path)
    {
        //if (dialoguePlayer.Playing)
        //{
        //    return;
        //}
        //dialoguePlayer.playDialogue(path);
    }
}
