using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string characterName;

    //TODO: find better way to store and retrieve this map
    private static Dictionary<string, Character> charMap = new System.Collections.Generic.Dictionary<string, Character>();

    private void Start()
    {
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError($"Invalid character name! {characterName}", this);
            return;
        }
        if (charMap.ContainsKey(characterName) && charMap[characterName] != this)
        {
            Debug.LogError($"Character name already in map! {characterName}", this);
            return;
        }
        charMap.Add(characterName, this);
    }

    public static Character getCharacterByName(string name) => charMap[name];
}
