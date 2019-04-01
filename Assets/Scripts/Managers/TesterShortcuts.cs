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
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                toggleAbility(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                toggleAbility(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                toggleAbility(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                toggleAbility(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                toggleAbility(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                toggleAbility(6);
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
    public static void toggleAbility(int abilityIndex)
    {
        enableAbility(abilityIndex, !abilityEnabled(abilityIndex));
    }
    public static void enableAbility(int abilityIndex, bool enable)
    {
        GameObject playerObject = GameManager.Player.gameObject;
        switch (abilityIndex)
        {
            case 1:
                ForceTeleportAbility fta = playerObject.GetComponent<ForceTeleportAbility>();
                fta.enabled = enable;
                break;
            case 2:
                WallClimbAbility wca = playerObject.GetComponent<WallClimbAbility>();
                wca.enabled = enable;
                break;
            case 3:
                ElectricFieldAbility efa = playerObject.GetComponent<ElectricFieldAbility>();
                efa.enabled = enable;
                break;
            case 4:
                SwapAbility sa = playerObject.GetComponent<SwapAbility>();
                sa.enabled = enable;
                break;
            case 5:
                AirSliceAbility asa = playerObject.GetComponent<AirSliceAbility>();
                asa.enabled = enable;
                break;
            case 6:
                LongTeleportAbility lta = playerObject.GetComponent<LongTeleportAbility>();
                lta.enabled = enable;
                break;
        }
    }
    public static bool abilityEnabled(int abilityIndex)
    {
        GameObject playerObject = GameManager.Player.gameObject;
        switch (abilityIndex)
        {
            case 1:
                ForceTeleportAbility fta = playerObject.GetComponent<ForceTeleportAbility>();
                return fta.enabled;
            case 2:
                WallClimbAbility wca = playerObject.GetComponent<WallClimbAbility>();
                return wca.enabled;
            case 3:
                ElectricFieldAbility efa = playerObject.GetComponent<ElectricFieldAbility>();
                return efa.enabled;
            case 4:
                SwapAbility sa = playerObject.GetComponent<SwapAbility>();
                return sa.enabled;
            case 5:
                AirSliceAbility asa = playerObject.GetComponent<AirSliceAbility>();
                return asa.enabled;
            case 6:
                LongTeleportAbility lta = playerObject.GetComponent<LongTeleportAbility>();
                return lta.enabled;
            default:
                throw new System.ArgumentException("abilityIndex is invalid!: " + abilityIndex);
        }
    }

    public enum Cheat
    {
        FORCE_CHARGE = 1,
        WALL_CLIMB = 2,
        ELECTRIC_FIELD = 3,
        SWAP = 4,
        AIR_SLICE = 5,
        LONG_TELEPORT = 6,
        ACTIVATE_ALL_CHECKPOINTS = 7,
        RESET_GAME = 8
    }
    public static void activateCheat(Cheat cheat, bool activate)
    {
        if ((int)cheat <= 6)
        {
            enableAbility((int)cheat, activate);
        }
        else if (cheat == Cheat.ACTIVATE_ALL_CHECKPOINTS)
        {
            activateAllCheckpoints();
        }
        else if (cheat == Cheat.RESET_GAME)
        {
            GameManager.resetGame();
        }
    }
    public static bool cheatActive(Cheat cheat)
    {
        if ((int)cheat <= 6)
        {
            return abilityEnabled((int)cheat);
        }
        else if (cheat == Cheat.ACTIVATE_ALL_CHECKPOINTS)
        {
            return false;
        }
        else if (cheat == Cheat.RESET_GAME)
        {
            return false;
        }
        return false;
    }
}
