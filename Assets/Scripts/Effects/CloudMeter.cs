using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMeter : MonoBehaviour
{
    public AirSliceAbility airSliceAbility;
    public GameObject cloudPrefab;
    public Sprite availableSprite;
    public Sprite usedSprite;

    private List<SpriteRenderer> clouds = new List<SpriteRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        Managers.Player.onAbilityActivated += abilityEnableChanged;
        airSliceAbility.onAirPortsUsedChanged += airPortsUsedChanged;
    }

    private void Update()
    {
        transform.position = airSliceAbility.transform.position;
        transform.up = Managers.Camera.Up;
    }

    private void abilityEnableChanged(PlayerAbility ability, bool active)
    {
        if (ability == airSliceAbility)
        {
            if (active)
            {
                Managers.Player.onGroundedStateUpdated += groundStateChanged;
            }
            else
            {
                Managers.Player.onGroundedStateUpdated -= groundStateChanged;
            }
        }
    }

    private void groundStateChanged(bool grounded, bool groundedNormal)
    {
        //Show when in the air
        gameObject.SetActive(!groundedNormal);
    }

    private void airPortsUsedChanged(int airPortsUsed, int maxAirPorts)
    {
        arrangeClouds(maxAirPorts - airPortsUsed, maxAirPorts);
        if (airPortsUsed > 0)
        {
            gameObject.SetActive(true);
        }
    }

    private void arrangeClouds(int available, int max)
    {
        while (clouds.Count < max)
        {
            clouds.Add(createCloud());
        }
        for (int i = 0; i < clouds.Count; i++)
        {
            SpriteRenderer sr = clouds[i];
            if (i < max)
            {
                sr.gameObject.SetActive(true);
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
            else
            {
                sr.gameObject.SetActive(false);
            }
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
