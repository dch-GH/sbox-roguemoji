﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class Mushroom : Thing
{
	public Mushroom()
	{
		DisplayIcon = "🍄";
        DisplayName = "Mushroom";
        Description = "There's a good chance it's poisonous.";
        Tooltip = "A mushroom.";
        IconDepth = 0;
        ShouldLogBehaviour = true;
		Flags = ThingFlags.Selectable | ThingFlags.Useable;

        if (Game.IsServer)
        {
            AddTrait("", "🍽️", "Eat for a random positive or negative effect.", offset: Vector2.Zero, tattooIcon: "🍄", tattooScale: 0.425f, tattooOffset: Vector2.Zero);
        }
    }
}
