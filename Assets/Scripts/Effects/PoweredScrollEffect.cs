using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredScrollEffect : MonoBehaviour
{
    public float scrollSpeed = 3;

    private float parentHeight;
    private float selfHeight;

    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        //get RectTransform
        rectTransform = GetComponent<RectTransform>();
        //set heights
        parentHeight = transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        selfHeight = rectTransform.sizeDelta.y;
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
        rectTransform.anchoredPosition += Vector2.up * (scrollSpeed * power / maxPower) * Time.deltaTime;
        if (rectTransform.anchoredPosition.y > selfHeight)
        {
            resetPosition();
        }
    }
}
