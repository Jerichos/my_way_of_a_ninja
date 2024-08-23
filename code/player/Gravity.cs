using System;

namespace Sandbox.player;


public sealed class Gravity : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] float GravityForce { get; set; } = 9.8f;
	[Property] float MaxVelocity { get; set; } = 10;
	
	[Property] Curve GravityCurve { get; set; } // apply gravity force portion between current and max velocity
	
	public Vector2 Velocity { get; private set; }
	public MotionType MotionType => MotionType.GRAVITY;
	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	
	protected override void OnFixedUpdate()
	{
		// apply gravity force until max velocity of gravity direction is reached
		if ( MotionCore.Grounded || MotionCore.Velocity.y > 0 )
		{
			// Log.Info("gravity ignored ground true or velocity.y > 0");
			Velocity = Vector2.Zero;
			return;
		}
		
		// auto dot = Vector2.Dot(MotionCore.Velocity, GravityDirection);
		if(Velocity.y < MaxVelocity)
		{
			var portion = Math.Abs(Velocity.y / MaxVelocity);
			var t = GravityCurve.Evaluate(portion);
			var gravityForce = t * GravityForce;
			Velocity += Vector2.Down * gravityForce;
			// Log.Info($"Gravity -> portion: {portion} t: {t} gravityForce: {gravityForce} velocity: {Velocity.y} MaxVelocity: {MaxVelocity}");
		}
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

	protected override void OnAwake()
	{
		MotionCore.AddMotionProvider(this);
	}
}
