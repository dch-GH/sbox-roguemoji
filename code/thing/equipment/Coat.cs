﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Coat : Thing
{
    public int MaxHealthAmount { get; private set; }

    protected override void OnAwake()
    {
        DisplayIcon = "🧥";
        DisplayName = "Coat";
        Description = "Thick and warm";
        Tooltip = "A thick coat";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 22;

        if (Game.IsServer)
        {
            MaxHealthAmount = 3;

            InitStat(StatType.MaxHealth, MaxHealthAmount, isModifier: true);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        thing.AdjustStatMax(StatType.Health, MaxHealthAmount);
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStatMax(StatType.Health, -MaxHealthAmount);
    }
}
