﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;
public partial class BookOrganize : Thing
{
    [Net] public int ManaCost { get; private set; }
    [Net] public int ReqInt { get; private set; }
    public float CooldownTime { get; private set; }

    public override string ChatDisplayIcons => $"📘{Globals.Icon(IconType.Organize)}";

    public BookOrganize()
	{
		DisplayIcon = "📘";
        DisplayName = "Book of Organize";
        Description = "Organizes the items below your hotbar";
        Tooltip = "A book of Organize";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;

        SetTattoo(Globals.Icon(IconType.Organize), scale: 0.5f, offset: new Vector2(0.5f, -4f), offsetWielded: new Vector2(0f, 0f), offsetInfo: new Vector2(1f, -1f), offsetCharWielded: new Vector2(2.5f, -6f), offsetInfoWielded: new Vector2(-1f, -2f));

        if (Game.IsServer)
        {
            ManaCost = 1;
            ReqInt = 3;
            CooldownTime = 10f;

            AddTrait(AbilityName, "📖", $"Spend {GetStatIcon(StatType.Mana)} to cast the spell Organize", offset: new Vector2(0f, -2f), tattooIcon: Globals.Icon(IconType.Organize), tattooScale: 0.7f, tattooOffset: new Vector2(0f, -5f), isAbility: true);
            AddTrait("", GetStatIcon(StatType.Mana), $"{ManaCost}{GetStatIcon(StatType.Mana)} used to cast spell", offset: new Vector2(0f, -3f), labelText: $"{ManaCost}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", GetStatIcon(StatType.Intelligence), Globals.GetStatReqString(StatType.Intelligence, ReqInt, VerbType.Read), offset: new Vector2(0f, -1f), labelText: $"≥{ReqInt}", labelFontSize: 16, labelOffset: new Vector2(0f, 0f), labelColor: new Color(1f, 1f, 1f));
            AddTrait("", "⏳", $"Cooldown time: {CooldownTime}s", offset: new Vector2(0f, -2f), labelText: $"{CooldownTime}", labelFontSize: 16, labelOffset: new Vector2(0f, 1f), labelColor: new Color(1f, 1f, 1f));
        }
    }

    public override bool CanBeUsedBy(Thing user, bool ignoreResources = false, bool shouldLogMessage = false)
    {
        if (user is RoguemojiPlayer p && p.IsConfused)
        {
            if (shouldLogMessage)
                RoguemojiGame.Instance.LogPersonalMessage(p, $"{Globals.Icon(IconType.Confusion)}Too confused to read books!");

            return false;
        }

        var intelligence = user.GetStatClamped(StatType.Intelligence);
        if (intelligence < ReqInt)
        {
            if (shouldLogMessage && user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ReqInt}{GetStatIcon(StatType.Intelligence)} to use {ChatDisplayIcons} but you only have {intelligence}{GetStatIcon(StatType.Intelligence)}");

            return false;
        }

        var mana = user.GetStatClamped(StatType.Mana);
        if(mana < ManaCost && !ignoreResources)
        {
            if (shouldLogMessage && user is RoguemojiPlayer player)
                RoguemojiGame.Instance.LogPersonalMessage(player, $"You need {ManaCost}{GetStatIcon(StatType.Mana)} to use {ChatDisplayIcons} but you only have {mana}{GetStatIcon(StatType.Mana)}");

            return false;
        }

        return true;
    }

    public override void Use(Thing user)
    {
        if (!user.TrySpendStat(StatType.Mana, ManaCost))
            return;

        var player = user as RoguemojiPlayer;
        if (player == null)
            return;

        player.AddComponent<COrganize>();

        StartCooldown(CooldownTime);

        base.Use(user);
    }
}