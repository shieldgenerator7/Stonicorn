using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSliceAbility : PlayerAbility {

    public float sliceDamage = 100f;//how much force damage to do to objects that get cut

	// Use this for initialization
	protected override void Start () {
        base.Start();
        playerController.maxAirPorts++;
        playerController.onTeleport += sliceThings;
	}
	
	void sliceThings(Vector2 oldPos, Vector2 newPos)
    {//2018-02-15: WARNING: This method assumes that onTeleport is called BEFORE the grounded state changes
        if (!playerController.Grounded)
        {
            bool slicedSomething = false;
            RaycastHit2D[] rch2ds = Physics2D.RaycastAll(oldPos, (newPos - oldPos), Vector2.Distance(oldPos,newPos));
            foreach (RaycastHit2D rch2d in rch2ds)
            {
                if (rch2d.collider.gameObject != gameObject)
                {
                    HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
                    if (hm)
                    {
                        hm.addIntegrity(-sliceDamage);
                        slicedSomething = true;
                    }
                }
            }
            if (slicedSomething)
            {
                //if the player slices something, allow them to teleport once more in the air
                playerController.airPorts--;
            }
        }
    }
}
