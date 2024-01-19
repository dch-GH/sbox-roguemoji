using Sandbox;

namespace Roguemoji;

public static class SoundExtensions
{
	public static SoundHandle Play( To to, string name, float x, float y )
	{
		return Sound.Play( name, new Vector3( x, y, 0 ) );
	}

	public static SoundHandle Play( To to, string name, Vector3 vec )
	{
		return Sound.Play( name, vec );
	}
}
