
using System;
using System.Runtime.CompilerServices;

[Serializable]
public class Action : DialogueComponent
{
    public string variableName = "var1";
    public ActionType actionType = ActionType.ADD;
    public int actionValue = 1;

    public enum ActionType
    {
        SET,
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE
    }

    public string ActionTypeString
    {
        get
        {
            switch (actionType)
            {
                case ActionType.SET: return "=";
                case ActionType.ADD: return "+=";
                case ActionType.SUBTRACT: return "-=";
                case ActionType.MULTIPLY: return "*=";
                case ActionType.DIVIDE: return "/=";
                default: throw new ArgumentException("actionType is not valid: " + actionType);
            }
        }
        set
        {
            switch (value)
            {
                case "=": actionType = ActionType.SET; break;
                case "+=": actionType = ActionType.ADD; break;
                case "-=": actionType = ActionType.SUBTRACT; break;
                case "*=": actionType = ActionType.MULTIPLY; break;
                case "/=": actionType = ActionType.DIVIDE; break;
                default: throw new ArgumentException("value is not valid: " + value);
            }
        }
    }


    public Action(string variableName = "var1", ActionType actionType = ActionType.ADD, int actionValue = 1)
    {
        this.variableName = variableName;
        this.actionType = actionType;
        this.actionValue = actionValue;
    }
}
