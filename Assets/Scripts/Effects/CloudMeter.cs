using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMeter : MonoBehaviour
{
    public GameObject cloudPrefab;
    public Sprite availableSprite;
    public Sprite usedSprite;

    private AirSliceAbility airSliceAbility;

    private List<SpriteRenderer> clouds = new List<SpriteRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        Managers.Player.onAbilityActivated += abilityEnableChanged;
        airSliceAbility = Managers.Player.GetComponent<AirSliceAbility>();
        abilityEnableChanged(airSliceAbility, airSliceAbility.enabled);
    }

    private void abilityEnableChanged(PlayerAbility ability, bool active)
    {
        if (ability == airSliceAbility)
        {
            if (active)
            {
                Managers.Player.onGroundedStateUpdated += groundStateChanged;
                airSliceAbility.onAirPortsUsedChanged += airPortsUsedChanged;
            }
            else
            {
                Managers.Player.onGroundedStateUpdated -= groundStateChanged;
                airSliceAbility.onAirPortsUsedChanged -= airPortsUsedChanged;
            }
        }
    }

    private void groundStateChanged(bool grounded, bool groundedNormal)
    {
        //Show when in the air
        showClouds(!groundedNormal);
    }

    private void airPortsUsedChanged(int airPortsUsed, int maxAirPorts)
    {
        arrangeClouds(maxAirPorts - airPortsUsed, maxAirPorts);
        if (airPortsUsed > 0)
        {
            showClouds(true);
        }
    }

    private void showClouds(bool show)
    {
        clouds.ForEach(sr => sr.gameObject.SetActive(show));
    }

    private void arrangeClouds(int available, int max)
    {
        //Assure the right amount of clouds
        while (clouds.Count < max)
        {
            clouds.Add(createCloud());
        }
        while (clouds.Count > max)
        {
            Destroy(clouds[0]);
            clouds.RemoveAt(0);
        }
        //Process clouds
        for (int i = 0; i < max; i++)
        {
            SpriteRenderer sr = clouds[i];
            sr.sprite = (i < available) ? availableSprite : usedSprite;
            float percent = (max - 1 > 0)
                ? ((float)i / (max - 1)) * 2 - 1
                : 0;
            percent *= Mathf.PI / 2;
            sr.transform.localPosition = new Vector2(
                Mathf.Sin(percent),
                -Mathf.Cos(percent)
                );
        }
    }

    private SpriteRenderer createCloud()
    {
        GameObject cloud = Instantiate(cloudPrefab);
        cloud.transform.parent = transform;
        cloud.transform.up = Managers.Camera.transform.up;
        SpriteRenderer sr = cloud.GetComponent<SpriteRenderer>();
        return sr;
    }
}
