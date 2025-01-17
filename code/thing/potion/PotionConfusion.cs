﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionConfusion : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Confusion);

    protected override void OnAwake()
    {
        PotionType = PotionType.Confusion;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Makes drinker confused";
        Tooltip = "A confusion potion";

        SetTattoo(Globals.Icon(IconType.Confusion));

        if (Game.IsServer)
        {
            AddTrait("", "😵", $"Makes you confused", offset: new Vector2(0f, 0f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
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
        if (!thing.HasComponent<CActing>())
            return;

        var confused = thing.AddComponent<CConfused>();
        confused.Lifetime = 60f;
        thing.AddSideFloater(Globals.Icon(IconType.Confusion));
    }
}
