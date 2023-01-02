﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class TreeEvergreen : Thing
{
	public TreeEvergreen()
	{
		DisplayIcon = "🌲";
        DisplayName = "Evergreen Tree";
        Description = "A tall tree.";
        Tooltip = "An evergreen tree.";
        IconDepth = 1;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Solid | ThingFlags.Selectable;
		PathfindMovementCost = 999f;
        ShouldUpdate = false;
		SightBlockAmount = 14;
    }
}
