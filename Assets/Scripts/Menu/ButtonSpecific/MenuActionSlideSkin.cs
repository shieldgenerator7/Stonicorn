using System.Linq;
using UnityEngine;

public class MenuActionSlideSkin : MenuActionSlide
{
    public GameObject mannequin;

    private GameObject currentSkin;

    public override float getCurrentValue()
    {
        return Managers.Skin.SkinIndex;
    }

    public override void valueAdjusted(float value)
    {
        Managers.Skin.SkinIndex = (int)value;
        if (currentSkin)
        {
            Destroy(currentSkin);
        }
        currentSkin = Instantiate(Managers.Skin.Skin.gameObject);
        currentSkin.transform.SetParent(mannequin.transform, false);
        int layerID = mannequin.GetComponent<SpriteRenderer>().sortingLayerID;
        currentSkin.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(sr =>
        {
            sr.sortingLayerID = layerID;
        });
    }

    public override float getOverriddenMaxValue(float currentMaxValue)
    {
        return Managers.Skin.FoundSkinCount-1;
    }

    public override string getValueLabel(float currentValue)
    {
        return Managers.Skin.getSkin((int)currentValue).name;
    }
}
