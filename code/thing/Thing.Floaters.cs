﻿using Sandbox;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public struct ThingFloaterData
{
    public string icon;
    public float time;
    public TimeSince timeSinceStart;
    public string text;
    public bool requireSight;
    public bool alwaysShowWhenAdjacent;
    public Vector2 offsetStart;
    public Vector2 offsetEnd;
    public float height;
    public EasingType offsetEasingType;
    public float fadeInTime;
    public float scale;
    public float opacity;
    public float shakeAmount;
    public bool moveToGridOnDeath;
    public bool showOnInvisible;
    public bool showOnSeen;

    public ThingFloaterData(string icon, float time, Vector2 offsetStart, Vector2 offsetEnd, float height, string text, bool requireSight, bool alwaysShowWhenAdjacent,
        EasingType offsetEasingType, float fadeInTime, float scale, float opacity, float shakeAmount, bool moveToGridOnDeath, bool showOnInvisible, bool showOnSeen)
    {
        this.icon = icon;
        this.time = time;
        this.timeSinceStart = 0f;
        this.offsetStart = offsetStart;
        this.offsetEnd = offsetEnd;
        this.height = height;
        this.text = text;
        this.requireSight = requireSight;
        this.alwaysShowWhenAdjacent = alwaysShowWhenAdjacent;
        this.offsetEasingType = offsetEasingType;
        this.fadeInTime = fadeInTime;
        this.scale = scale;
        this.opacity = opacity;
        this.shakeAmount = shakeAmount;
        this.moveToGridOnDeath = moveToGridOnDeath;
        this.showOnInvisible = showOnInvisible;
        this.showOnSeen = showOnSeen;
    }
}

public partial class Thing : Entity
{
    public bool HasFloaters => Floaters != null && Floaters.Count > 0; // Client-only
    public List<ThingFloaterData> Floaters { get; private set; } // Client-only

    public void HandleFloaters(float dt)
    {
        if (!HasFloaters)
            return;

        for (int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];
            if (floater.time > 0f && floater.timeSinceStart > floater.time)
                Floaters.RemoveAt(i);
        }
    }

    public void AddFloater(string icon, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false, EasingType offsetEasingType = EasingType.Linear,
        float fadeInTime = 0f, float scale = 1f, float opacity = 1f, float shakeAmount = 0f, bool moveToGridOnDeath = false, bool showOnInvisible = false, bool showOnSeen = false)
    {
        AddFloaterClient(icon, time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, shakeAmount, moveToGridOnDeath, showOnInvisible, showOnSeen);
    }

    // //[TargetedRPC]
    public void AddFloaterClient(string icon, float time, Vector2 offsetStart, Vector2 offsetEnd, float height = 0f, string text = "", bool requireSight = true, bool alwaysShowWhenAdjacent = false, EasingType offsetEasingType = EasingType.Linear,
        float fadeInTime = 0f, float scale = 1f, float opacity = 1f, float shakeAmount = 0f, bool moveToGridOnDeath = false, bool showOnInvisible = false, bool showOnSeen = false)
    {
        if (Floaters == null)
            Floaters = new List<ThingFloaterData>();

        Floaters.Add(new ThingFloaterData(icon, time, offsetStart, offsetEnd, height, text, requireSight, alwaysShowWhenAdjacent, offsetEasingType, fadeInTime, scale, opacity, shakeAmount, moveToGridOnDeath, showOnInvisible, showOnSeen));
    }

    public void RemoveFloater(string icon)
    {
        RemoveFloaterClient(icon);
    }

    // //[TargetedRPC]
    public void RemoveFloaterClient(string icon)
    {
        if (!HasFloaters)
            return;

        //Log.Info($"{DisplayIcon} RemoveFloaterClient - Floaters.Count: {Floaters.Count}");

        for (int i = Floaters.Count - 1; i >= 0; i--)
        {
            var floater = Floaters[i];
            //Log.Info($"{i}: {floater.icon}");

            if (floater.icon.Equals(icon))
                Floaters.RemoveAt(i);
        }

        //Log.Info($"{DisplayIcon} RemoveFloaterClient - Floaters.Count: {Floaters.Count}");
    }

    // //[TargetedRPC]
    public void DestroyFloatersClient()
    {
        if (ContainingGridManager == null || Floaters == null)
            return;

        foreach (var floater in Floaters)
        {
            if (floater.moveToGridOnDeath)
            {
                ContainingGridManager.Floaters.Add(
                    new GridFloaterData(floater.icon, GridPos, floater.time, floater.timeSinceStart, floater.offsetStart, floater.offsetEnd, floater.height, floater.text,
                    floater.requireSight, floater.alwaysShowWhenAdjacent, floater.offsetEasingType, floater.fadeInTime, floater.scale, floater.opacity, floater.shakeAmount)
                );
            }
        }

        Floaters.Clear();
    }

    /// <summary> Whether an invisible thing has a floater that should still be drawn. </summary>
    public bool ShowFloaterOnInvisible()
    {
        Game.AssertClient();

        if (!HasFloaters)
            return false;

        foreach (var floater in Floaters)
        {
            if (floater.showOnInvisible)
                return true;
        }

        return false;
    }

    public static float GetFloaterOpacity(ThingFloaterData floater)
    {
        return floater.timeSinceStart < floater.fadeInTime
            ? Utils.Map(floater.timeSinceStart, 0f, floater.fadeInTime, 0f, floater.opacity, EasingType.SineIn)
            : (floater.time > 0f ? Utils.Map(floater.timeSinceStart, floater.fadeInTime, floater.time, floater.opacity, 0f, floater.offsetEasingType) : floater.opacity);
    }
}
