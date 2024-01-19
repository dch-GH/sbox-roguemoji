using Sandbox;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public class Entity : Sandbox.Component
{
	public string Name;
	public TransmitType Transmit;
	public virtual void Spawn() {}
	public virtual void OnClientActive( IClient client ) { }
	public virtual void Simulate( IClient client ) { }

	//public virtual void Update( float dt ) { }
	public virtual void Delete() { }

	public int NetworkIdent => System.HashCode.Combine( GameObject.Network.OwnerId );
	public IClient Client;

	public static Entity FindByIndex( int index )
	{
		return null;
	}

	public static List<Entity> All => null;
}

