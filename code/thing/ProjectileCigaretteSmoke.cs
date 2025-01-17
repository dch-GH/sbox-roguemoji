﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class ProjectileCigaretteSmoke : Thing
{
    public Direction Direction { get; set; }

    public override void OnSpawned()
    {
        DisplayIcon = "💨";
        DisplayName = "Cigarette Smoke";
        Description = "";
        Tooltip = "";
        IconDepth = (int)IconDepthLevel.Projectile;
        ThingFlags = ThingFlags.DoesntBumpThings;
        Flammability = 0;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }

    public override void OnMovedOntoThing(Thing thing)
    {
        base.OnMovedOntoThing(thing);

        thing.TakeDamageFrom(this);
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        thing.TakeDamageFrom(this);
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if (type == TypeLibrary.GetType(typeof(CProjectile)))
            Destroy();
    }
}
