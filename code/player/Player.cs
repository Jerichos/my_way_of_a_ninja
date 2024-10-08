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
	[Property] public bool Unkillable { get; set; }
	
	private int _defaultMaxHealth;
	
	public Action<int> HealthChangedEvent;
	public Action<int> MaxHealthChangedEvent;
	public Action RespawnEvent;

	private bool _isDead;
	public bool IsDead => _isDead;
	
	private float _gracePeriodTime = 1f;
	private float _gracePeriodTimer;
	private bool _graceFromDamage; // TODO: not nice but quick fix, so what
	
	public bool IsInGracePeriod => _gracePeriodTimer > 0;

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
		SwordAbility.HitEvent += OnSwordHit;
		OnFacingChanged(MotionCore.Facing);
	}

	private void OnSwordHit()
	{
		if(DashAbility.IsDashing)
			DashAbility.StopDash();
	}

	private void OnItemsChanged(Inventory inventory)
	{
		if (inventory.HasUpgrade(ItemType.MAX_HEALTH, out int value))
		{
			int prevMaxHealth = MaxHealth;
			MaxHealth = _defaultMaxHealth + value;
			int diff = MaxHealth - prevMaxHealth;
			MaxHealthChangedEvent?.Invoke(MaxHealth);
			if (diff > 0)
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
		if(_gracePeriodTimer > 0)
		{
			_gracePeriodTimer -= Time.Delta;
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
		if(_gracePeriodTimer > 0 || Unkillable)
			return;
		
		Health -= 1;
		HealthChangedEvent?.Invoke(Health);
		_gracePeriodTimer = _gracePeriodTime;
		_graceFromDamage = true;
		
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
	}
	
	private void OnTakeDamage()
	{
		Sound.Play(HitSound);
		HitEvent?.Invoke();
	}

	public void Kill()
	{
		if(Unkillable)
			return;
		
		if(_isDead || (_gracePeriodTimer > 0 && !_graceFromDamage))
			return;
		
		Health = 0;
		HealthChangedEvent?.Invoke(Health);
		_isDead = true;
		OnDeath();
	}
	
	private void OnDeath()
	{
		DashAbility?.CancelMotion();
		Sound.Play(DeathSound); // huh we don't need sound point?
		DeathEvent?.Invoke();
		MoveAbility?.SetInputX(0);
		
		MotionCore.Collider.Enabled = false;
		Enabled = false;
	}

	public void OnRespawn()
	{
		_gracePeriodTimer = _gracePeriodTime;
		_graceFromDamage = false;
		MotionCore.Collider.Enabled = true;
		
		_isDead = false;
		Inventory?.ResetPendingItems();
		Health = MaxHealth;
		
		Enabled = true;
		HealthChangedEvent?.Invoke(Health);
		RespawnEvent?.Invoke();
	}

	public void RestoreHealth()
	{
		Health = MaxHealth;
		HealthChangedEvent?.Invoke(Health);
	}
}

