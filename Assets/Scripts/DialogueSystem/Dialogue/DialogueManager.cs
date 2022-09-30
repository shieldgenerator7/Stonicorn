using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Determines which dialogue happens when you want to trigger a dialogue
/// </summary>
[RequireComponent(typeof(DialoguePlayer))]
public class DialogueManager : MonoBehaviour
{
    private DialogueData dialogueData;
    [SerializeField]
    private DialoguePlayer dialoguePlayer;


    private void Awake()
    {
        string jsonString = Resources.Load<TextAsset>("dialogues").text;
        dialogueData = JsonUtility.FromJson<DialogueData>(jsonString);
        dialogueData.dialogues.ForEach(d => d.inflate());
        dialoguePlayer.onDialogueEnded += takeActions;
        //OnStartCheckVariable
        SceneManager.sceneLoaded +=
            (s, m) =>
            {
                foreach (OnStartCheckVariable oscv in Resources.FindObjectsOfTypeAll(typeof(OnStartCheckVariable)))
                {
                    oscv.checkTakeAction(Managers.Progress);
                }
            };
    }

    public bool hasDialogue(string title)
    {
        if (String.IsNullOrEmpty(title))
        {
            throw new NullReferenceException($"Title cannot be empty! title: {title}");
        }
        return dialogueData.getDialoguePath(title) != null;
    }

    public bool hasDialogue(List<string> characters)
    {
        DialoguePath path = dialogueData.getDialoguePaths(characters)
            .FirstOrDefault(dp => conditionsMet(dp));
        return path != null;
    }

    public void playDialogue(string title = null)
    {
        if (dialoguePlayer.Playing)
        {
            return;
        }
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
            path = dialogueData.getDialoguePath(title);
            if (path == null)
            {
                throw new Exception($"Dialogue with this title cannot be found: {title}");
            }
        }
        playDialogue(path);
    }

    public void playDialogue(List<string> characters)
    {
        if (dialoguePlayer.Playing)
        {
            return;
        }
        DialoguePath path = dialogueData.getDialoguePaths(characters)
            .FirstOrDefault(dp => conditionsMet(dp));
        if (path == null)
        {
            string characterString = "";
            characters.ForEach(
                c => characterString += $"{c}, ");
            throw new Exception($"Dialogue with these characters cannot be found: {characterString}");
        }
        playDialogue(path);
    }

    public void playDialogue(DialoguePath path)
    {
        if (dialoguePlayer.Playing)
        {
            return;
        }
        dialoguePlayer.playDialogue(path);
    }

    private bool conditionsMet(DialoguePath path)
    {
        return path.conditions.All(c => conditionMet(c));
    }

    private bool conditionMet(Condition c)
    {
        int value = Managers.Progress.get(c.variableName);
        switch (c.testType)
        {
            case Condition.TestType.EQUAL: return value == c.testValue;
            case Condition.TestType.NOT_EQUAL: return value != c.testValue;
            case Condition.TestType.GREATER_THAN: return value > c.testValue;
            case Condition.TestType.GREATER_THAN_EQUAL: return value >= c.testValue;
            case Condition.TestType.LESS_THAN: return value < c.testValue;
            case Condition.TestType.LESS_THAN_EQUAL: return value <= c.testValue;
            default: throw new ArgumentException($"condition testType is not valid: {c.testType}");
        }
    }

    private void takeActions(DialoguePath path)
    {
        path.actions.ForEach(a => takeAction(a));
        InteractUI.instance.refreshTriggerList();
    }

    private void takeAction(Action a)
    {
        switch (a.actionType)
        {
            case Action.ActionType.SET: Managers.Progress.set(a.variableName, a.actionValue); break;
            case Action.ActionType.ADD: Managers.Progress.add(a.variableName, a.actionValue); break;
            case Action.ActionType.SUBTRACT: Managers.Progress.add(a.variableName, -a.actionValue); break;
            case Action.ActionType.MULTIPLY: Managers.Progress.multiply(a.variableName, a.actionValue); break;
            case Action.ActionType.DIVIDE: Managers.Progress.multiply(a.variableName, 1 / a.actionValue); break;
            default: throw new ArgumentException($"Action testType is not valid: {a.actionType}");
        }
    }
}
