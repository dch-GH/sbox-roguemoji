﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class ProjectileArrow : Thing
{
    public Direction Direction { get; set; }

    public override void OnSpawned()
    {
        DisplayIcon = "🔰";
        DisplayName = "Arrow";
        Description = "";
        Tooltip = "";
        IconDepth = (int)IconDepthLevel.Projectile;
        Flammability = 0;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }

    public override void OnBumpedIntoThing(Thing thing, Direction direction)
    {
        base.OnBumpedIntoThing(thing, direction);
        Destroy();
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);
        Destroy();
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if (type == TypeLibrary.GetType(typeof(CProjectile)))
            Destroy();
    }
}
