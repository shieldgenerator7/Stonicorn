
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DialoguePath
{
    public string title = "Dialogue Title";
    public List<Condition> conditions = new List<Condition>();
    public List<Quote> quotes = new List<Quote>();
    public List<Action> actions = new List<Action>();

    /// <summary>
    /// Restores temp variables after being read in
    /// </summary>
    public void inflate()
    {
        conditions.ForEach(
            c => c.path = this
            );
        quotes.ForEach(
            q => q.path = this
            );
        actions.ForEach(
            a => a.path = this
            );
        //set cached variables
        Characters = quotes.Select(q => q.characterName).Distinct().ToList();
    }

    /// <summary>
    /// Returns a list of the characters in this dialogue path
    /// </summary>
    public List<string> Characters { get; private set; }

    /// <summary>
    /// Returns true if all the required characters are in this dialogue path.
    /// Allows for extra characters not mentioned
    /// </summary>
    /// <param name="requiredCharacters"></param>
    /// <returns></returns>
    public bool allCharactersPresent(List<string> requiredCharacters)

    public void remove(DialogueComponent dc)
    {
        if (dc is Condition)
        {
            conditions.Remove((Condition)dc);
        }
        else if (dc is Quote)
        {
            quotes.Remove((Quote)dc);
        }
        else if (dc is Action)
        {
            actions.Remove((Action)dc);
        }
    }
        => requiredCharacters.All(rchar => Characters.Contains(rchar));
}
