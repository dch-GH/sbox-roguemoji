﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CFearful : ThingComponent
{
    public Trait Trait { get; private set; }
    public Thing FearedThing { get; set; }
    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Fearful", Globals.Icon(IconType.Fear), $"Terrified of something", offset: Vector2.Zero);

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component))
            IconId = ((CIconPriority)component).AddIconPriority("😱", (int)PlayerIconPriority.Fearful);

        thing.AddFloater("💧", time: 0f, new Vector2(10f, -10f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.65f);
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        TimeElapsed += dt;

        if (Lifetime > 0f && TimeElapsed > Lifetime)
            Remove();

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);
    }

    public override void OnRemove()
    {
        Thing.RemoveFloater("💧");

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).RemoveIconPriority(IconId);

        Thing.RemoveTrait(Trait);
    }

    public override void OnThingDestroyed()
    {
        Thing.RemoveFloater("💧");
    }

    public static IntVector GetTargetRetreatPoint(IntVector startingPoint, IntVector avoidPoint, GridManager gridManager)
    {
        IntVector diff = startingPoint - avoidPoint;
        IntVector targetPos = new IntVector(diff.x < 0 ? 0 : gridManager.GridWidth - 1, diff.y < 0 ? 0 : gridManager.GridHeight - 1);

        return targetPos;
    }
}
