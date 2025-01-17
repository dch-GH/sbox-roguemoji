﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionSpeed : Potion
{
    public override string SplashIcon => Thing.GetStatIcon(StatType.Speed);

    protected override void OnAwake()
    {
        PotionType = PotionType.Speed;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Temporarily quickens the drinker";
        Tooltip = "A speed potion";

        SetTattoo(Thing.GetStatIcon(StatType.Speed));

        if (Game.IsServer)
        {
            AddTrait("", "🏁", $"Quickens the drinker", offset: new Vector2(0f, 0f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.Speed))
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
        if (!thing.HasStat(StatType.Speed))
            return;

        var speedIncrease = thing.AddComponent<CSpeedIncrease>();
        speedIncrease.Lifetime = 30f;
        thing.AddSideFloater(Thing.GetStatIcon(StatType.Speed));
    }
}
