﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionEnergy : Potion
{
    public override string SplashIcon => GetStatIcon(StatType.Energy);
    public int EnergyAmount { get; private set; }

    protected override void OnAwake()
    {
        PotionType = PotionType.Energy;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Recover some energy";
        Tooltip = "An energy potion";

        SetTattoo(GetStatIcon(StatType.Energy));

        if (Game.IsServer)
        {
            EnergyAmount = 10;
            AddTrait("", GetStatIcon(StatType.Energy), $"Drinking recovers {EnergyAmount}{GetStatIcon(StatType.Energy)}", offset: new Vector2(0f, -3f), labelText: $"+{EnergyAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Energy))
            return false;

        return true;
    }

    public override void Use(Thing user)
    {
        ApplyEffectToThing(user);
        Destroy();

        base.Use(user);
    }

    public override void ApplyEffectToThing(Thing thing)
    {
        if (!thing.HasStat(StatType.Energy))
            return;

        int amountRecovered = Math.Min(EnergyAmount, thing.GetStatMax(StatType.Energy) - thing.GetStatClamped(StatType.Energy));
        thing.AddFloater(GetStatIcon(StatType.Energy), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (thing.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (thing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);
        thing.AdjustStat(StatType.Energy, EnergyAmount);
    }
}
