using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Plays dialogue
/// </summary>
public class DialoguePlayer : MonoBehaviour
{
    [Tooltip("Determines how fast to display the quote")]
    public float charsPerSecond = 10;
    [Tooltip(
        "How many seconds should it wait before it auto-advances after the current quote finishes? " +
        "-1 to disable."
        )]
    public float autoAdvanceDelay = 1;

    private int index = 0;
    private DialoguePath path;

    public delegate void DialoguePathDelegate(DialoguePath path);
    public DialoguePathDelegate onDialogueStarted;
    public DialoguePathDelegate onDialogueEnded;

    public delegate void DialogueDelegate(string dialogue);
    public event DialogueDelegate onDialogueChanged;
    public event DialogueDelegate onDialogueAdvanced;

    private float revealStartTime = -1;
    private string revealedString = "";

    public bool Playing => path != null && index >= 0;
    public Quote CurrentQuote
        => (path != null && index >= 0) ? path.quotes[index] : null;
    public bool FullyRevealed
    {
        get =>
            //yes, it is "fully revealed" if there is no selected quote yet
            index < 0
            //but also if all characters should be shown
            || RevealedCharacterCount >= CurrentQuote.text.Length;
        set
        {
            if (value)
            {
                //Fully reveal
                revealStartTime = -1;
            }
            else
            {
                //Fully hide
                revealStartTime = Time.time;
            }
        }
    }

    public int RevealedCharacterCount
        => (int)((Time.time - revealStartTime) * charsPerSecond);

    /// <summary>
    /// Amount of time elapsed since it has been fully revealed. Returns a negative number if it's not fully revealed
    /// </summary>
    public float RevealedTime
        => Time.time - (revealStartTime + (CurrentQuote.text.Length / charsPerSecond));

    public void playDialogue(DialoguePath path)
    {
        //error checking
        if (!(path?.quotes.Count > 0))
        {
            Debug.LogError(
                $"Cannot call playDialogue() with null path! path: {path}. " +
                $"Call stopDialogue() instead",
                this
                );
            return;
        }
        //
        index = -1;//will be incremented in advanceDialogue()
        this.path = path;
        //OnStart delegate
        onDialogueStarted?.Invoke(path);
        //UI
        this.enabled = true;
        //Show the first quote
        advanceDialogue();
    }

    public void stopDialogue()
    {
        //UI
        this.enabled = false;
        //OnStop delegate
        onDialogueEnded?.Invoke(path);
        //Unset path
        this.path = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Playing)
        {
            string prevRevealedString = revealedString;
            revealedString = RevealedString;
            if (prevRevealedString != revealedString)
            {
                onDialogueChanged?.Invoke(revealedString);
            }
            //Debug.Log($"RevealedTime: {RevealedTime}");
            else if (autoAdvanceDelay >= 0 && RevealedTime >= autoAdvanceDelay)
            {
                advanceDialogue();
            }
        }
    }

    void advanceDialogue()
    {
        //If not all characters are revealed yet,
        if (!FullyRevealed)
        {
            //Show all the characters
            FullyRevealed = true;
            //Dialogue delegates
            onDialogueChanged?.Invoke(RevealedString);
            //Consume event
            return;
        }
        index++;
        if (index >= path.quotes.Count)
        {
            stopDialogue();
            return;
        }
        //Reset timer
        revealStartTime = Time.time;
        //Dialogue delegates
        onDialogueAdvanced?.Invoke(CurrentQuote.text);
        onDialogueChanged?.Invoke(RevealedString);
    }

    public string RevealedString
    {
        get
        {
            string quoteString = CurrentQuote.text;
            int charCount = RevealedCharacterCount;
            if (charCount >= quoteString.Length)
            {
                return quoteString;
            }
            string builtString = "";
            bool inTag = false;
            int length = 0;
            for (int i = 0; i < quoteString.Length; i++)
            {
                builtString += quoteString[i];
                if (inTag)
                {
                    if (quoteString[i] == '>')
                    {
                        inTag = false;
                    }
                }
                else
                {
                    length++;
                    if (quoteString[i] == '<')
                    {
                        inTag = true;
                    }
                }
                //If we got enough characters,
                if (length >= charCount)
                {
                    //we got our revealed string
                    break;
                }
            }
            return builtString;
        }
    }

}
