using Sandbox;
using System;

namespace Roguemoji;

public class Client : Component, IClient
{
	public Connection Connection;

	public string Name { get; set; }
	public GameObject Pawn { get; set; }
	public RoguemojiPlayer PlayerComponent { get; set; }
	public Guid ConnectionId => Connection.Id;
}

public interface IClient
{
	public string Name { get; set; }
	public GameObject Pawn { get; set; }
	public RoguemojiPlayer PlayerComponent { get; set; }
	public Guid ConnectionId => Guid.Empty;
}

public struct To
{
	public Guid ConnectionId { get; private set; }
	public bool SendToAll = false;
	public static To Single( IClient client )
	{
		return new To { ConnectionId = client.ConnectionId };
	}
	public static To Single( Entity ent )
	{
		//Log.Info( $"Entity: {ent}" );
		//Log.Info( $"Client: {ent.Client}" );
		return new To { ConnectionId = ent.Client.ConnectionId };
	}
	public static To All => new To { SendToAll = true };

	public To( Guid id, bool sendToAll = false )
	{
		ConnectionId = id;
		SendToAll = sendToAll;
	}
}

public static class DebugOverlay
{
	public static void Line( Vector3 from, Vector3 to, Color color, float duration, bool ignoreDepth )
	{
		var d = Gizmo.Draw;
		d.Color = color;
		d.Line( from, to );
	}

	public static void ScreenText( string text, Vector2 pos, int line, Color color, float duration )
	{
		var d = Gizmo.Draw;
		d.ScreenText( text, pos );
	}
}

public static class ConnectionExtensions
{
	public static RoguemojiPlayer Pawn( this Connection self )
	{
		return RoguemojiGame.Clients[self.Id].PlayerComponent;
	}
}
