
using System;

namespace Sandbox.player;

public sealed class JumpAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }

	[Property] private float MaxHeight { get; set; } = 200f;
	[Property] private float MinHeight { get; set; } = 100f;
	[Property] private float MaxVelocity { get; set; } = 100f; // apply force to jump
	
	[Property] Curve VelocityCurve { get; set; }
	
	public Vector2 Velocity { get; private set; }
	
	public bool IsJumping { get; private set; }

	private float _jumpHeight;
	private float _jumpHeightReached;
	
	protected override void OnUpdate()
	{
		if(Input.Pressed("Jump") && CanJump())
		{
			Log.Info("Jump start!");
			IsJumping = true;
			_jumpHeightReached = 0;
			_jumpHeight = 0;
		}
		
		if(Input.Down("Jump") && IsJumping)
		{
			_jumpHeight += 1000 * Time.Delta;
			Log.Info($"Jump height: {_jumpHeight}");
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if(IsJumping)
		{
			_jumpHeightReached += MotionCore.Velocity.y * Time.Delta;
			
			if(_jumpHeightReached < Math.Clamp(_jumpHeight, MinHeight, MaxHeight))
			{
				float t = VelocityCurve.Evaluate(_jumpHeightReached / MaxHeight);
				
				var velocity = MaxVelocity * t;
				Velocity = Vector2.Up * velocity;
				
				Log.Info($"JUMP UP t: {t} force: {velocity} velocity: {Velocity.y} heightReached: {_jumpHeightReached}");
			}
			else
			{
				Log.Info("jump height reached");
				IsJumping = false;
				Velocity = Vector2.Zero;
			}
		}
	}

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
		Log.Info("Jump canceled!");
		IsJumping = false;
		Velocity = Vector2.Zero;
	}
	
	private bool CanJump()
	{
		return !IsJumping && MotionCore.Grounded;
	}

	protected override void OnEnabled()
	{
		Log.Info("JumpAbility enabled");
		MotionCore.AddMotionProvider(this);
		MotionCore.CeilingHitEvent += CancelJump;
	}
	
	protected override void OnDisabled()
	{
		Log.Info("JumpAbility disabled");
		MotionCore.RemoveMotionProvider(this);
		MotionCore.CeilingHitEvent -= CancelJump;
	}

	
}
