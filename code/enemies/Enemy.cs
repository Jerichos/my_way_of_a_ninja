using System;
using Sandbox.level;
using Sandbox.player;
using SpriteTools;

namespace Sandbox.enemies;

// Heli is a spider enemy that moves back and forth on a platform, from edge to edge.
public class Enemy : Component, IHittable
{
	[Property] private SpriteComponent Sprite { get; set; }
	[Property] private Knockback Knockback { get; set; }
	[Property] private ContactDamage ContactDamage { get; set; }

	[Property] private int Health { get; set; } = 1;
	[Property] private int MaxHealth { get; set; } = 1;
	
	[Property] private SoundEvent HitSound { get; set; }
	[Property] private SoundEvent DestroySound { get; set; }
	[Property] public bool IgnoreRespawn { get; set; }

	public Action<int> HitEvent;
	private bool _dead;
	
	// hit animation
	private float _hitFadeTime = 0.1f;
	private float _flashAlpha;
	private float _hitFadeTimer;
	private bool _isHit;
	
	private Vector3 _initialPosition;
	
	protected override void OnStart()
	{
		ContactDamage = Components.Get<ContactDamage>();
		_initialPosition = Transform.Position;
		if (Components.TryGet(out Level level, FindMode.InAncestors))
		{
			level.RestartEvent += OnLevelRestart;
		}
		else
		{
			// TODO(bug): log is not invoked in OnAwake
			if(!IgnoreRespawn)
				Log.Warning("Enemy component should be a child of a Level component for respawn. " + GameObject);
		}
	}

	protected override void OnFixedUpdate()
	{
		if(!_isHit)
			return;
		
		if(_hitFadeTimer > 0)
		{
			_hitFadeTimer -= Time.Delta;
			_flashAlpha = _hitFadeTimer / _hitFadeTime;
			var color = Sprite.FlashTint;
			color.a = _flashAlpha;
			Sprite.FlashTint = color;
		}
		else
		{
			var color = Sprite.FlashTint;
			color.a = 0;
			Sprite.FlashTint = color;
			_isHit = false;
		}
	}
	
	public void Hit(int damage, Action<SoundEvent> soundCallback, GameObject source = null)
	{
		Health -= damage;
		if ( Health <= 0 )
		{
			Kill(source);
			soundCallback?.Invoke(DestroySound);
		}
		else
		{
			HitEvent?.Invoke(damage);
			_hitFadeTimer = _hitFadeTime;
			_isHit = true;
			soundCallback?.Invoke(HitSound);
		}
	}
	
	private void Kill(GameObject source = null)
	{
		if(_dead)
			return;
		
		_dead = true;
		OnDeath(source);
	}

	private void OnDeath(GameObject source = null)
	{
		if(ContactDamage != null)
			ContactDamage.Enabled = false;
		
		Color color = Sprite.FlashTint;
		color.a = 0.5f;
		Sprite.FlashTint = color;
		if ( source == null )
		{
			Knockback.Activate(Vector2.Zero);
		}
		else
		{
			float moreDistance = 0;

			if ( source.Components.TryGet( out MotionCore2D motionCore ) )
			{
				moreDistance = motionCore.Velocity.Length * 0.2f;
			}
			
			Vector2 knockbackDirection = (Transform.Position - source.Transform.Position).Normal;
			Knockback.Activate(knockbackDirection, moreDistance);
		}
	}

	private void OnLevelRestart()
	{
		OnRespawn();
	}

	private void OnRespawn()
	{
		if(ContactDamage != null)
			ContactDamage.Enabled = true;
		
		if(Knockback != null)
			Knockback.Enabled = false;
		
		Transform.Position = _initialPosition;
		Health = MaxHealth;
		_dead = false;
		GameObject.Enabled = true;
		var color = Sprite.FlashTint;
		color.a = 0;
		Sprite.FlashTint = color;
	}

	protected override void OnEnabled()
	{
		if(Knockback != null)
			Knockback.KnockbackEndEvent += ()=> GameObject.Enabled = false;
	}
	
	protected override void OnDisabled()
	{
		if(Knockback != null)
			Knockback.KnockbackEndEvent -= ()=> GameObject.Enabled = false;
	}
	
	
}
