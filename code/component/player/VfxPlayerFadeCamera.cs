using Sandbox;
using System;

namespace Roguemoji;

public class VfxPlayerFadeCamera : PlayerComponent
{
    public float Lifetime { get; set; }
    public bool ShouldFadeOut { get; set; }

    public override void Init(RoguemojiPlayer player)
    {
        base.Init(player);

        ShouldUpdate = true;
        IsClientComponent = true;
    }

    protected override void OnUpdate()
    {
        var dt = Time.Delta;

        Player.CameraFade = ShouldFadeOut
            ? Utils.Map(TimeElapsed, 0f, Lifetime, 0f, 1f, EasingType.SineInOut)
            : Utils.Map(TimeElapsed, 0f, Lifetime, 1f, 0f, EasingType.SineInOut);

        Player.CameraFadeColor = new Color(0.02f, 0.02f, 0.02f);

        if (TimeElapsed > Lifetime)
            Remove();

        //Player.DrawDebugText($"{TimeElapsed}/{Lifetime}");
    }

    public override void OnRemove()
    {
        Player.CameraFade = ShouldFadeOut ? 1f : 0f;
    }
}
