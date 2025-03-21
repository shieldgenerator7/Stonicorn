using System.Linq;
using TMPro;
using UnityEngine;

public abstract class ProgressDisplayer : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private TMP_Text txtProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();
    }

    protected virtual void init()
    {
        updateLabel();
    }

    protected virtual string Progress => $"{CurrentCount}/{MaxCount}";

    public float Percent => ((float)CurrentCount) / (float)MaxCount;

    protected abstract int CurrentCount { get; }
    protected abstract int MaxCount { get; }

    private void updateLabel()
    {
        txtProgress.text = Progress;
    }

    [Initializer]
    private TMP_Text init_txtProgress => GetComponentsInChildren<TMP_Text>().First(txt => txt.gameObject.name == "value");
}
