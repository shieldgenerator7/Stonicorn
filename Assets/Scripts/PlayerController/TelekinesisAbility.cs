using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisAbility : PlayerAbility
{
    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul) { }

    protected override bool isGrounded() => false;

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos) { }
}
