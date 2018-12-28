using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonSlide : MenuButton
{
    public Vector2 valueBounds = new Vector2(0, 100);//x is min, y is max
    [SerializeField]
    private float value = 100;
    public float Value
    {
        get { return value; }
        set
        {
            this.value = Mathf.Clamp(value, valueBounds.x, valueBounds.y);
            Vector3 pos = sliderBar.transform.position;
            pos.x = Utility.convertToRange(
                this.value,
                valueBounds.x,
                valueBounds.y,
                validBarBounds.bounds.min.x,
                validBarBounds.bounds.max.x
                )
                - sliderBarBC2D.bounds.size.x;
            sliderBar.transform.position = pos;
            valueText.text = ""+Mathf.Floor(this.value);
            Vector2 size = sliderFillSR.size;
            size.x = (sliderBarBC2D.bounds.max.x - sliderFill.transform.position.x)/sliderFill.transform.lossyScale.x;
            sliderFillSR.size = size;
        }
    }

    public GameObject sliderFill;
    public GameObject sliderBar;
    public BoxCollider2D validBarBounds;//the box that binds where the slider bar can be
    public Text valueText;

    private SpriteRenderer sliderFillSR;
    private BoxCollider2D sliderBarBC2D;

    protected override void Start()
    {
        base.Start();
        sliderFillSR = sliderFill.GetComponent<SpriteRenderer>();
        sliderBarBC2D = sliderBar.GetComponent<BoxCollider2D>();
        if (value != 0)
        {
            value = valueBounds.y;
        }
        value = Mathf.Clamp(value, valueBounds.x, valueBounds.y);
        Value = value;
    }

    public override void processTap(Vector2 tapPos)
    {
        Debug.Log("MenuButtonSlide " + name + " adjusted.");
        Value = Utility.convertToRange(
                tapPos.x,
                validBarBounds.bounds.min.x,
                validBarBounds.bounds.max.x,
                valueBounds.x,
                valueBounds.y,
                true//for clamping the value
                );
    }
}
