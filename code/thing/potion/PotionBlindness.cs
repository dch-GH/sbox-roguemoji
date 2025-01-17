﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionBlindness : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Blindness);

    protected override void OnAwake()
    {
        PotionType = PotionType.Blindness;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Blinds the drinker";
        Tooltip = "A blindness potion";

        SetTattoo(Globals.Icon(IconType.Blindness));

        if (Game.IsServer)
        {
            AddTrait("", "🚫", $"Drinking temporarily reduces your {Thing.GetStatIcon(StatType.SightDistance)}", offset: new Vector2(0f, -1f), tattooIcon: Thing.GetStatIcon(StatType.SightDistance), tattooOffset: new Vector2(-0.2f, 0.5f), tattooScale: 0.5f);
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (!user.HasStat(StatType.SightDistance))
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
        if (!thing.HasStat(StatType.SightDistance))
            return;

        var blinded = thing.AddComponent<CBlinded>();
        blinded.Lifetime = 30f;
        thing.AddSideFloater(Globals.Icon(IconType.Blindness));
    }
}
