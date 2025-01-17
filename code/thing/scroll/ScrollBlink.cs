﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollBlink : Scroll
{
    protected override void OnAwake()
    {
        ScrollType = ScrollType.Blink;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Teleport to a target place nearby";
        Tooltip = "A scroll of Blink";

        SetTattoo(Globals.Icon(IconType.Blink));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😝", (int)PlayerIconPriority.UseScroll, 1.0f);

        BlinkThing(user, targetGridPos);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public static void BlinkThing(Thing thing, IntVector targetGridPos)
    {
        var things = thing.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Solid).ToList();
        if (things.Count > 0)
        {
            if (thing.ContainingGridManager.GetRandomEmptyAdjacentGridPos(targetGridPos, out var emptyGridPos, allowNonSolid: true))
                targetGridPos = emptyGridPos;
            else
                targetGridPos = thing.GridPos;
        }

        thing.ContainingGridManager.AddFloater(Globals.Icon(IconType.Blink), targetGridPos, 0.8f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);
        thing.SetGridPos(targetGridPos, setLastGridPosSame: true);
        thing.AddFloater(Globals.Icon(IconType.Blink), 1.1f, new Vector2(0, -3f), new Vector2(0, -12f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);

        if (thing.Brain is RoguemojiPlayer player)
        {
            player.RecenterCamera();
            player.VfxFlashCamera(0.8f, new Color(0.8f, 0.8f, 0.3f, 0.1f));
        }
    }
}
