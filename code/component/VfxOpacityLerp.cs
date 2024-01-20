using Sandbox;
using System;

namespace Roguemoji;

public class VfxOpacityLerp : ThingComponent
{
    public float Lifetime { get; set; }
    public float StartOpacity { get; set; }
    public float EndOpacity { get; set; }
    public EasingType EasingType { get; set; }

    public override void Init(Thing thing)
    {
        base.Init(thing);

        ShouldUpdate = true;
        IsClientComponent = true;

        Thing.SetOpacity(StartOpacity);
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        Thing.SetOpacity(Utils.Map(TimeElapsed, 0f, Lifetime, StartOpacity, EndOpacity, EasingType));

        if (TimeElapsed > Lifetime)
            Remove();
    }

    public override void OnRemove()
    {
        Thing.SetOpacity(EndOpacity);
    }
}
