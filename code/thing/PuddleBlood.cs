﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class PuddleBlood : Thing
{
    private float _elapsedTime;
    private int _iconState;

	public PuddleBlood()
	{
		DisplayIcon = "🩸";
        DisplayName = "Puddle of Blood";
        Description = "A puddle of blood";
        Tooltip = "A puddle of blood";
        IconDepth = (int)IconDepthLevel.Normal;
        ShouldUpdate = true;
		//Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp;
    }

    // todo: make splashing noise when you move onto it
    // todo: make visible when walking onto this while invisible

    public override void Update(float dt)
    {
        base.Update(dt);

        _elapsedTime += dt;

        if(_iconState == 0 && _elapsedTime > 0.25f)
        {
            _iconState++;
            DisplayIcon = "🔴";
            IconDepth = (int)IconDepthLevel.Puddle;
        }
        else if(_iconState == 1 && _elapsedTime > 0.4f)
        {
            _iconState++;
            DisplayIcon = "🟥";
        }
        else if(_iconState == 2 && _elapsedTime > 25f)
        {
            _iconState++;
            VfxOpacityLerp(5f, 1f, 0f);
        }
        else if(_iconState == 3 && _elapsedTime > 30f)
        {
            Destroy();
        }
    }
}
