﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class WhiteCane : Thing
{
    public int MinSightChange { get; set; }
    public Trait Trait { get; private set; }

    public override void OnSpawned()
    {
        DisplayIcon = "🦯";
        DisplayName = "White Cane";
        Description = "Useful when you can't see anything";
        Tooltip = "A white cane";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 9;

        if (Game.IsServer)
        {
            MinSightChange = 1;

            InitStat(StatType.Attack, 2);
            AddTrait("", "😎", $"Prevents your {GetStatIcon(StatType.SightDistance)} from reaching zero", offset: Vector2.Zero, tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f));
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        thing.AdjustStatMin(StatType.SightDistance, MinSightChange);
        Trait = thing.AddTrait("", "😎", $"Your {GetStatIcon(StatType.SightDistance)} can't go down to zero", offset: Vector2.Zero, tattooIcon: "🦯", tattooScale: 0.7f, tattooOffset: new Vector2(7f, 6f), source: DisplayName);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldedBy(thing);

        thing.AdjustStatMin(StatType.SightDistance, -MinSightChange);
        thing.RemoveTrait(Trait);
    }
}
