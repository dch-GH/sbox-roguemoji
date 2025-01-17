﻿using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionMedicine : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Medicine);

    protected override void OnAwake()
    {
        PotionType = PotionType.Medicine;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Cures ailments";
        Tooltip = "A medicine potion";

        SetTattoo(Globals.Icon(IconType.Medicine));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Medicine), $"Cures {Globals.Icon(IconType.Poison)}{Globals.Icon(IconType.Hallucination)}{Globals.Icon(IconType.Confusion)}{Globals.Icon(IconType.Blindness)}{Globals.Icon(IconType.Fear)}", offset: new Vector2(0f, 0f));
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
        if (thing.HasComponent<CPoisoned>() || thing.HasComponent<CHallucinating>() || thing.HasComponent<CConfused>() || thing.HasComponent<CBlinded>() || thing.HasComponent<CFearful>())
            thing.AddSideFloater(Globals.Icon(IconType.Medicine));

        thing.RemoveComponent<CPoisoned>();
        thing.RemoveComponent<CHallucinating>();
        thing.RemoveComponent<CConfused>();
        thing.RemoveComponent<CBlinded>();
        thing.RemoveComponent<CFearful>();
    }
}
