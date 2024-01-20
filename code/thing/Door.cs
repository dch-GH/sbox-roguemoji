using Sandbox;
using System;

namespace Roguemoji;
public partial class Door : Thing
{
    protected override void OnAwake()
    {
        DisplayIcon = "️🚪";
        DisplayName = "Door";
        IconDepth = (int)IconDepthLevel.Solid;
        Tooltip = "A door";
        ThingFlags = ThingFlags.Solid | ThingFlags.Selectable | ThingFlags.CantBePushed;
        PathfindMovementCost = 15f;
        Flammability = 0;

        if (Game.IsServer)
        {
            InitStat(StatType.SightBlockAmount, 20);
        }
    }
}
