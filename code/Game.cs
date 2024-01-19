using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Network;
using System;
using System.Collections.Generic;

namespace Roguemoji;

public static class Game
{
	public static bool IsClient = GameNetworkSystem.IsClient;

	public static bool IsServer = GameNetworkSystem.IsHost;
	public static System.Random Random => System.Random.Shared;
	public static Component LocalPawn;

	public static void AssertServer()
	{
		Assert.True( GameNetworkSystem.IsHost );
	}

	public static void AssertClient()
	{
		Assert.True( GameNetworkSystem.IsClient );
	}
}

public partial class GameManager : Sandbox.Component
{
	public static Dictionary<Guid, Client> Clients;
	public virtual void ClientJoined( IClient client ) { }
	public virtual void ClientDisconnect( IClient client, NetworkDisconnectionReason reason ) { }
}

public enum NetworkDisconnectionReason
{
	None
}
