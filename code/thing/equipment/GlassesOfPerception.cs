﻿using Sandbox;
using System;

namespace Roguemoji;
public partial class GlassesOfPerception : Thing
{
    public int Range { get; set; }
    public int IconId { get; set; }

    protected override void OnAwake()
    {
        DisplayIcon = "👓️";
        DisplayName = "Glasses of Perception";
        Description = "Allows you to see invisible things";
        Tooltip = "Glasses of Perception";
        IconDepth = (int)IconDepthLevel.Normal;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Equipment;
        Flammability = 13;

        SetTattoo("👁️", scale: 0.4f, offset: new Vector2(0f, 0f), offsetWielded: Vector2.Zero, offsetInfo: new Vector2(5f, 4f), offsetCharWielded: Vector2.Zero, offsetInfoWielded: Vector2.Zero);

        if (Game.IsServer)
        {
            Range = 5;
            InitStat(StatType.Perception, Range, isModifier: true);
            //AddTrait("", Globals.Icon(IconType.Invisible), $"Reveals invisible things", offset: new Vector2(0f, -1f), tattooIcon: "👁️‍🗨️", tattooOffset: new Vector2(8f, 8f), tattooScale: 0.7f);
        }
    }

    public override void OnEquippedTo(Thing thing)
    {
        base.OnEquippedTo(thing);

        if (!thing.HasStat(StatType.Perception))
            thing.InitStat(StatType.Perception, Range);
        else
            thing.AdjustStat(StatType.Perception, Range);

        if (thing is Smiley smiley)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                IconId = iconPriority.AddIconPriority("🤓", (int)PlayerIconPriority.GlassesOfPerception);
            }
        }
    }

    public override void OnUnequippedFrom(Thing thing)
    {
        base.OnUnequippedFrom(thing);

        thing.AdjustStat(StatType.Perception, -Range);

        if (thing is Smiley smiley)
        {
            if (thing.GetComponent<CIconPriority>(out var component))
            {
                var iconPriority = (CIconPriority)component;
                iconPriority.RemoveIconPriority(IconId);
            }
        }
    }
}
