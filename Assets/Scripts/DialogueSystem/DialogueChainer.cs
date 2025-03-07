using System;
using UnityEngine;

[NonSolid]
public class DialogueChainer:MonoBehaviour, ISetupable
{
    [Tooltip("When the current dialogue stops, auto-increment this variable and trigger next dialogue")]
    public string variableName;
    public bool requirePlayerInArea = true;

    [AutoInitialize(SearchParent = true), SerializeField, HideInInspector]
    private Character character;
    [SerializeField, HideInInspector]
    private Action incrementAction;

    private bool _active = false;
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;

            Managers.Event.OnDialoguePlayingChanged -= onDialogueEnded;
            if (_active)
            {
                Managers.Event.OnDialoguePlayingChanged += onDialogueEnded;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            Active = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            Active = false;
        }
    }

    void onDialogueEnded(bool playing)
    {
        if (!playing && Active)
        {
            Managers.Progress.add(variableName);
            Managers.Event.playDialogue(character.characterName);
        }
    }

    [Initializer]
    private Action init_incrementAction
        => new Action(variableName);

    public int checkForErrors()
    {
        int errorCount = 0;

        if (string.IsNullOrWhiteSpace(variableName))
        {
            errorCount++;
        }

        return errorCount;
    }

    public int setup()
    {
        return 0;
    }
}
