﻿using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class ScrollSentience : Scroll
{
    protected override void OnAwake()
    {
        ScrollType = ScrollType.Sentience;
        ThingFlags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable | ThingFlags.UseRequiresAiming | ThingFlags.AimTypeTargetCell;

        DisplayName = GetDisplayName(ScrollType);
        Description = "Animate a non-thinking object";
        Tooltip = "A scroll of Sentience";

        SetTattoo(Globals.Icon(IconType.Sentience));

        if (Game.IsServer)
        {
            AddTrait("", "📈", $"Spell range increased by {GetStatIcon(StatType.Intelligence)}", offset: new Vector2(0f, -1f), tattooIcon: GetStatIcon(StatType.Intelligence), tattooScale: 0.6f, tattooOffset: new Vector2(6f, -8f));
        }
    }

    public override void Use(Thing user, GridType gridType, IntVector targetGridPos)
    {
        base.Use(user, gridType, targetGridPos);

        if (user is Smiley && user.GetComponent<CIconPriority>(out var component))
            ((CIconPriority)component).AddIconPriority("😯", (int)PlayerIconPriority.UseScroll, 1.0f);

        var thing = user.ContainingGridManager.GetThingsAt(targetGridPos).WithAll(ThingFlags.Selectable).Where(x => x.Brain == null).Where(x => ScrollSentience.CanGainSentience(x)).OrderByDescending(x => x.GetZPos()).FirstOrDefault();

        if (thing != null)
        {
            var brain = new SquirrelBrain();
            brain.ControlThing(thing);

            if (!thing.HasStat(StatType.Health)) thing.InitStat(StatType.Health, 10, 0, 10);
            if (!thing.HasStat(StatType.SightDistance)) thing.InitStat(StatType.SightDistance, 7);
            if (!thing.HasStat(StatType.SightPenetration)) thing.InitStat(StatType.SightPenetration, 7);
            if (!thing.HasStat(StatType.Speed)) thing.InitStat(StatType.Speed, 5);
            if (!thing.HasStat(StatType.Attack)) thing.InitStat(StatType.Attack, 1);
            if (!thing.HasStat(StatType.Hearing)) thing.InitStat(StatType.Hearing, 3);
            if (!thing.HasStat(StatType.SightBlockAmount)) InitStat(StatType.SightBlockAmount, 3);

            thing.AddFlag(ThingFlags.Solid);
            thing.AddFlag(ThingFlags.CanWieldThings);
            thing.AddFlag(ThingFlags.CanGainMutations);

            if (!thing.HasComponent<CTargeting>())

                thing.AddComponent<CTargeting>();
            if (!thing.HasComponent<CActing>())
            {
                var acting = thing.AddComponent<CActing>();
                acting.ActionDelay = CActing.CalculateActionDelay(thing.GetStatClamped(StatType.Speed));
                acting.ActionTimer = Game.Random.Float(0f, 1f);
            }

            thing.AddFloater(Globals.Icon(IconType.Sentience), time: 0f, new Vector2(0f, -5f), new Vector2(0f, -5f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.3f, scale: 0.6f, opacity: 1f, showOnSeen: true);
        }

        user.ContainingGridManager.AddFloater(Globals.Icon(IconType.Sentience), targetGridPos, 0.8f, new Vector2(0f, 0f), new Vector2(0f, -14f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: true, EasingType.SineOut, fadeInTime: 0.2f, scale: 0.9f);

        RoguemojiGame.Instance.RevealScroll(ScrollType, user.GridPos, user.CurrentLevelId);

        Destroy();
    }

    public static bool CanGainSentience(Thing thing)
    {
        if (thing is Hole)
            return false;

        return true;
    }
}
