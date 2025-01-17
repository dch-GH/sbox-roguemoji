﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollFear : Scroll
{
    protected override void OnAwake()
    {
        ScrollType = ScrollType.Fear;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Scare all enemies near you";
        Tooltip = "A scroll of Fear";

        SetTattoo(Globals.Icon(IconType.Fear));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var c))
            ((CIconPriority)c).AddIconPriority("🤬", (int)PlayerIconPriority.UseScroll, 1.0f);

        int radius = 3;
        var things = user.ContainingGridManager.GetThingsWithinRange(user.GridPos, radius, allFlags: ThingFlags.Solid);
        foreach (var thing in things)
        {
            if (thing == user || thing.HasComponent<CFearful>())
                continue;

            if (thing.GetComponent<CActing>(out var component))
            {
                var fearful = thing.AddComponent<CFearful>();
                fearful.Lifetime = Game.Random.Float(8f, 10f);
                fearful.FearedThing = user;

                var acting = (CActing)component;
                acting.ActionTimer = acting.ActionDelay - Game.Random.Float(0f, 0.2f);
            }
        }

        var circlePoints = user.ContainingGridManager.GetPointsWithinCircle(user.GridPos, radius);
        foreach (var point in circlePoints)
        {
            if (!point.Equals(user.GridPos))
                user.ContainingGridManager.AddFloater("❗️", point, Game.Random.Float(0.65f, 0.85f), Vector2.Zero, new Vector2(0f, Game.Random.Float(-1f, -10f)), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, Game.Random.Float(0.25f, 0.35f));
        }

        Destroy();
    }
}
