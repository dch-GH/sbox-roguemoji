using Sandbox;
using System;

namespace Roguemoji;

//public enum LiquidType { Water, Lava, Blood, Mud, Oil, Piss, ToxicSludge, Snow, Purple }

public partial class Puddle : Thing
{
    protected float _elapsedTime;
    protected int _iconState;

    public PotionType LiquidType { get; protected set; }

    protected override void OnAwake()
    {
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
        ThingFlags = ThingFlags.Selectable | ThingFlags.Puddle | ThingFlags.CantBePushed;
    }
}
