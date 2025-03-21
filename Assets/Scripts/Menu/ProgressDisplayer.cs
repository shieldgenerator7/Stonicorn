using TMPro;
using UnityEngine;

public abstract class ProgressDisplayer : MonoBehaviour
{
    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private TMP_Text txtProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
}
