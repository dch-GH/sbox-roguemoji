﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class Joystick : Thing
{
    [Net] public int EnergyCost { get; private set; }
    public float CooldownTime { get; private set; }
    public override string AbilityName => "Move";

    public override void OnSpawned()
    {
        DisplayIcon = "🕹️";
        DisplayName = "Joystick";
        Description = "Lets you move in any direction";
        Tooltip = "A joystick";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;
        Flammability = 13;

        if (Game.IsServer)
        {
            EnergyCost = 3;
            CooldownTime = 30f;

            AddTrait(AbilityName, "🕹️", $"Move in any direction", offset: new Vector2(0f, -1f), isAbility: true);
            AddTrait("", GetStatIcon(StatType.Energy), $"Ability costs {EnergyCost}{GetStatIcon(StatType.Energy)}", offset: new Vector2(0f, -3f), labelText: $"{EnergyCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        var energy = user.GetStatClamped(StatType.Energy);
        if (energy < EnergyCost && !ignoreResources)
        {
            if (shouldLogMessage && user.Brain is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {EnergyCost}{GetStatIcon(StatType.Energy)} to use {ChatDisplayIcons} but you only have {energy}{GetStatIcon(StatType.Energy)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        if (IsOnCooldown)
            return;

        if (!user.TrySpendStat(StatType.Energy, EnergyCost))
            return;

        var startingGridPos = user.GridPos;

        var direction = GridManager.GetDirectionForIntVector(targetGridPos - startingGridPos);
        var player = (RoguemojiPlayer)user.Brain;
        bool success = user.TryMove(direction, out bool switchedLevel, out bool actionWasntReady, shouldAnimate: true, dontRequireAction: true);

        if (success)
        {
            user.VfxFly(startingGridPos, 0.2f);

            if (player != null)
                player.RecenterCamera(shouldAnimate: true);
        }

        StartCooldown(CooldownTime);

        base.Use(user, gridType, targetGridPos);
    }

    public override HashSet<IntVector> GetAimingTargetCellsClient()
    {
        Game.AssertClient();

        if (ThingWieldingThis == null)
            return null;

        HashSet<IntVector> aimingCells = new HashSet<IntVector>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var gridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                var gridManager = ThingWieldingThis.ContainingGridManager;
                if (!gridManager.IsGridPosInBounds(gridPos))
                    continue;

                aimingCells.Add(gridPos);
            }
        }

        return aimingCells;
    }

    public override bool IsPotentialAimingTargetCell(IntVector gridPos)
    {
        if (ThingWieldingThis == null)
            return false;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var currGridPos = ThingWieldingThis.GridPos + new IntVector(x, y);
                if (gridPos.Equals(currGridPos))
                    return true;
            }
        }

        return false;
    }
}
