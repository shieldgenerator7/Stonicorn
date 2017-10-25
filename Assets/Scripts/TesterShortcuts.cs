using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesterShortcuts : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.resetGame();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            activateAllCheckpoints();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            enableAbility(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            enableAbility(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            enableAbility(3);
        }
    }
    public static void activateAllCheckpoints()
    {
        foreach (CheckPointChecker cpc in GameObject.FindObjectsOfType<CheckPointChecker>())
        {
            cpc.activate();
        }
    }
    public static void enableAbility(int abilityIndex)
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
                ShieldBubbleAbility sba = playerObject.GetComponent<ShieldBubbleAbility>();
                sba.enabled = !sba.enabled;
                break;
        }
    }
}
