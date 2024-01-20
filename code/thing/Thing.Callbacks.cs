using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguemoji;

public partial class Thing : Entity
{
	/// <summary> Thing may be null. </summary>
	public virtual void OnWieldThing( Thing thing )
	{
		foreach ( var component in ThingComponents ) { component.OnWieldThing( thing ); }
		Brain?.OnWieldThing( thing );
	}

	public virtual void OnNoLongerWieldingThing( Thing thing ) { foreach ( var component in ThingComponents ) { component.OnNoLongerWieldingThing( thing ); } }

	public virtual void OnWieldedBy( Thing thing )
	{
		ThingWieldingThis = thing;
		foreach ( var component in ThingComponents ) { component.OnWieldedBy( thing ); }
	}

	public virtual void OnNoLongerWieldedBy( Thing thing )
	{
		if ( thing == ThingWieldingThis )
			ThingWieldingThis = null;

		foreach ( var component in ThingComponents ) { component.OnNoLongerWieldedBy( thing ); }
	}

	public virtual void OnChangedStat( StatType statType, int changeCurrent, int changeMin, int changeMax )
	{
		if ( statType == StatType.Speed && GetComponent<CActing>( out var acting ) )
		{
			((CActing)acting).ActionDelay = CActing.CalculateActionDelay( GetStatClamped( StatType.Speed ) );
		}
		else if ( statType == StatType.Intelligence && HasStat( StatType.Mana ) )
		{
			int amount = changeCurrent * 1;
			AdjustStatMax( StatType.Mana, amount );
		}
		else if ( statType == StatType.Stamina && HasStat( StatType.Energy ) )
		{
			int amount = changeCurrent * 2;
			AdjustStatMax( StatType.Energy, amount );
			StaminaDelay = Utils.Map( GetStatClamped( StatType.Stamina ), 0, 20, 3f, 0.1f );
		}
		else if ( statType == StatType.Invisible && GetStatClamped( StatType.SightBlockAmount ) > 0 )
		{
			int currInvis = GetStatClamped( StatType.Invisible );
			int oldInvis = currInvis - changeCurrent;

			// if we've actually changed invisibility state (anything >0 is invisible)
			if ( currInvis == 0 || oldInvis == 0 )
				ContainingGridManager?.CheckPlayerVisionChange( this, GridPos, PlayerVisionChangeReason.ChangedInvisibleAmount );
		}
		else if ( statType == StatType.SightBlockAmount )
		{
			ContainingGridManager?.CheckPlayerVisionChange( this, GridPos, changeCurrent > 0 ? PlayerVisionChangeReason.IncreasedSightBlockAmount : PlayerVisionChangeReason.DecreasedSightBlockAmount );
		}

		StatHash = 0;
		foreach ( var pair in Stats )
			StatHash += pair.Value.HashCode;

		foreach ( var component in ThingComponents )
			component.OnChangedStat( statType, changeCurrent, changeMin, changeMax );

		Brain?.OnChangedStat( statType, changeCurrent, changeMin, changeMax );
	}

	public virtual void OnSpawned()
	{

	}

	public virtual void OnEquipThing( Thing thing ) { foreach ( var component in ThingComponents ) { component.OnEquipThing( thing ); } }
	public virtual void OnUnequipThing( Thing thing ) { foreach ( var component in ThingComponents ) { component.OnUnequipThing( thing ); } }
	public virtual void OnEquippedTo( Thing thing ) { foreach ( var component in ThingComponents ) { component.OnEquippedTo( thing ); } }
	public virtual void OnUnequippedFrom( Thing thing ) { foreach ( var component in ThingComponents ) { component.OnUnequippedFrom( thing ); } }
	public virtual void OnActionRecharged()
	{
		foreach ( var component in ThingComponents ) { component.OnActionRecharged(); }
		Brain?.OnActionRecharged();
	}
	public virtual void OnWieldedThingBumpedOther( Thing thing, Direction direction )
	{
		foreach ( var component in ThingComponents ) { component.OnWieldedThingBumpedOther( thing, direction ); }
		Brain?.OnWieldedThingBumpedOther( thing, direction );
	}
	public virtual void OnBumpedIntoThing( Thing thing, Direction direction )
	{
		foreach ( var component in ThingComponents ) { component.OnBumpedIntoThing( thing, direction ); }
		Brain?.OnBumpedIntoThing( thing, direction );
	}
	public virtual void OnBumpedIntoBy( Thing thing, Direction direction ) // thing may be null
	{
		foreach ( var component in ThingComponents ) { component.OnBumpedIntoBy( thing, direction ); }
		Brain?.OnBumpedIntoBy( thing, direction );
	}
	public virtual void OnBumpedOutOfBounds( Direction direction )
	{
		foreach ( var component in ThingComponents ) { component.OnBumpedOutOfBounds( direction ); }
		Brain?.OnBumpedOutOfBounds( direction );
	}
	public virtual void OnMovedOntoThing( Thing thing )
	{
		foreach ( var component in ThingComponents ) { component.OnMovedOntoThing( thing ); }
	}
	public virtual void OnMovedOntoBy( Thing thing )
	{
		for ( int i = ThingComponents.Count - 1; i >= 0; i-- ) { ThingComponents.ElementAt( i ).OnMovedOntoBy( thing ); }
	}
	public virtual void OnChangedGridPos()
	{
		foreach ( var component in ThingComponents ) { component.OnChangedGridPos(); }
		Brain?.OnChangedGridPos();
	}
	public virtual void OnMove( Direction direction )
	{
		foreach ( var component in ThingComponents ) { component.OnMove( direction ); }
		Brain?.OnMove( direction );
	}
	public virtual void OnAddComponent( TypeDescription type ) { foreach ( var component in ThingComponents ) { component.OnAddComponent( type ); } }
	public virtual void OnRemoveComponent( TypeDescription type ) { foreach ( var component in ThingComponents ) { component.OnRemoveComponent( type ); } }
	public virtual void OnUseThing( Thing thing )
	{
		for ( int i = ThingComponents.Count - 1; i >= 0; i-- ) { ThingComponents.ElementAt( i ).OnUseThing( thing ); }
		Brain?.OnUseThing( thing );
	}
	public virtual void OnCooldownStart() { foreach ( var component in ThingComponents ) { component.OnCooldownStart(); } }
	public virtual void OnCooldownFinish() { foreach ( var component in ThingComponents ) { component.OnCooldownFinish(); } }
	public virtual void OnFindTarget( Thing target )
	{
		foreach ( var component in ThingComponents ) { component.OnFindTarget( target ); }
		Brain?.OnFindTarget( target );
	}
	public virtual void OnLoseTarget()
	{
		foreach ( var component in ThingComponents ) { component.OnLoseTarget(); }
		Brain?.OnLoseTarget();
	}
	public virtual void OnPlayerChangedGridPos( RoguemojiPlayer player ) { foreach ( var component in ThingComponents ) { component.OnPlayerChangedGridPos( player ); } }
	public virtual void OnTakeDamageFrom( Thing thing )
	{
		foreach ( var component in ThingComponents ) { component.OnTakeDamageFrom( thing ); }
		Brain?.OnTakeDamageFrom( thing );
	}
	public virtual void OnHurt( int amount )
	{
		foreach ( var component in ThingComponents ) { component.OnHurt( amount ); }
		Brain?.OnHurt( amount );
	}
	public virtual void OnDestroyed()
	{
		foreach ( var component in ThingComponents ) { component.OnThingDestroyed(); }
		Brain?.OnDestroyed();
	}
	public virtual void OnDied() { foreach ( var component in ThingComponents ) { component.OnThingDied(); } }
}
