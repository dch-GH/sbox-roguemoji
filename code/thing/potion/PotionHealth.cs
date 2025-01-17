﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionHealth : Potion
{
    public override string SplashIcon => GetStatIcon(StatType.Health);
    public int HealthAmount { get; private set; }

    protected override void OnAwake()
    {
        PotionType = PotionType.Health;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Recover some health";
        Tooltip = "A health potion";

        SetTattoo(GetStatIcon(StatType.Health));

        if (Game.IsServer)
        {
            HealthAmount = 5;
            AddTrait("", GetStatIcon(StatType.Health), $"Drinking recovers {HealthAmount}{GetStatIcon(StatType.Health)}", offset: new Vector2(0f, -1f), labelText: $"+{HealthAmount}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Health))
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
        if (!thing.HasStat(StatType.Health))
            return;

        int amountRecovered = Math.Min(HealthAmount, thing.GetStatMax(StatType.Health) - thing.GetStatClamped(StatType.Health));
        thing.AddFloater(GetStatIcon(StatType.Health), 1.33f, new Vector2(Game.Random.Float(8f, 12f) * (thing.FloaterNum % 2 == 0 ? -1 : 1), Game.Random.Float(-3f, 8f)), new Vector2(Game.Random.Float(12f, 15f) * (thing.FloaterNum++ % 2 == 0 ? -1 : 1), Game.Random.Float(-13f, 3f)), height: Game.Random.Float(10f, 35f), text: $"+{amountRecovered}", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.1f, scale: 0.75f);
        thing.AdjustStat(StatType.Health, HealthAmount);
    }
}
