using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                Managers.Game.resetGame();
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
            cpc.Discovered = true;
        }
    }
    public static void toggleAbility(int abilityIndex)
    {
        enableAbility(abilityIndex, !abilityEnabled(abilityIndex));
    }
    public static void enableAbility(int abilityIndex, bool enable)
    {
        GameObject playerObject = Managers.Player.gameObject;
        switch (abilityIndex)
        {
            case (int)Cheat.FORCE_LAUNCH:
                ForceLaunchAbility fta = playerObject.GetComponent<ForceLaunchAbility>();
                fta.Active = enable;
                break;
            case (int)Cheat.WALL_CLIMB:
                WallClimbAbility wca = playerObject.GetComponent<WallClimbAbility>();
                wca.Active = enable;
                break;
            case (int)Cheat.ELECTRIC_BEAM:
                ElectricBeamAbility eba = playerObject.GetComponent<ElectricBeamAbility>();
                eba.Active = enable;
                break;
            case (int)Cheat.SWAP:
                SwapAbility sa = playerObject.GetComponent<SwapAbility>();
                sa.Active = enable;
                break;
            case (int)Cheat.AIR_SLICE:
                AirSliceAbility asa = playerObject.GetComponent<AirSliceAbility>();
                asa.Active = enable;
                break;
            case (int)Cheat.LONG_TELEPORT:
                LongTeleportAbility lta = playerObject.GetComponent<LongTeleportAbility>();
                lta.Active = enable;
                break;
        }
    }
    public static bool abilityEnabled(int abilityIndex)
    {
        GameObject playerObject = Managers.Player.gameObject;
        switch (abilityIndex)
        {
            case (int)Cheat.FORCE_LAUNCH:
                ForceLaunchAbility fta = playerObject.GetComponent<ForceLaunchAbility>();
                return fta.Active;
            case (int)Cheat.WALL_CLIMB:
                WallClimbAbility wca = playerObject.GetComponent<WallClimbAbility>();
                return wca.Active;
            case (int)Cheat.ELECTRIC_BEAM:
                ElectricBeamAbility eba = playerObject.GetComponent<ElectricBeamAbility>();
                return eba.Active;
            case (int)Cheat.SWAP:
                SwapAbility sa = playerObject.GetComponent<SwapAbility>();
                return sa.Active;
            case (int)Cheat.AIR_SLICE:
                AirSliceAbility asa = playerObject.GetComponent<AirSliceAbility>();
                return asa.Active;
            case (int)Cheat.LONG_TELEPORT:
                LongTeleportAbility lta = playerObject.GetComponent<LongTeleportAbility>();
                return lta.Active;
            default:
                throw new System.ArgumentException("abilityIndex is invalid!: " + abilityIndex);
        }
    }

    public enum Cheat
    {
        FORCE_LAUNCH = 1,
        WALL_CLIMB = 4,
        ELECTRIC_BEAM = 3,
        SWAP = 2,
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
            Managers.Game.resetGame();
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
            //If no checkpoints are undiscovered (all CPs discovered)
            //Then this cheat is active,
            //even if the CPs were activated by legitimate means
            return !FindObjectsOfType<CheckPointChecker>().ToList()
                .Any(cpc => !cpc.Discovered);
        }
        else if (cheat == Cheat.RESET_GAME)
        {
            return false;
        }
        return false;
    }
}
