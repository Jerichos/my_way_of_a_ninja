using System;

namespace Sandbox.player;

public enum MotionType
{
	GRAVITY,
	JUMP,
	MOVE,
	DASH,
	ENVIRONMENT,
	KNOCKBACK,
	CLIMB,
	ALL,
}

public interface IMotionProvider
{
	public Vector2 Velocity { get; }
	
	public MotionType[] OverrideMotions { get; }
	public MotionType MotionType { get; }
	
	public void CancelMotion();
	
	public void OnMotionRestored();
}
