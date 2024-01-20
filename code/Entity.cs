using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class Entity : Sandbox.Component
{
	public string Name;
	public virtual void Spawn() { }
	public virtual void OnClientActive( IClient client ) { }
	public virtual void Simulate( IClient client ) { }

	public virtual void Delete() { }

	public Guid NetworkIdent => GameObject.Id;
	public IClient Client;

	public Entity FindByIndex( Guid index )
	{
		var scene = RoguemojiGame.Instance.Scene;
		Log.Info( scene );
		var go = scene.Directory.FindByGuid( index );
		Log.Info( $"Found: {go}" );
		if ( go.Components.TryGet<Entity>( out var ent ) )
			return ent;

		return null;
	}

	public static List<Entity> All => null;
}

