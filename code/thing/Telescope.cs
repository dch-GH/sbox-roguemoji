﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Telescope : Thing
{
    public int SightAmount { get; private set; }
    public int SpeedAmount { get; private set; }

    public Telescope()
	{
		DisplayIcon = "🔭";
        DisplayName = "Telescope";
        Description = "See farther but move slower.";
        Tooltip = "A telescope.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable;

        if (Game.IsServer)
        {
            SightAmount = 4;
            SpeedAmount = -3;
            InitStat(StatType.Sight, SightAmount, int.MinValue, isModifier: true);
            InitStat(StatType.Speed, SpeedAmount, int.MinValue, isModifier: true);
        }
    }

    public override void OnWieldedBy(Thing thing)
    {
        base.OnWieldedBy(thing);

        thing.AdjustStat(StatType.Sight, SightAmount);
        thing.AdjustStat(StatType.Speed, SpeedAmount);
    }

    public override void OnNoLongerWieldedBy(Thing thing)
    {
        base.OnNoLongerWieldedBy(thing);

        thing.AdjustStat(StatType.Sight, -SightAmount);
        thing.AdjustStat(StatType.Speed, -SpeedAmount);
    }
}
