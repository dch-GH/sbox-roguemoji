﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionHallucination : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Hallucination);

    protected override void OnAwake()
    {
        PotionType = PotionType.Hallucination;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Makes drinker hallucinate";
        Tooltip = "A hallucination potion";

        SetTattoo(Globals.Icon(IconType.Hallucination));

        if (Game.IsServer)
        {
            AddTrait("", "🐘", $"Hallucinating makes things appear to be different things", offset: new Vector2(0f, -1f), tattooIcon: "🤪", tattooScale: 0.9f, tattooOffset: new Vector2(12f, 12f));
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

        var hallucinating = thing.AddComponent<CHallucinating>();
        hallucinating.Lifetime = 60f;
        thing.AddSideFloater(Globals.Icon(IconType.Hallucination));
    }
}
