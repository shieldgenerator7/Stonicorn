using System.Collections.Generic;
using UnityEngine;

public class DialogueVoiceLinePlayer : MonoBehaviour
{
    public string voiceLineFolder = "Dialogue";
    private List<AudioClip> clipList;
    private AudioSource audioSource;
    private DialoguePath dialoguePath;
    private int currentIndex = 0;

    public void init(DialoguePath dialogue, AudioSource audioSource)
    {
        this.dialoguePath = dialogue;
        clipList = dialogue.quotes.ConvertAll(quote =>
             (!string.IsNullOrWhiteSpace(quote.voiceLineFileName))
                ? Resources.Load<AudioClip>($"{voiceLineFolder}/{quote.voiceLineFileName}")
                : null
        );
        this.audioSource = audioSource;
    }

    public void playVoiceLine(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, clipList.Count-1);
        audioSource.Stop();
        audioSource.clip = clipList[currentIndex];
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogError($"Missing voice line! {dialoguePath.title}:{currentIndex}", this);
        }
    }

    public float Duration => clipList[currentIndex]?.length ?? 0;
}
