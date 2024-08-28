
using System;

namespace Sandbox.player;

public sealed class JumpAbility : Component, IMotionProvider
{
	[Property] private Player Player { get; set; }
	private MotionCore2D MotionCore => Player.MotionCore;

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
	// private float _colliderFactor = 1.5f;
	
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
				float targetHeight = HeightCurve.Evaluate(_t) * MaxHeight * _wishT;
				float heightDiff = targetHeight - _distanceTraveled;
				Velocity = new Vector2(0, heightDiff / Time.Delta);
				_distanceTraveled += heightDiff;
				
			}
			else
			{
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
		_jumps++;
		
		// change collider size
		Collider.Scale = _defaultColliderScale.WithY(32);
		Collider.Center = _defaultColliderCenter.WithY(24);

		JumpSound.Pitch = BasePitch + PitchPerJump * _jumps;
		Sound.Play( JumpSound );		
	}

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
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
	
	public void CancelMotion()
	{
		CancelJump();
	}
	
	public void OnMotionRestored()
	{
		
	}

	protected override void OnEnabled()
	{
		MotionCore.CeilingHitEvent += CancelJump;
		MotionCore.GroundedEvent += OnGroundedChanged;
		
		Player.RespawnEvent += OnRespawn;
		OnRespawn();
	}
	
	protected override void OnDisabled()
	{
		MotionCore.CeilingHitEvent -= CancelJump;
		MotionCore.GroundedEvent -= OnGroundedChanged;
		
		if ( Player.Inventory != null )
			Player.Inventory.AddedItemEvent -= OnUnlockedUpgradesChanged;
	}

	private void OnRespawn()
	{
		if ( Player.Inventory != null )
		{
			Player.Inventory.AddedItemEvent += OnUnlockedUpgradesChanged;
			OnUnlockedUpgradesChanged(Player.Inventory);
		}
	}

	private void OnUnlockedUpgradesChanged( Inventory upgrades )
	{
		if(Player.Inventory.Enabled == false)
			return;
		
		MaxJumps = 1;
		if ( upgrades.HasUpgrade(ItemType.DOUBLE_JUMP, out var value))
		{
			MaxJumps = 2;
		}
	}

	private void OnGroundedChanged( bool grounded )
	{
		if(grounded)
		{
			_jumps = 0;
			// o("grounded jumps reset");
		}
		else
		{
			_jumps = 1;
		}
	}
}
