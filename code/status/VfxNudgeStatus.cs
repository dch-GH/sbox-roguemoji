﻿using Sandbox;
using System;

namespace Interfacer;

public class VfxNudgeStatus : ThingStatus
{
    public Direction Direction { get; set; }
    public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientStatus = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var dir = GridManager.GetVectorForDirection(Direction);
        Thing.SetOffset(dir * Utils.MapReturn(TimeSinceStart, 0f, Lifetime, 0f, Distance, EasingType.QuadOut));

        if(TimeSinceStart > Lifetime)
            Remove();
    }

    public override void ReInitialize()
    {

    }

    public override void OnRemove()
    {
        Thing.SetOffset(Vector2.Zero);
    }
}