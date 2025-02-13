using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredScrollEffect : MonoBehaviour
{
    public float scrollSpeed = 3;

    [SerializeField,HideInInspector]
    private float parentHeight;
    [SerializeField, HideInInspector]
    private float selfHeight;

    [AutoInitialize, SerializeField, HideInInspector]
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        //register delegate
        IPowerConduit conduit = GetComponent<IPowerConduit>();
        if (conduit == null)
        {
            conduit = GetComponentInParent<IPowerConduit>();
        }
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


    [Initializer("parentHeight")]
    private float initParentHeight()
        => transform.parent.GetComponent<RectTransform>().sizeDelta.y;

    [Initializer("selfHeight")]
    private float initSelfHeight()
       => rectTransform.sizeDelta.y;
}
