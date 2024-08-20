
using System;

namespace Sandbox.player;

public sealed class JumpAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }

	[Property][Range(1, 10)] private int MaxJumps { get; set; } = 1;
	[Property] private float MaxHeight { get; set; } = 200f;
	[Property] private float MinHeight { get; set; } = 100f;
	[Property] private float MaxVelocity { get; set; } = 100f; // apply force to jump
	[Property] private float JumpIn { get; set; } = 0.5f; // time to reach max height
	
	[Property] Curve VelocityCurve { get; set; }
	[Property] SoundEvent JumpSound { get; set; }
	[Property] float BasePitch { get; set; } = 1;
	[Property] float PitchPerJump { get; set; } = 0.1f;
	
	public Vector2 Velocity { get; private set; }
	public MotionType MotionType => MotionType.JUMP;
	public MotionType[] OverrideMotions => new[] {MotionType.DASH, MotionType.GRAVITY}; // Jump overrides dash and gravity
	
	
	public bool IsJumping { get; private set; }
	
	private float _wishHeight;
	private float _distanceTraveled;
	private float _t;

	private bool _increaseHeight;
	
	private int _jumps; // resets when grounded
	
	protected override void OnUpdate()
	{
		if(Input.Pressed("Jump") && CanJump())
		{
			Log.Info("jump pressed");
			StartJump();
		}

		_increaseHeight = false;
		if(Input.Down("Jump") && IsJumping)
		{
			_increaseHeight = true;
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if(IsJumping)
		{
			if(_increaseHeight)
				_wishHeight += MaxHeight * (Time.Delta / (JumpIn/2.75f)); // you have x times more time to reach max height
			
			_t += Time.Delta / JumpIn;
			_distanceTraveled += MotionCore.Velocity.y * Time.Delta;
			
			if(_distanceTraveled < Math.Clamp(_wishHeight, MinHeight, MaxHeight))
			{
				// Ensure _t doesn't exceed 1 (end of the dash)
				if (_t > 1)
				{
					_t = 1;
				}

				// Evaluate the curve based on normalized time
				float curveVelocity = VelocityCurve.Evaluate(_t);

				// Calculate the velocity required to reach the MaxDistance in the given dashIn time
				float requiredVelocity = MaxHeight / JumpIn;

				// Actual velocity is scaled by the curve
				var velocity = requiredVelocity * curveVelocity;

				// Update the distance traveled
				_distanceTraveled += velocity * Time.Delta;

				// Check if the dash should be completed
				if (_distanceTraveled >= MaxHeight)
				{
					_distanceTraveled = MaxHeight;
					_t = 1;  // Ensure the dash finishes
				}
				
				// Apply the velocity in the direction the character is facing
				Velocity = Util.UpY * velocity;
				Log.Info($"jump _t: {_t} distance: {_distanceTraveled} _wishHeight: {_wishHeight} clamped: {Math.Clamp(_wishHeight, MinHeight, MaxHeight)}");
			}
			else
			{
				Log.Info($"jump end distance: {_distanceTraveled} _wishHeight: {_wishHeight} clamped: {Math.Clamp(_wishHeight, MinHeight, MaxHeight)}");
				CancelJump();
			}
		}
	}

	private void StartJump()
	{
		if ( IsJumping == false )
		{
			MotionCore.AddMotionProvider(this);
		}
		
		IsJumping = true;
		_t = 0;
		_distanceTraveled = 0;
		_wishHeight = 0;
		_jumps++;
		
		Components.Get<SoundPointComponent>().SoundOverride = true;
		Components.Get<SoundPointComponent>().SoundEvent = JumpSound;
		Components.Get<SoundPointComponent>().Pitch = BasePitch + PitchPerJump * _jumps;
		Components.Get<SoundPointComponent>().StartSound();
		Components.Get<SoundPointComponent>().SoundOverride = false;
		
		
		Log.Info($"Jump start! jumps: {_jumps}");
		
	}

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
		Log.Info("Jump canceled!");
		IsJumping = false;
		Velocity = Vector2.Zero;
		
		MotionCore.RemoveMotionProvider(this);
	}
	
	private bool CanJump()
	{
		return !IsJumping && MotionCore.Grounded || _jumps < MaxJumps;
	}
	
	public void OnMotionCanceled()
	{
		CancelJump();
	}
	
	public void OnMotionRestored()
	{
		
	}

	protected override void OnEnabled()
	{
		Log.Info("JumpAbility enabled");
		MotionCore.CeilingHitEvent += CancelJump;
		MotionCore.GroundHitEvent += () => _jumps = 0;
	}
	
	protected override void OnDisabled()
	{
		Log.Info("JumpAbility disabled");
		MotionCore.CeilingHitEvent -= CancelJump;
		MotionCore.GroundHitEvent -= () => _jumps = 0;
	}

	
}
