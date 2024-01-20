using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Roguemoji;

public static class Game
{
	public static bool IsClient = true;

	public static bool IsServer = true;
	public static System.Random Random => System.Random.Shared;
	public static RoguemojiPlayer LocalPlayer = null;
	public static GameObject LocalPawn => RoguemojiGame.Clients.Values.ToArray()[0].Pawn;

	public static void AssertServer()
	{
	}

	public static void AssertClient()
	{
	}
}

public enum NetworkDisconnectionReason
{
	None
}
