using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class CTempIconDepth : ThingComponent
{
    public float Lifetime { get; set; }
    private int _oldIconDepth = -1;

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        TimeElapsed += dt;

        if (TimeElapsed > Lifetime)
            Remove();
    }

    public void SetTempIconDepth(int depth)
    {
        _oldIconDepth = Thing.IconDepth;
        Thing.IconDepth = depth;
    }

    public override void OnRemove()
    {
        if (_oldIconDepth >= 0)
            Thing.IconDepth = _oldIconDepth;
    }
}
