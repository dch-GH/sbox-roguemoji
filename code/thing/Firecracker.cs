﻿using Sandbox;
using Sandbox.Internal;
using System;
using System.Linq;

namespace Roguemoji;
public partial class Firecracker : Thing
{
    public override string AbilityName => "Light Firecracker";
    public Trait AbilityTrait { get; private set; }
    [Net] public bool IsLit { get; private set; }
    private float _fuseSfxTimer;
    private float FUSE_SFX_DELAY = 0.5f;
    private int _numFuseSfx;

    public override string ChatDisplayIcons => $"🧨{(IsLit ? Globals.Icon(IconType.Burning) : "")}";

    public Firecracker()
	{
		DisplayIcon = "🧨";
        DisplayName = "Firecracker";
        Description = "Stuns anyone who sees it explode";
        Tooltip = "A firecracker";
        IconDepth = (int)IconDepthLevel.Normal;
        Flags = ThingFlags.Selectable | ThingFlags.CanBePickedUp | ThingFlags.Useable;
        Flammability = 24;

        ShouldUpdate = true;

        if (Game.IsServer)
        {
            AbilityTrait = AddTrait(AbilityName, "🧨", $"Ignite the fuse", offset: new Vector2(0f, -1f), tattooIcon: Globals.Icon(IconType.Burning), tattooScale: 0.6f, tattooOffset: new Vector2(9f, -9f), isAbility: true);
        }
    }

    public override void Use(Thing user)
    {
        if (IsLit)
            return;

        PlaySfx("firecracker_light", loudness: 3);

        IsLit = true;

        RemoveFlag(ThingFlags.Useable);
        RemoveTrait(AbilityTrait);

        DisplayName = "Lit Firecracker";
        Tooltip = "A lit firecracker";

        var burning = AddComponent<CBurning>();
        burning.Lifetime = 5f;

        ShouldUpdate = true;
        
        base.Use(user);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if(IsLit && !IsRemoved)
        {
            _fuseSfxTimer += dt;
            if (_fuseSfxTimer > FUSE_SFX_DELAY)
            {
                PlaySfx("firecracker_fuse", loudness: 2, pitch: Utils.Map(_numFuseSfx++, 0, 5, 1f, 1.33f, EasingType.QuadIn));
                _fuseSfxTimer -= FUSE_SFX_DELAY;
            }
        }
    }

    public override void GetSound(SoundActionType actionType, SurfaceType surfaceType, out string sfxName, out int loudness)
    {
        switch (actionType)
        {
            case SoundActionType.Use:
                sfxName = "trumpet";
                loudness = 9;
                return;
        }

        base.GetSound(actionType, surfaceType, out sfxName, out loudness);
    }

    public override void OnDestroyed()
    {
        if(HasComponent<CBurning>())
        {
            PlaySfx("firecracker_explode", loudness: 9);
            ContainingGridManager.AddFloater(Globals.Icon(IconType.Explosion), GridPos, 1f, new Vector2(0f, 0f), new Vector2(0f, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.Linear, fadeInTime: 0.01f, scale: 1.3f, opacity: 0.6f, shakeAmount: 0f);
            ContainingGridManager.AddFloater("🔆", GridPos, 0.4f, new Vector2(0f, 0f), new Vector2(0f, -4f), height: 0f, text: "", requireSight: true, alwaysShowWhenAdjacent: false, EasingType.QuadOut, fadeInTime: 0.01f, scale: 1f, opacity: 1f, shakeAmount: 2f);

            if (ContainingGridManager.GridType == GridType.Inventory && ThingWieldingThis == null)
            {
                StunThing(ContainingGridManager.OwningPlayer.ControlledThing);
            }
            else
            {
                var gridManager = ThingWieldingThis != null ? ThingWieldingThis.ContainingGridManager : ContainingGridManager;
                var gridPos = ThingWieldingThis != null ? ThingWieldingThis.GridPos : GridPos;

                var things = gridManager.GetAllThings().Where(x => x.HasStat(StatType.Sight)).ToList();
                foreach (var thing in things)
                {
                    if (thing == this)
                        continue;

                    bool didSeeExplosion = false;
                    var sight = thing.GetStatClamped(StatType.Sight);
                    if (thing.HasBrain && thing.Brain is RoguemojiPlayer player)
                    {
                        didSeeExplosion = thing.CanSeeGridPos(gridPos, sight) && player.IsGridPosOnCamera(gridPos);

                        if (didSeeExplosion)
                        {
                            player.PlaySfxUI("firecracker_tinnitus");
                            player.PlaySfxUI("firecracker_explode", volume: 1f, pitch: 1.25f);
                        }
                    }
                    else
                    {
                        didSeeExplosion = thing.CanSeeGridPos(gridPos, sight);
                    }

                    if (didSeeExplosion)
                        StunThing(thing);
                }
            }
        }

        base.OnDestroyed();
    }

    void StunThing(Thing thing)
    {
        var stunned = thing.AddComponent<CStunned>();
        stunned.Lifetime = 5f;
    }
}
