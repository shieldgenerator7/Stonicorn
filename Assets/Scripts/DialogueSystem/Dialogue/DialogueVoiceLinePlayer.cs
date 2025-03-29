using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueVoiceLinePlayer : MonoBehaviour
{
    public string voiceLineFolder = "Dialogue";
    private List<AudioClip> clipList;
    private AudioSource audioSource;
    private DialoguePath dialoguePath;
    private int currentIndex = 0;

    private float volume = 1;
    public float Volume
    {
        set
        {
            volume = Mathf.Clamp(value,0,1);
            audioSource.volume = volume;
        }
    }

    public void init(DialoguePath dialogue, AudioSource audioSource)
    {
#if UNITY_EDITOR
        string quoteFileNames = String.Join(
            ", ",
            dialogue.quotes
                .ConvertAll(quote => quote.voiceLineFileName)
                .FindAll(filename => !String.IsNullOrWhiteSpace(filename))
                .ConvertAll(filename => $"{voiceLineFolder}/{filename}")
            );
        //    Dialogue/Transistor/transistor_welcomemerky.mp3
        Debug.Log($"Loading dialogue path {dialogue.title}: voicelines: {quoteFileNames}");
        int badEndingCount = dialogue.quotes.Count(quote => 
            quote.voiceLineFileName.EndsWith(".mp3") ||
            quote.voiceLineFileName.EndsWith(".wav") 
        );
        if (badEndingCount > 0)
        {
            Debug.LogError($"DIALOGUE WILL NOT WORK BECAUSE it lists filenames with their extensions. Remove the filetype extension and it should work. Dialogue: {dialogue.title}");
        }
#endif
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
#if UNITY_EDITOR
        else
        {
            Debug.LogError(
                $"Missing voice line! {dialoguePath.title}: {currentIndex} - {dialoguePath.quotes[index].characterName}",
                this
                );
        }
#endif
    }

    internal void stop()
    {
        audioSource?.Stop();
    }

    public float Duration => clipList[currentIndex]?.length ?? 0;
}
