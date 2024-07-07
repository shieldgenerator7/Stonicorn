
using System;
using System.Runtime.CompilerServices;

[Serializable]
public class Condition : DialogueComponent
{
    public string variableName = "var1";
    public TestType testType = TestType.EQUAL;
    public int testValue = 0;

    public enum TestType
    {
        EQUAL,
        NOT_EQUAL,
        GREATER_THAN,
        GREATER_THAN_EQUAL,
        LESS_THAN,
        LESS_THAN_EQUAL,
    }

    public string TestTypeString
    {
        get
        {
            switch (testType)
            {
                case TestType.EQUAL: return "==";
                case TestType.NOT_EQUAL: return "!=";
                case TestType.GREATER_THAN: return ">";
                case TestType.GREATER_THAN_EQUAL: return ">=";
                case TestType.LESS_THAN: return "<";
                case TestType.LESS_THAN_EQUAL: return "<=";
                default: throw new ArgumentException($"testType is not valid: {testType}");
            }
        }
        set
        {
            switch (value)
            {
                case "==": testType = TestType.EQUAL; break;
                case "!=": testType = TestType.NOT_EQUAL; break;
                case ">": testType = TestType.GREATER_THAN; break;
                case ">=": testType = TestType.GREATER_THAN_EQUAL; break;
                case "<": testType = TestType.LESS_THAN; break;
                case "<=": testType = TestType.LESS_THAN_EQUAL; break;
                default: throw new ArgumentException($"value is not valid: {value}");
            }
        }
    }


    public Condition(string variableName = "var1", TestType testType = TestType.EQUAL, int testValue = 0)
    {
        this.variableName = variableName;
        this.testType = testType;
        this.testValue = testValue;
    }
}
