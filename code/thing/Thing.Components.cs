using Sandbox;
using System;

namespace Roguemoji;

public partial class Thing : Entity
{
	public List<ThingComponent> ThingComponents => GameObject.Components.GetAll<ThingComponent>().ToList();
	public ThingComponent AddComponent( TypeDescription type )
	{
		if ( type == null )
		{
			Log.Info( "type is null!" );
			return null;
		}

		var component = (ThingComponent)Components.Get( type.TargetType );
		if ( component is not null )
		{
			component.ReInitialize();
		}
		else
		{
			component = Components.Create<ThingComponent>( type );
			component.Init( this );
		}

		OnAddComponent( type );
		return component;
	}

	public T AddComponent<T>() where T : ThingComponent
	{
		return AddComponent( TypeLibrary.GetType( typeof( T ) ) ) as T;
	}

	public bool RemoveComponent( TypeDescription type )
	{
		var component = (ThingComponent)Components.Get( type.TargetType );
		if ( component is null )
		{
			component.OnRemove();
			OnRemoveComponent( type );
			component.Destroy();
			return true;
		}

		return false;
	}

	public bool RemoveComponent<T>() where T : ThingComponent
	{
		return RemoveComponent( TypeLibrary.GetType( typeof( T ) ) );
	}

	public bool GetComponent( TypeDescription type, out ThingComponent component )
	{
		component = (ThingComponent)Components.Get( type.TargetType );

		if ( component is not null )
		{
			return true;
		}

		component = null;
		return false;
	}

	public bool GetComponent<T>( out ThingComponent component ) where T : ThingComponent
	{
		return GetComponent( TypeLibrary.GetType( typeof( T ) ), out component );
	}

	public bool HasComponent( TypeDescription type )
	{
		var component = (ThingComponent)Components.Get( type.TargetType );
		return component is not null;
	}

	/// <summary> Server-only. </summary>
	public bool HasComponent<T>() where T : ThingComponent
	{
		return HasComponent( TypeLibrary.GetType( typeof( T ) ) );
	}

	[TargetedRPC]
	public void VfxNudge( Direction direction, float lifetime, float distance )
	{
		RemoveMoveVfx();

		var nudge = AddComponent<VfxNudge>();
		nudge.Direction = direction;
		nudge.Lifetime = lifetime;
		nudge.Distance = distance;
	}

	[TargetedRPC]
	public void VfxSlide( Direction direction, float lifetime, float distance )
	{
		RemoveMoveVfx();

		var slide = AddComponent<VfxSlide>();
		slide.Direction = direction;
		slide.Lifetime = lifetime;
		slide.Distance = distance;
	}

	[TargetedRPC]
	public void VfxShake( float lifetime, float distance )
	{
		var shake = AddComponent<VfxShake>();
		shake.Lifetime = lifetime;
		shake.Distance = distance;
	}

	[TargetedRPC]
	public void VfxScale( float lifetime, float startScale, float endScale )
	{
		var scale = AddComponent<VfxScale>();
		scale.Lifetime = lifetime;
		scale.StartScale = startScale;
		scale.EndScale = endScale;
	}

	[TargetedRPC]
	public void VfxSpin( float lifetime, float startAngle, float endAngle )
	{
		var scale = AddComponent<VfxSpin>();
		scale.Lifetime = lifetime;
		scale.StartAngle = startAngle;
		scale.EndAngle = endAngle;
	}

	[TargetedRPC]
	public void VfxFly( IntVector startingGridPos, float lifetime, float heightY = 0f, EasingType progressEasingType = EasingType.ExpoOut, EasingType heightEasingType = EasingType.QuadInOut )
	{
		RemoveMoveVfx();

		var fly = AddComponent<VfxFly>();
		fly.StartingGridPos = startingGridPos;
		fly.Lifetime = lifetime;
		fly.HeightY = heightY;
		fly.ProgressEasingType = progressEasingType;
		fly.HeightEasingType = heightEasingType;
	}

	[TargetedRPC]
	public void VfxOpacityLerp( float lifetime, float startOpacity, float endOpacity, EasingType easingType = EasingType.Linear )
	{
		var opacityLerp = AddComponent<VfxOpacityLerp>();
		opacityLerp.Lifetime = lifetime;
		opacityLerp.StartOpacity = startOpacity;
		opacityLerp.EndOpacity = endOpacity;
		opacityLerp.EasingType = easingType;
	}

	void RemoveMoveVfx()
	{
		RemoveComponent<VfxSlide>();
		RemoveComponent<VfxNudge>();
		RemoveComponent<VfxFly>();
	}
}
