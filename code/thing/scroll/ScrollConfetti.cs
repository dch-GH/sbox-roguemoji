﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollConfetti : Scroll
{
    protected override void OnAwake()
    {
        ScrollType = ScrollType.Confetti;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Tosses all your items in the air";
        Tooltip = "A scroll of Confetti";

        SetTattoo(Globals.Icon(IconType.Confetti));
    }

    public override void Use(Thing user)
    {
        base.Use(user);

        if (user.Brain is RoguemojiPlayer player)
            user.AddComponent<CConfetti>();

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }
}
