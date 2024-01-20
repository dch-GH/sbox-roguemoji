using Sandbox;
using System;

namespace Roguemoji;
public partial class Bone : Thing
{
    public override void OnSpawned()
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
