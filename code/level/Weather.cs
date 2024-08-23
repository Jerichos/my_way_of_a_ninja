using System;
using Sandbox.player;

namespace Sandbox.level;

public class Weather : Component, IMotionProvider
{
	public Vector2 Velocity { get; private set; }
	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.ENVIRONMENT;

	[Property] ParticleEffect WheaterEffect { get; set; } 
	[Property] public int Direction = 1; // 0 means no force
	[Property] public float Speed = 100;
	[Property] public float ChangeInterval = 5;
	
	private float _timer;
	private int _right = 1;
	
	public void AddToPlayer(MotionCore2D motionCore)
	{
		motionCore.AddMotionProvider(this);
	}
	
	public void RemoveFromPlayer(MotionCore2D motionCore)
	{
		motionCore.RemoveMotionProvider(this);
	}
	
	public void SetDirection(int direction)
	{
		Direction = Math.Clamp(direction, -1, 1);
		WheaterEffect.ForceDirection = new Vector3( 5000 * Direction, -5000, 0 );
		_timer = 0;
	}

	protected override void OnFixedUpdate()
	{
		_timer += Time.Delta;
		if(_timer >= ChangeInterval)
		{
			// set randomly
			// SetDirection(Random.Shared.Next( -1, 2));
			
			SetDirection(Direction + _right);
			if(Direction > 0 || Direction < 0)
				_right *= -1;
		}
		
		Velocity = new Vector2(Direction * Speed, 0);
	}

	public void CancelMotion()
	{
		Velocity = Vector2.Zero;
		Enabled = false;
	}

	public void OnMotionRestored()
	{
		Enabled = true;
	}
}
