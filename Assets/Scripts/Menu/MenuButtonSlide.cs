using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonSlide : MenuButton
{
    [SerializeField]
    private Vector2 valueBounds = new Vector2(0, 100);//x is min, y is max
    public float MinValue
    {
        get { return valueBounds.x; }
        private set { valueBounds.x = value; }
    }
    public float MaxValue
    {
        get { return valueBounds.y; }
        private set { valueBounds.y = value; }
    }
    public Vector2 PointZero
    {
        get
        {
            return transform.TransformPoint(validBarBounds.points[0]);
        }
        private set { validBarBounds.points[0] = value; }
    }
    public Vector2 PointOne
    {
        get
        {
            return transform.TransformPoint(validBarBounds.points[1])
                - ((transform.TransformPoint(validBarBounds.points[1])- transform.TransformPoint(validBarBounds.points[0])).normalized * sliderBarBC2D.bounds.size.x);
        }
        private set { validBarBounds.points[1] = value; }
    }

    [SerializeField]
    private int value = 100;
    public float Value
    {
        get { return value; }
        set
        {
            this.value = (int)Mathf.Clamp(Mathf.Round(value), MinValue, MaxValue);
            //Update value it controls
            mas.valueAdjusted(this.value);
            //Update UI value text
            valueText.text = "" + this.value;
            //Update Slider Bar
            Vector3 pos = sliderBar.transform.position;
            pos = Utility.convertToRange(
                Vector2.one * this.value,
                Vector2.one * MinValue,
                Vector2.one * MaxValue,
                PointZero,
                PointOne
                );
            sliderBar.transform.position = pos;
            //Update Slider Fill
            Vector2 size = sliderFillSR.size;
            size.x = ((sliderBar.transform.position - sliderFill.transform.position).magnitude + sliderBarBC2D.bounds.size.x) / sliderFill.transform.lossyScale.x;
            sliderFillSR.size = size;
        }
    }

    public GameObject sliderFill;
    public GameObject sliderBar;
    public EdgeCollider2D validBarBounds;//the box that binds where the slider bar can be
    public Text valueText;

    private SpriteRenderer sliderFillSR;
    private BoxCollider2D sliderBarBC2D;
    private MenuActionSlide mas;

    protected override void Start()
    {
        base.Start();
        mas = GetComponent<MenuActionSlide>();
        sliderFillSR = sliderFill.GetComponent<SpriteRenderer>();
        sliderBarBC2D = sliderBar.GetComponent<BoxCollider2D>();
        //Update the value
        Value = Mathf.Clamp(mas.getCurrentValue(), MinValue, MaxValue);
    }

    public override void processTap(Vector2 tapPos)
    {
        Value = Utility.convertToRange(
                tapPos,
                PointZero,
                PointOne,
                MinValue,
                MaxValue
                );
    }
}
