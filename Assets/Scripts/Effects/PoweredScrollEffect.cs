using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredScrollEffect : MonoBehaviour, ISetupable
{
    public float scrollSpeed = 3;

    [SerializeField,HideInInspector]
    private float parentHeight;
    [SerializeField, HideInInspector]
    private float selfHeight;

    [AutoInitialize, SerializeField, HideInInspector]
    private RectTransform rectTransform;

    [AutoInitialize(SearchParent =true), SerializeReference]
    public IPowerConduit conduit;

    // Start is called before the first frame update
    void Start()
    {
        //register delegate
        conduit.OnPowerFlowed += onPowerFlowed;
        //set position
        resetPosition();
    }

    private void resetPosition()
    {
        rectTransform.anchoredPosition = Vector2.down * parentHeight;
    }

    void onPowerFlowed(float power, float maxPower)
    {
        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        if (rectTransform.anchoredPosition.y > selfHeight)
        {
            resetPosition();
        }
    }

    public int setup()
    {
        int changeCount = 0;
        //set heights
        if (parentHeight == 0)
        {
            parentHeight = transform.parent.GetComponent<RectTransform>().sizeDelta.y;
            changeCount++;            
        }
        if (selfHeight == 0)
        {
            selfHeight = rectTransform.sizeDelta.y;
            changeCount++;
        }
        return changeCount;
    }
}
