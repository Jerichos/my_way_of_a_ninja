namespace Sandbox.player;

public static class Util
{
	public static readonly Vector3 DownY	= new(0.0f, -1.0f, 0.0f);
	public static readonly Vector3 UpY		= new(0.0f, 1.0f, 0.0f);
	public static readonly Vector3 RightX	= new(1.0f, 0.0f, 0.0f);
	public static readonly Vector3 LeftX	= new(-1.0f, 0.0f, 0.0f);

	public static Vector3 WorldScale( this BoxCollider collider )
	{
		return collider.Scale * collider.Transform.Scale;
	}

	public static Vector3 WorldCenter( this BoxCollider collider )
	{
		return collider.Transform.Position + collider.Center / 2;
	}
	
}
