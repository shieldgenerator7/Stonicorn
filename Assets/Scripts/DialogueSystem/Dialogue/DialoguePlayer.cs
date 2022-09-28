using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays dialogue
/// </summary>
public class DialoguePlayer : MonoBehaviour
{
    [Tooltip("Determines how fast to display the quote")]
    public float charsPerSecond = 10;

    public Canvas dialogueCanvas;
    public Image charPortrait;
    public TMP_Text charName;
    public TMP_Text charQuote;
    public Image imgDiamond;

    public AudioClip endDialogueSound;

    private int index = 0;
    private DialoguePath path;

    public delegate void DialogueDelegate(DialoguePath path);
    public DialogueDelegate onDialogueStarted;
    public DialogueDelegate onDialogueEnded;

    private float revealStartTime = -1;

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
                revealStartTime = -1;
            }
            else
            {
                revealStartTime = Time.time;
            }
        }
    }

    public int RevealedCharacterCount
        => (int)((Time.time - revealStartTime) * charsPerSecond);

    public void playDialogue(DialoguePath path)
    {
        index = -1;//will be incremented in advanceDialogue()
        this.path = path;
        onDialogueStarted?.Invoke(path);
        InteractUI.instance.suppress(this);
        if (path.quotes.Count > 0)
        {
            //UI
            this.enabled = true;
            dialogueCanvas.gameObject.SetActive(true);
            //Show the first quote
            advanceDialogue();
            //Subscribe to Interact button
            PlayerController.OnPlayerInteract += advanceDialogue;
            PlayerController.OnPlayerJump += advanceDialogue;
        }
        else
        {
            stopDialogue();
        }
    }

    public void stopDialogue()
    {
        //UI
        this.enabled = false;
        dialogueCanvas.gameObject.SetActive(false);
        onDialogueEnded?.Invoke(path);
        InteractUI.instance.suppress(this, false);
        //Unsubscribe from Interact button
        PlayerController.OnPlayerInteract -= advanceDialogue;
        PlayerController.OnPlayerJump -= advanceDialogue;
        //Unset path
        this.path = null;
        //Sound
        AudioSource.PlayClipAtPoint(
            endDialogueSound,
            FindObjectOfType<PlayerController>().transform.position
            );
    }

    // Update is called once per frame
    private void Update()
    {
        if (Playing)
        {
            imgDiamond.enabled = FullyRevealed;
            displayQuoteText(CurrentQuote.text);
        }
    }

    void advanceDialogue()
    {
        //If not all characters are revealed yet,
        if (!FullyRevealed)
        {
            //Show all the characters
            FullyRevealed = true;
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
        //Continue diamond image
        imgDiamond.enabled = false;
        //Display quote
        displayQuote(CurrentQuote);
    }

    private void displayQuote(Quote quote)
    {
        charPortrait.sprite = Resources.Load<Sprite>("DialogueFaces/" + quote.imageName);
        charName.text = quote.characterName;
        displayQuoteText(quote.text);
    }
    private void displayQuoteText(string text)
    {
        charQuote.text = getRevealedString(text);
    }

    public string getRevealedString(string quoteString)
    {
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
