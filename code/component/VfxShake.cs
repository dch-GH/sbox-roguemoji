﻿using Sandbox;
using System;

namespace Roguemoji;

public class VfxShake : ThingComponent
{
	public float Lifetime { get; set; }
    public float Distance { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;
    }

	protected override void OnUpdate()
	{
        var dir = Utils.DegreesToVector(Game.Random.Float(0f, 360f));
        Thing.SetShakeOffset(dir * Utils.Map(TimeElapsed, 0f, Lifetime, Distance, 0f, EasingType.QuadOut));

        //if(TimeElapsed > Lifetime)
        //    Remove();

		base.OnUpdate();
    }

    public override void OnRemove()
    {
        Thing.SetShakeOffset(Vector2.Zero);
    }
}
