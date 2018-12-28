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
                );
            sliderBar.transform.position = pos;
            valueText.text = ""+Mathf.Floor(this.value);
        }
    }

    public GameObject sliderFill;
    public GameObject sliderBar;
    public BoxCollider2D validBarBounds;//the box that binds where the slider bar can be
    public Text valueText; 

    protected override void Start()
    {
        base.Start();
        if (value != 0)
        {
            value = valueBounds.y;
        }
        value = Mathf.Clamp(value, valueBounds.x, valueBounds.y);
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
