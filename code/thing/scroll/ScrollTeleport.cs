﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollTeleport : Scroll
{
    protected override void OnAwake()
    {
        ScrollType = ScrollType.Teleport;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Teleport to a random place on the current floor";
        Tooltip = "A scroll of Teleport";

        SetTattoo(Globals.Icon(IconType.Teleport));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😮", (int)PlayerIconPriority.UseScroll, 1.0f);

        TeleportThing(user);

        // reveal scroll at destination after teleporting
        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public static bool TeleportThing(Thing thing, bool showStartFloater = true)
    {
        if (thing.ContainingGridManager.GetRandomEmptyGridPos(out var targetGridPos, allowNonSolid: true))
        {
            if (showStartFloater)
                thing.ContainingGridManager.AddFloater(Globals.Icon(IconType.Teleport), targetGridPos, 0.8f, new Vector2(0, -3f), new Vector2(0, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.1f);

            thing.SetGridPos(targetGridPos, setLastGridPosSame: true);
            thing.AddFloater(Globals.Icon(IconType.Teleport), 1.1f, new Vector2(0, -3f), new Vector2(0, -12f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f);

            if (thing.Brain is RoguemojiPlayer player)
            {
                player.RecenterCamera();
                player.VfxFlashCamera(0.8f, new Color(0.2f, 0.35f, 1f, 0.4f));
            }

            return true;
        }

        return false;
    }
}
