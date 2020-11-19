using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Sets the indicated sprites to the right color and then destroys itself
/// </summary>
public class ColorInitializer : MonoBehaviour
{

    [System.Serializable]
    public struct ColorInitial
    {
        [Tooltip("Doesn't do anything; it's for you to tell which color is for what")]
        public string label;
        public Color color;
        [Tooltip("If not supplied, it will retrieve all the SpriteRenderers"
            + " from the object's children")]
        public List<SpriteRenderer> srs;
    }

    public List<ColorInitial> initialColors;

    // Start is called before the first frame update
    void Start()
    {
        //Get list of all SRs in and under this GameObject
        List<SpriteRenderer> childSRs = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        //Make sure all ColorInitials have an SR list
        initialColors
            .Where(ic => ic.srs.Count == 0).ToList()
            .ForEach(ic => ic.srs.AddRange(childSRs));
        //Set the colors of the SRs
        initialColors.ForEach(ic =>
            ic.srs.ForEach(sr => sr.color = ic.color)
        );
        //Destroy this script
        Destroy(this);
    }
}
