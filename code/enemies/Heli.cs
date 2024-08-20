using Sandbox.player;
using SpriteTools;

namespace Sandbox.enemies;

// Heli is a spider enemy that moves back and forth on a platform, from edge to edge.
public class Heli : Component, IHittable
{
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private SpriteComponent Sprite { get; set; }

	[Property] private int Health { get; set; } = 1;
	[Property] private int MaxHealth { get; set; } = 1;

	private bool _dead;
	
	private float _hitFadeTime = 0.1f;
	private float _flashAlpha;
	private float _hitFadeTimer;
	
	private bool _hitThisFrame;
	
	protected override void OnFixedUpdate()
	{
		if(_hitThisFrame)
		{
			_hitFadeTimer = _hitFadeTime;
			_hitThisFrame = false;
		}
		
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
		}
	}
	
	public void Hit( int damage )
	{
		_hitThisFrame = true;
		Health -= damage;
		if(Health <= 0)
			Kill();
		
		Log.Info($"Heli hit! Health: {Health}/{MaxHealth}");
	}
	
	private void Kill()
	{
		if(_dead)
			return;
		
		_dead = true;
		OnDeath();
	}

	private void OnDeath()
	{
		
	}
}
