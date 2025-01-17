﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class PotionInvisible : Potion
{
    public override string SplashIcon => Globals.Icon(IconType.Invisible);

    protected override void OnAwake()
    {
        PotionType = PotionType.Invisibility;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = Potion.GetDisplayName(PotionType);
        Description = "Makes things temporarily invisible";
        Tooltip = "An invisibility potion";

        SetTattoo(Globals.Icon(IconType.Invisible));

        if (Game.IsServer)
        {
            AddTrait("", Globals.Icon(IconType.Invisible), $"Makes you invisible", offset: new Vector2(0f, 0f));
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
        MakeInvisible(thing);

        if (thing.WieldedThing != null)
            MakeInvisible(thing.WieldedThing);
    }

    void MakeInvisible(Thing thing)
    {
        var invisible = thing.AddComponent<CInvisible>();
        invisible.Lifetime = 60f;
    }
}
