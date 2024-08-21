using System;
using Sandbox;
using Sandbox.player;
using SpriteTools;

public sealed class Player : Component
{
	[Property] private SpriteComponent SpriteComponent { get; set; }
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private MoveAbility MoveAbility { get; set; }
	[Property] private JumpAbility JumpAbility { get; set; }
	[Property] private DashAbility DashAbility { get; set; }
	[Property] private SwordAbility SwordAbility { get; set; }
	[Property] private Knockback Knockback { get; set; }

	// [Property] public SAttribute<int> Health { get; private set; } = 3;
	// [Property] public SAttribute<int> MaxHealth { get; private set; } = 3;
	
	[Property] public int Health { get; set; } = 3;
	[Property] public int MaxHealth { get; set; } = 3;
	
	public Action<int> HealthChangedEvent;
	public Action<int> MaxHealthChangedEvent;

	private bool _dead;

	public Action DeathEvent;
	
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
		DeathEvent?.Invoke();
	}

	public void OnRespawn()
	{
		_dead = false;
		Health = MaxHealth;
	}
}

