using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportRangeUpdater : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 5)]
    public float suggestedSpacing = 3;//radial distance between fragments, not guaranteed

    public List<TeleportRangeEffect> effects;

    [Header("Components")]
    public GameObject fragmentPrefab;
    public Timer timer;

    private List<GameObject> fragments = new List<GameObject>();
    private float range;//cached range, gets updated in updateRange()

    // Start is called before the first frame update
    void Start()
    {
        //Timer
        timer.onTimeLeftChanged += updateEffects;
        //Register range update delegate
        Managers.Player.Teleport.onRangeChanged += updateRange;
        Managers.Player.onAbilityActivated += abilityActivated;
        updateRange(Managers.Player.Teleport.Range);
    }

    private void updateRange(float range)
    {
        clear();
        this.range = range;
        float circumference = 2 * range * Mathf.PI;
        float spacing = suggestedSpacing;// * fragmentPrefab.GetComponent<SpriteRenderer>().size.x;
        int fragmentCount = Mathf.RoundToInt(circumference / spacing);
        float angleSpacing = 2 * Mathf.PI / (float)fragmentCount;
        Vector2 placer = Vector2.up * range;
        for (int i = 0; i < fragmentCount; i++)
        {
            //Instantiate
            GameObject fragment = Instantiate(fragmentPrefab);
            //Parent it to this object
            fragment.transform.parent = transform;
            //Place the fragment in the right position and rotation
            fragment.transform.localPosition = placer;
            fragment.transform.up = fragment.transform.position - transform.position;
            //Add it to the list
            fragments.Add(fragment);
            //Update placer
            placer = Utility.RotateZ(placer, angleSpacing);
        }
        //Update effects
        updateEffects(timer.TimeLeft, timer.Duration);
    }

    private void updateEffects(float timeLeft, float duration)
    {
        effects.ForEach(fx => fx.updateEffect(fragments, timeLeft, duration));
    }

    private void abilityActivated(PlayerAbility ability, bool active)
    {
        //Update effects
        updateEffects(timer.TimeLeft, timer.Duration);
    }

    private void clear()
    {
        foreach (GameObject fragment in fragments)
        {
            Destroy(fragment);
        }
        fragments.Clear();
    }
}
