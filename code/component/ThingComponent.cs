﻿using Sandbox;
using System;

namespace Roguemoji;

public abstract class ThingComponent : Sandbox.Component
{
    public Thing Thing { get; private set; }

    public bool ShouldUpdate { get; protected set; }

    public float TimeElapsed { get; protected set; }
    public bool IsClientComponent { get; protected set; }

	protected override void OnAwake()
	{
		Thing = GameObject.Components.Get<Thing>();
		ShouldUpdate = false;
		TimeElapsed = 0f;
	}

	public virtual void Init(Thing thing)
    {
    }

	protected override void OnUpdate()
    {
		var dt = Time.Delta;
        TimeElapsed += dt;
        //if (IsClientComponent == Game.IsServer)
        //{
        //    Log.Error(GetType().Name + " IsClientComponent: " + IsClientComponent + " IsServer: " + Game.IsServer + "!");
        //}
    }

    // component was added when already existing
    public virtual void ReInitialize()
    {
        TimeElapsed = 0f;
    }

    public virtual void Remove()
    {
		Destroy();
    }

    public virtual void OnWieldThing(Thing thing) { }
    public virtual void OnNoLongerWieldingThing(Thing thing) { }
    public virtual void OnWieldedBy(Thing thing) { }
    public virtual void OnNoLongerWieldedBy(Thing thing) { }
    public virtual void OnEquipThing(Thing thing) { }
    public virtual void OnUnequipThing(Thing thing) { }
    public virtual void OnEquippedTo(Thing thing) { }
    public virtual void OnUnequippedFrom(Thing thing) { }
    public virtual void OnActionRecharged() { }
    public virtual void OnWieldedThingBumpedOther(Thing thing, Direction direction) { }
    public virtual void OnBumpedIntoThing(Thing thing, Direction direction) { }
    public virtual void OnBumpedIntoBy(Thing thing, Direction direction) { }
    public virtual void OnBumpedOutOfBounds(Direction direction) { }
    public virtual void OnMovedOntoThing(Thing thing) { }
    public virtual void OnMovedOntoBy(Thing thing) { }
    public virtual void OnChangedStat(StatType statType, int changeCurrent, int changeMin, int changeMax) { }
    public virtual void OnChangedGridPos() { }
    public virtual void OnMove(Direction direction) { }
    public virtual void OnAddComponent(TypeDescription type) { }
    public virtual void OnRemoveComponent(TypeDescription type) { }
    public virtual void OnUseThing(Thing thing) { }
    public virtual void OnCooldownStart() { }
    public virtual void OnCooldownFinish() { }
    public virtual void OnFindTarget(Thing target) { }
    public virtual void OnLoseTarget() { }
    public virtual void OnPlayerChangedGridPos(RoguemojiPlayer player) { }
    public virtual void OnTakeDamageFrom(Thing thing) { }
    public virtual void OnHurt(int amount) { }
    public virtual void OnRemove() { }
    public virtual void OnThingDestroyed() { }
    public virtual void OnThingDied() { }
}
