using System;
using Sandbox.player;

namespace Sandbox.enemies;

// move on platform from edge to edge
public class MoveLeftRight : Component, IMotionProvider
{
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private float Speed { get; set; } = 100;
	public Vector2 Velocity { get; private set; }

	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.MOVE;
	
	private int _direction = 1;
	
	public void CancelMotion()
	{
		Velocity = new Vector2(0, 0);
		Enabled = false;
	}

	public void OnMotionRestored()
	{
		Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		// if there is ground or wall on direction change direction
		if (MotionCore.Collisions.Right || MotionCore.Collisions.Left)
		{
			_direction *= -1;
		}
		
		if(!MotionCore.GroundEdgeCheck(_direction))
		{
			_direction *= -1;
		}
		
		if(MotionCore.Grounded)
			Velocity = new Vector2(_direction * Speed, 0);
		else
			Velocity = new Vector2(0, 0);
	}

	protected override void OnAwake()
	{
		MotionCore.AddMotionProvider(this);
	}
}
