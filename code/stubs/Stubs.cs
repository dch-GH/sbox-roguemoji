using Sandbox;
using System;

namespace Roguemoji;

public class Client : Sandbox.Component, IClient
{
	public Connection Connection;

	public string Name { get; set; }
	public Entity Pawn { get; set; }
	public Guid ConnectionId => Connection.Id;
}

public interface IClient
{
	public string Name { get; set; }
	public Entity Pawn { get; set; }
	public Guid ConnectionId => Guid.Empty;
}

public static class Event
{
	public class Tick
	{
		public class ClientAttribute : System.Attribute { }
		public class ServerAttribute : System.Attribute { }
	}
}

public enum TransmitType
{
	Never,
	Always
}

public struct To
{
	private IClient _to;
	public Guid ConnectionId { get; private set; }
	public bool SendToAll = false;
	public static To Single( IClient client )
	{
		return new To { _to = client };
	}
	public static To Single( Entity ent )
	{
		return new To { _to = ent.Client };
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
	public static Entity Pawn( this Connection self )
	{
		return GameManager.Clients[self.Id].Pawn;
	}
}
