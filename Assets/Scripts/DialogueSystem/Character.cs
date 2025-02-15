using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Character : MonoBehaviour, ISetupable
{
    public string characterName;

    //TODO: find better way to store and retrieve this map
    private static Dictionary<string, List<Character>> charMap = new System.Collections.Generic.Dictionary<string, List<Character>>();

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError($"Invalid character name! {characterName}", this);
            return;
        }
        if (!charMap.ContainsKey(characterName))
        {
            charMap[characterName] = new List<Character>();
        }
        List<Character> list = charMap[characterName];
        if (!list.Contains(this))
        {
            list.Add(this);
        }
    }
    private void OnDisable()
    {
        List<Character> list = charMap[characterName];
        if (list.Contains(this))
        {
            list.Remove(this);
        }
    }

    public static Character getCharacterByName(string name) => charMap[name]
        //TODO: find closest character, even if not in range
        .Find(chr => Vector2.Distance(
            chr.transform.position,
            Managers.Player.transform.position
            ) <= 10);//dirty: hard coded range

    public int checkForErrors()
    {
        int problemCount = 0;
        DialogueTrigger dt = GetComponent<DialogueTrigger>() ?? GetComponentInChildren<DialogueTrigger>();
        if (!dt)
        {
            Debug.LogError($"Character {gameObject.Name()} needs a DialogueTrigger!", this);
            problemCount++;
        }
        return problemCount;
    }

    public int setup()
    {
        int changedCount = 0;
        Character chr = this;
        DialogueTrigger dt = chr.GetComponent<DialogueTrigger>() ?? chr.GetComponentInChildren<DialogueTrigger>();
        if (!dt.characters.Contains(chr.characterName))
        {
            dt.characters.Add(chr.characterName);
            Debug.LogWarning($"Character {chr.gameObject.Name()} now has dialogue trigger set up!", chr);
            changedCount++;
        }
        return changedCount;
    }
}
