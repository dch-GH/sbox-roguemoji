﻿using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CStunned : ThingComponent
{
    public Trait Trait { get; private set; }

    public float Lifetime { get; set; }
    public int IconId { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;

        Trait = thing.AddTrait("Stunned", $"{Globals.Icon(IconType.Stunned)}", $"Too shocked to do anything", offset: Vector2.Zero);

        if (thing.GetComponent<CActing>(out var component))
            ((CActing)component).PreventAction();

        if (thing is Smiley && thing.GetComponent<CIconPriority>(out var component2))
            IconId = ((CIconPriority)component2).AddIconPriority("😖", (int)PlayerIconPriority.Stunned);

        thing.AddFloater(Globals.Icon(IconType.Stunned), time: 0f, new Vector2(0f, -15f), Vector2.Zero, height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.025f, scale: 0.9f, opacity: 0.7f);
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        TimeElapsed += dt;
        if (Lifetime > 0f && TimeElapsed > Lifetime)
        {
            Remove();
            return;
        }

        Trait.BarPercent = 1f - Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f);
    }

    public override void OnRemove()
    {
        Thing.RemoveTrait(Trait);

        if (Thing.Brain is RoguemojiPlayer player)
            player.ClearQueuedAction();

        if (Thing.GetComponent<CActing>(out var component))
            ((CActing)component).AllowAction();

        if (Thing is Smiley && Thing.GetComponent<CIconPriority>(out var component2))
            ((CIconPriority)component2).RemoveIconPriority(IconId);

        Thing.RemoveFloater(Globals.Icon(IconType.Stunned));
    }

    public override void OnThingDestroyed()
    {
        Thing.RemoveFloater(Globals.Icon(IconType.Stunned));
    }

    public override void OnThingDied()
    {
        Remove();
    }
}
