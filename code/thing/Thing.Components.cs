using Sandbox;
using System;

namespace Roguemoji;

public partial class Thing : Entity
{
	//public List<ThingComponent> ThingComponents => GameObject.Components.GetAll<ThingComponent>().ToList();
	public List<ThingComponent> ThingComponents = new();

	public T AddComponent<T>() where T : ThingComponent
	{
		if ( Components.TryGet<T>( out var existing ) )
		{
			existing.ReInitialize();
			return existing;
		}

		var compType = TypeLibrary.GetType( typeof( T ) );
		var component = Components.Create( compType );
		if ( component is not ThingComponent thingComponent )
		{
			Log.Error( "AddComponent<T> failed!" );
			return null;
		}

		thingComponent.Init( this );
		OnAddComponent( compType );
		ThingComponents.Add( thingComponent );
		return thingComponent as T;
	}

	public bool RemoveComponent<T>() where T : ThingComponent
	{
		if ( Components.TryGet<T>( out var component ) && component is ThingComponent thingComponent )
		{
			thingComponent.OnRemove();
			OnRemoveComponent( TypeLibrary.GetType( typeof( T ) ) );
			ThingComponents.Remove( thingComponent );
			component.Destroy();
			return true;
		}

		return false;
	}

	public bool GetComponent<T>( out ThingComponent component ) where T : ThingComponent
	{
		component = null;
		if ( Components.TryGet<T>( out var c ) && c is ThingComponent thingComponent )
		{
			component = thingComponent;
			return true;
		}

		return false;
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

	//[TargetedRPC]
	public void VfxNudge( Direction direction, float lifetime, float distance )
	{
		RemoveMoveVfx();

		var nudge = AddComponent<VfxNudge>();
		nudge.Direction = direction;
		nudge.Lifetime = lifetime;
		nudge.Distance = distance;
	}

	//[TargetedRPC]
	public void VfxSlide( Direction direction, float lifetime, float distance )
	{
		RemoveMoveVfx();

		var slide = AddComponent<VfxSlide>();
		slide.Direction = direction;
		slide.Lifetime = lifetime;
		slide.Distance = distance;
	}

	//[TargetedRPC]
	public void VfxShake( float lifetime, float distance )
	{
		var shake = AddComponent<VfxShake>();
		shake.Lifetime = lifetime;
		shake.Distance = distance;
	}

	//[TargetedRPC]
	public void VfxScale( float lifetime, float startScale, float endScale )
	{
		var scale = AddComponent<VfxScale>();
		scale.Lifetime = lifetime;
		scale.StartScale = startScale;
		scale.EndScale = endScale;
	}

	//[TargetedRPC]
	public void VfxSpin( float lifetime, float startAngle, float endAngle )
	{
		var scale = AddComponent<VfxSpin>();
		scale.Lifetime = lifetime;
		scale.StartAngle = startAngle;
		scale.EndAngle = endAngle;
	}

	//[TargetedRPC]
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

	//[TargetedRPC]
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
