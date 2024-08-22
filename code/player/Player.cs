using System;
using Sandbox.player;
using SpriteTools;

public sealed class Player : Component
{
	[Property] public MotionCore2D MotionCore { get; set; }
	[Property] public SpriteComponent SpriteComponent { get; set; }
	[Property] public MoveAbility MoveAbility { get; set; }
	[Property] public JumpAbility JumpAbility { get; set; }
	[Property] public DashAbility DashAbility { get; set; }
	[Property] public SwordAbility SwordAbility { get; set; }
	[Property] public CrouchAbility CrouchAbility { get; set; }
	[Property] public Knockback Knockback { get; set; }

	[Property] private SoundEvent HitSound { get; set; }
	[Property] private SoundEvent DeathSound { get; set; }
	
	[Property] public int Health { get; set; } = 3;
	[Property] public int MaxHealth { get; set; } = 3;
	
	public Action<int> HealthChangedEvent;
	public Action<int> MaxHealthChangedEvent;

	private bool _dead;

	public Action DeathEvent;
	public Action HitEvent;
	
	protected override void OnEnabled()
	{
		MotionCore.FacingChangedEvent += OnFacingChanged;
		OnFacingChanged(MotionCore.Facing);
	}

	protected override void OnUpdate()
	{
		HandleInput();
	}

	private void HandleInput()
	{
		if(Input.Pressed("Jump"))
		{
			JumpAbility.Jump();
		}
		else if ( Input.Released("Jump") )
		{
			JumpAbility.StopJump();
		}
		
		if(Input.Pressed("Run"))
		{
			DashAbility.StartDash();
		}
		
		if(Input.Pressed("attack1"))
		{
			SwordAbility.StartAttack();
		}
		
		if(Input.Down("Crouch"))
		{
			CrouchAbility.StartCrouch();
		}
		else if ( Input.Released("Crouch") )
		{
			CrouchAbility.StopCrouch();
		}
	}

	private void OnFacingChanged( int facing)
	{
		SpriteComponent.SpriteFlags = facing == 1 ? SpriteFlags.None : SpriteFlags.HorizontalFlip;
	}
	
	public void Teleport(Vector3 position)
	{
		MotionCore.Teleport(position);
	}

	public void TakeDamage( int contactDamage, Component fromComponent) // position is used for knockback
	{
		Health -= 1;
		HealthChangedEvent?.Invoke(Health);
		
		if(Health <= 0)
		{
			Kill();
		}
		else
		{
			Vector2 knockbackDirection = (MotionCore.Transform.Position - fromComponent.Transform.Position).Normal;
			Knockback.Activate(knockbackDirection);
			OnTakeDamage();
		}
		
		Log.Info($"Player took damage! Health: {Health}/{MaxHealth} damage:{contactDamage}  from: {fromComponent.GameObject.Name}");
	}
	
	private void OnTakeDamage()
	{
		Sound.Play(HitSound);
		HitEvent?.Invoke();
	}

	public void Kill()
	{
		if(_dead)
			return;
		
		_dead = true;
		OnDeath();
	}
	
	private void OnDeath()
	{
		Log.Info("Player died");
		Sound.Play(DeathSound); // huh we don't need sound point?
		DeathEvent?.Invoke();
	}

	public void OnRespawn()
	{
		_dead = false;
		Health = MaxHealth;
	}
}

