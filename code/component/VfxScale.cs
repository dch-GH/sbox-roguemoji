using Sandbox;
using System;

namespace Roguemoji;

public class VfxScale : ThingComponent
{
    public float Lifetime { get; set; }
    public float StartScale { get; set; }
    public float EndScale { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;

        Thing.SetScale(StartScale);
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        Thing.SetScale(Utils.Map(TimeElapsed, 0f, Lifetime, StartScale, EndScale, EasingType.Linear));

        if (TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetScale(EndScale);
    }
}
