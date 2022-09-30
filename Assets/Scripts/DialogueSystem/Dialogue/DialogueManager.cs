using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Determines which dialogue happens when you want to trigger a dialogue
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Settings")]
    public string dialogueResourceName = "dialogues";

    private DialogueData dialogueData;

    private void Awake()
    {
        string jsonString = Resources.Load<TextAsset>(dialogueResourceName).text;
        dialogueData = JsonUtility.FromJson<DialogueData>(jsonString);
        dialogueData.dialogues.ForEach(d => d.inflate());
        //dialoguePlayer.onDialogueEnded += takeActions;
        //TODO: move to GameManager
        //OnStartCheckVariable
        Managers.Scene.onSceneObjectsLoaded +=
            (sceneGOs, foreignGOs, lastStateSeen) =>
            {
                //TODO: use list of scene gos instead of searching with resources
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

    public DialoguePath getDialogue(string title)
        => dialogueData.getDialoguePath(title);

    public List<DialoguePath> getDialogues(List<string> characters)
        => dialogueData.getDialoguePaths(characters);

    public DialoguePath getDialogue(List<string> characters)
        => getDialogues(characters).FirstOrDefault(dp => conditionsMet(dp));

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
