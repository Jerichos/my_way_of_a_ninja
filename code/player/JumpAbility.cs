
using System;

namespace Sandbox.player;

public sealed class JumpAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }

	[Property][Range(1, 10)] private int MaxJumps { get; set; } = 1;
	[Property] private float MaxHeight { get; set; } = 200f;
	[Property] private float MinHeight { get; set; } = 100f;
	[Property] private float JumpIn { get; set; } = 0.5f; // time to reach max height
	
	[Property] Curve HeightCurve { get; set; } // in time at what height the jump should be
	[Property] SoundEvent JumpSound { get; set; }
	[Property] float BasePitch { get; set; } = 1;
	[Property] float PitchPerJump { get; set; } = 0.1f;
	
	public Vector2 Velocity { get; private set; }
	public MotionType MotionType => MotionType.JUMP;
	public MotionType[] OverrideMotions => new[] {MotionType.DASH, MotionType.GRAVITY}; // Jump overrides dash and gravity
	
	BoxCollider Collider => MotionCore.Collider;
	
	public bool IsJumping { get; private set; }
	
	private float _wishT;
	private float _distanceTraveled;
	private float _t;

	private bool _increaseHeight;
	
	private int _jumps; // resets when grounded
	private float _colliderFactor = 1.5f;
	
	private Vector3 _defaultColliderCenter;
	private Vector3 _defaultColliderScale;

	protected override void OnStart()
	{
		_defaultColliderCenter = Collider.Center;
		_defaultColliderScale = Collider.Scale;
	}

	public void Jump()
	{
		if(CanJump())
		{
			Log.Info("jump pressed");
			StartJump();
		}
		
		_increaseHeight = true;
	}

	public void StopJump()
	{
		_increaseHeight = false;
	}
	

	// for smoother jump experience, move _t calc to OnUpdate
	protected override void OnUpdate()
	{
		if ( IsJumping )
		{
			float increase = Time.Delta / JumpIn;
			_t += increase;
			
			if(_t > 1)
				_t = 1;
			
			if ( _increaseHeight )
			{
				increase *= 1.1f;
				_wishT += increase;
				if(_wishT > 1)
					_wishT = 1;
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		if(IsJumping)
		{
			if(_t < _wishT)
			{
				// Ensure _t doesn't exceed 1 (end of the dash)
				// if (_t > 1)
				// {
				// 	_t = 1;
				// }
				//
				// if(_t > _wishT)
				// 	_t = _wishT;

				float targetHeight = HeightCurve.Evaluate(_t) * MaxHeight * _wishT;
				float heightDiff = targetHeight - _distanceTraveled;
				Velocity = new Vector2(0, heightDiff / Time.Delta);
				_distanceTraveled += heightDiff;
				
				Log.Info($"jump _t: {_t} _wishT: {_wishT} targetHeight: {targetHeight} heightDiff: {heightDiff} velocity: {Velocity.y}");
			}
			else
			{
				Log.Info($"jump end distance: {_distanceTraveled} _wishHeight: {_wishT} _t: {_t}");
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
		_wishT = (MinHeight / MaxHeight) * 0.8f;
		// _wishT = 0;
		_jumps++;
		
		// change collider size
		Collider.Scale = _defaultColliderScale.WithY(Collider.Scale.y / _colliderFactor);
		Collider.Center = _defaultColliderCenter.WithY(Collider.Center.y / _colliderFactor);
		

		JumpSound.Pitch = BasePitch + PitchPerJump * _jumps;
		Sound.Play( JumpSound );		
	}

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
		Log.Info("Jump canceled!");
		Velocity = Vector2.Zero;
		IsJumping = false;

		Collider.Scale = _defaultColliderScale;
		Collider.Center = _defaultColliderCenter;
		
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
		MotionCore.GroundedEvent += OnGroundedChanged;
	}
	
	protected override void OnDisabled()
	{
		Log.Info("JumpAbility disabled");
		MotionCore.CeilingHitEvent -= CancelJump;
		MotionCore.GroundedEvent -= OnGroundedChanged;
	}

	private void OnGroundedChanged( bool grounded )
	{
		if(grounded)
		{
			_jumps = 0;
			Log.Info("grounded jumps reset");
		}
		else
		{
			_jumps = 1;
			Log.Info("not grounded jumps: " + _jumps);
		}
	}
}
