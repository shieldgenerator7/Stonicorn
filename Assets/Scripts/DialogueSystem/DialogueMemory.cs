using UnityEngine;

[NonSolid]
[RequireComponent (typeof(MemoryObjectInfo), typeof(VariableSetAction))]
public class DialogueMemory : MemoryMonoBehaviour
{
    [AutoInitialize,SerializeField,HideInInspector]
    private VariableSetAction variableSetAction;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isPlayerSolid())
        {
            nowDiscovered();
        }
    }

    protected override void nowDiscovered()
    {
        variableSetAction.processAllActions();
        Destroy(gameObject);
    }

    protected override void previouslyDiscovered()
    {
        Destroy(gameObject);
    }
}
