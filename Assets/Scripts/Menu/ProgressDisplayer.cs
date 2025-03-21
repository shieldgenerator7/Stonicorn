using System.Linq;
using TMPro;
using UnityEngine;

public abstract class ProgressDisplayer : MonoBehaviour
{
    public string label;

    [SerializeField, HideInInspector]
    private TMP_Text txtLabel;
    [SerializeField, HideInInspector]
    private TMP_Text txtProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();
    }

    internal virtual void init()
    {
        updateLabel();
    }

    protected virtual string Label => (Shown) ? label : "???";

    protected virtual string Progress => (Shown) ? $"{CurrentCount}/{MaxCount}" : "???/???";
    public virtual float Percent => ((float)CurrentCount) / (float)MaxCount;

    protected abstract int CurrentCount { get; }
    protected abstract int MaxCount { get; }

    protected virtual bool Shown => CurrentCount > 0;

    private void updateLabel()
    {
        txtLabel.text = Label;
        txtProgress.text = Progress;
    }


    [Initializer]
    private TMP_Text init_txtLabel => GetComponentsInChildren<TMP_Text>().First(txt => txt.gameObject.name == "name");

    [Initializer]
    private TMP_Text init_txtProgress => GetComponentsInChildren<TMP_Text>().First(txt => txt.gameObject.name == "value");
}
