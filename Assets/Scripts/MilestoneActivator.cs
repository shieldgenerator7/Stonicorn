using UnityEngine;

public abstract class MilestoneActivator : MemoryMonoBehaviour {
    
    public GameObject particle;
    public int starAmount = 25;
    public int starSpawnDuration = 25;
    public string abilityIndicatorName;//used for AbilityGainEffect
    
    public bool used = false;
    private float minX, maxX, minY, maxY;

    // Use this for initialization
    void Start()
    {
        if (transform.parent != null && !(transform.parent.position == Vector3.zero))
        {
            Bounds bounds = GetComponentInParent<SpriteRenderer>().bounds;
            float extra = 0.1f;
            minX = bounds.min.x - extra;
            maxX = bounds.max.x + extra;
            minY = bounds.min.y - extra;
            maxY = bounds.max.y + extra;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!used && coll.gameObject.Equals(Managers.Player.gameObject))
        {
            activate(true);
        }
    }

    public void activate(bool showFX)
    {
        if (showFX)
        {
            //Ability Indicator Animation Setup
            if (abilityIndicatorName != null && abilityIndicatorName != "")
            {
                foreach (GameObject abilityIndicator in GameObject.FindGameObjectsWithTag("AbilityIndicator"))
                {
                    if (abilityIndicator.name.Contains(abilityIndicatorName))
                    {
                        abilityIndicator.GetComponent<ParticleSystem>().Play();
                        break;
                    }
                }
            }
        }
        used = true;
        activateEffect();
        Managers.Game.saveMemory(this);
        Destroy(this);//makes sure it can only be used once
    }

    public abstract void activateEffect();
    
    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, used);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        used = memObj.found;
    }
}
