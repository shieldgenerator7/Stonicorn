using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Character : MonoBehaviour
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
}
