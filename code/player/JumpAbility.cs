
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
	// [Property] private float JumpColliderHeight { get; set; } = 32f;
	
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
				if(MotionCore.Collisions.Up)
					Log.Info("Jump collision up");
				if(MotionCore.Collisions.Down)
					Log.Info("Jump collision down");
				
				float targetHeight = HeightCurve.Evaluate(_t) * MaxHeight * _wishT;
				float heightDiff = targetHeight - _distanceTraveled;

				if ( heightDiff < 0 )
				{
					Velocity = Velocity.WithY(0);
					heightDiff = 0;
				}
				else
				{
					Velocity = new Vector2(0, heightDiff / Time.Delta);
					_distanceTraveled += heightDiff;
				}
				
				Log.Info("jump velocity: " + Velocity.y);
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
		
		Log.Info("start jump");
		
		IsJumping = true;
		_t = 0;
		_distanceTraveled = 0;
		_wishT = (MinHeight / MaxHeight) * 0.5f;
		_jumps++;
		
		// change collider size
		// float diff = MotionCore.DefaultColliderSize.y - JumpColliderHeight;
		// Collider.Scale = Collider.Scale.WithY(JumpColliderHeight);
		// Collider.Center = Collider.Center.WithY(Collider.Center.y + diff / 2);

		JumpSound.Pitch = BasePitch + PitchPerJump * _jumps;
		Sound.Play( JumpSound );		
	}

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
		Log.Info("cancel jump Now: " + Time.Now);
		
		Velocity = Vector2.Zero;
		MotionCore.RemoveMotionProvider(this);
		MotionCore.ResetCollider();
		
		Log.Info("1");
		MotionCore.HandleVerticalCollisions();
		Log.Info("2");
		
		IsJumping = false;
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
