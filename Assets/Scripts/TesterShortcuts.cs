using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterShortcuts : MonoBehaviour
{
    public bool active = false;

    public void Update()
    {
        //SHIFT+` to activate key shortcuts
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            active = !active;
        }
        if (active)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.resetGame();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                activateAllCheckpoints();
            }
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                enableAbility(1, shift);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                enableAbility(2, shift);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                enableAbility(3, shift);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                enableAbility(4, shift);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                enableAbility(5, shift);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                enableAbility(6, shift);
            }
        }
    }
    public static void activateAllCheckpoints()
    {
        foreach (CheckPointChecker cpc in GameObject.FindObjectsOfType<CheckPointChecker>())
        {
            cpc.activate();
        }
    }
    public static void enableAbility(int abilityIndex, bool shift)
    {
        GameObject playerObject = GameManager.getPlayerObject();
        switch (abilityIndex)
        {
            case 1:
                ForceTeleportAbility fta = playerObject.GetComponent<ForceTeleportAbility>();
                fta.enabled = !fta.enabled;
                break;
            case 2:
                WallClimbAbility wca = playerObject.GetComponent<WallClimbAbility>();
                wca.enabled = !wca.enabled;
                break;
            case 3:
                if (shift)
                {
                    ShieldBubbleAbility sba = playerObject.GetComponent<ShieldBubbleAbility>();
                    sba.enabled = !sba.enabled;
                }
                else
                {
                    ElectricFieldAbility efa = playerObject.GetComponent<ElectricFieldAbility>();
                    efa.enabled = !efa.enabled;
                }
                break;
            case 4:
                SwapAbility sa = playerObject.GetComponent<SwapAbility>();
                sa.enabled = !sa.enabled;
                break;
            case 5:
                AirSliceAbility asa = playerObject.GetComponent<AirSliceAbility>();
                asa.enabled = !asa.enabled;
                break;
            case 6:
                LongTeleportAbility lta = playerObject.GetComponent<LongTeleportAbility>();
                lta.enabled = !lta.enabled;
                break;
        }
    }
}
