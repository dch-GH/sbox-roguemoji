using Sandbox;
using System;

namespace Roguemoji;
public partial class Bone : Thing
{
    protected override void OnAwake()
    {
        DisplayIcon = "🦴";
        DisplayName = "Bone";
        IconDepth = (int)IconDepthLevel.Normal;
        Tooltip = "A bone";
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
        Flammability = 8;

        if (Game.IsServer)
        {
            InitStat(StatType.Attack, 1);
        }
    }
}
