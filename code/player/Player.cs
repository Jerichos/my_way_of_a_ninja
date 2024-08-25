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
	[Property] public Inventory Inventory { get; set; }

	[Property] private SoundEvent HitSound { get; set; }
	[Property] private SoundEvent DeathSound { get; set; }
	
	[Property] public int Health { get; set; } = 3; // TODO: UI this
	[Property] public int MaxHealth { get; set; } = 3;
	
	private int _defaultMaxHealth;
	
	public Action<int> HealthChangedEvent;
	public Action<int> MaxHealthChangedEvent;
	public Action RespawnEvent;

	private bool _dead;

	public Action DeathEvent;
	public Action HitEvent;

	protected override void OnAwake()
	{
		_defaultMaxHealth = MaxHealth;
	}

	protected override void OnEnabled()
	{
		MotionCore.FacingChangedEvent += OnFacingChanged;
		Inventory.AddedItemEvent += OnItemsChanged;
		OnFacingChanged(MotionCore.Facing);
	}

	private void OnItemsChanged( Inventory inventory )
	{
		Log.Info($"checking for upgrades");
		if ( inventory.HasUpgrade( ItemType.MAX_HEALTH, out int value ) )
		{
			Log.Info($"Max health upgrade: {value}");
			var prevMaxHealth = _defaultMaxHealth;
			MaxHealth = _defaultMaxHealth + value;
			int diff = MaxHealth - prevMaxHealth;
			Log.Info("diff: " + diff);
			MaxHealthChangedEvent?.Invoke(MaxHealth);
			if ( diff > 0 )
			{
				Health += diff;
				HealthChangedEvent?.Invoke(Health);
			}
		}
		else
		{
			MaxHealth = _defaultMaxHealth;
			MaxHealthChangedEvent?.Invoke(MaxHealth);
		}
	}

	protected override void OnDisabled()
	{
		MotionCore.FacingChangedEvent -= OnFacingChanged;
	}

	protected override void OnUpdate()
	{
		HandleInput();
	}

	private void HandleInput()
	{
		// TODO: controller support! Can't find a way to make it work with my ps5 controller...
		
		if(Input.Pressed("Jump") && !CrouchAbility.IsCrouching)
		{
			JumpAbility.Jump();
		}
		else if ( Input.Released("Jump") )
		{
			JumpAbility.StopJump();
		}
		
		if(Input.Pressed("Dash"))
		{
			DashAbility.StartDash();
		}
		
		if(Input.Pressed("attack1"))
		{
			SwordAbility.StartAttack();
		}
		
		if(Input.Down("Down"))
		{
			CrouchAbility.StartCrouch();
			MoveAbility.SetInputX(0);
		}
		else if ( Input.Released("Down") )
		{
			CrouchAbility.StopCrouch();
		}
		
		// if(Input.Down("Up"))
		// {
		// 	ClimbMovement.TryClimbUp();
		// }
		// else if(Input.Down("Down"))
		// {
		// 	ClimbMovement.TryClimbDown();
		// }
		
		if ( Input.Down( "Right" ))
		{
			if(!CrouchAbility.IsCrouching)
				MoveAbility.SetInputX(1);
			
			MotionCore.Facing = 1;
		}
		else if ( Input.Down( "Left" ))
		{
			if(!CrouchAbility.IsCrouching)
				MoveAbility.SetInputX(-1);
			
			MotionCore.Facing = -1;
		}
		else
		{
			MoveAbility.SetInputX(0);
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
		
		Health = 0;
		HealthChangedEvent?.Invoke(Health);
		_dead = true;
		OnDeath();
	}
	
	private void OnDeath()
	{
		Log.Info("Player died");
		DashAbility?.CancelMotion();
		Sound.Play(DeathSound); // huh we don't need sound point?
		DeathEvent?.Invoke();
		Enabled = false;
		MoveAbility?.SetInputX(0);
	}

	public void OnRespawn()
	{
		_dead = false;
		Enabled = true;
		Inventory?.ResetPendingItems();
		Health = MaxHealth;
		Log.Info("Health: " + Health + "/" + MaxHealth + " default: " + _defaultMaxHealth);
		HealthChangedEvent?.Invoke(Health);
		
		RespawnEvent?.Invoke();
	}

	public void RestoreHealth()
	{
		Health = MaxHealth;
		HealthChangedEvent?.Invoke(Health);
	}
}

