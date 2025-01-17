﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Leaf : Thing
{
    public override void OnSpawned()
    {
        DisplayIcon = "🍂";
        DisplayName = "Leaves";
        Description = "Small pile of dry leaves";
        Tooltip = "A pile of leaves";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 55;
    }

    // todo: make crunching noise when you move onto it
    // todo: wind blows it easily

    public override void OnAddComponent(TypeDescription type)
    {
        base.OnAddComponent(type);

        if (type == TypeLibrary.GetType(typeof(CProjectile)))
            SetIcon("🍃");
    }

    public override void OnRemoveComponent(TypeDescription type)
    {
        base.OnRemoveComponent(type);

        if (type == TypeLibrary.GetType(typeof(CProjectile)) || type == TypeLibrary.GetType(typeof(CTempIconDepth)))
            SetIcon("🍂");
    }

    public override void OnMovedOntoBy(Thing thing)
    {
        base.OnMovedOntoBy(thing);

        if (thing.HasFlag(ThingFlags.Solid))
        {
            thing.ContainingGridManager.PlaySfx("footstep_dry_leaves", GridPos, sourceThing: thing, loudness: 2);
        }
    }
}
