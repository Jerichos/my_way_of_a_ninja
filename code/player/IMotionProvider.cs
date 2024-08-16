using System;

namespace Sandbox.player;

public enum MotionType
{
	GRAVITY,
	JUMP,
	MOVE,
	DASH,
	ENVIRONMENT,
}

public interface IMotionProvider
{
	public Vector2 Velocity { get; }
	public int Priority { get; } // only with same or higher priority will be processed
	public void OnVelocityIgnored();
	public bool Additive { get; }
	public MotionType MotionType { get; }
}
