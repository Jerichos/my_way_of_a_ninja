using System;
using Sandbox.enemies;
using SpriteTools;

namespace Sandbox.player;

public class SwordAbility : Component
{
	[Property] private Player Player { get; set; }
	[Property] private int Damage { get; set; } = 1;
	[Property] private int DashDamage { get; set; } = 2;
	
	[Property] private float Range { get; set; } = 150;
	[Property] private float DashRange { get; set; } = 200;
	[Property] private float Cooldown { get; set; }= 0.2f;
	[Property] private TagSet AttackTags { get; set; }
	[Property] private SoundEvent AttackSound { get; set; }
	
	private MotionCore2D MotionCore => Player.MotionCore;
	private SpriteComponent Sprite => Player.SpriteComponent;
	
	private float _cooldownTimer;
	
	public bool IsAttacking { get; private set; }
	
	public Action<bool> AttackEvent;
	private SceneTraceResult _hitResult;
	private Vector2 _rayStart;
	private Vector2 _rayEnd;
	private readonly BBox _bbox = BBox.FromPositionAndSize( Vector3.Zero, new Vector3( 5, 5, 5 ));

	private bool _isHitting;

	private readonly List<IHittable> _hitTargets = new();

	public void StartAttack()
	{
		if(!CanAttack())
			return;
		
		Log.Info("start attack");
		
		_cooldownTimer = Cooldown;
		
		IsAttacking = true;
		_isHitting = true;
		_hitTargets.Clear();

		Sound.Play( AttackSound, Transform.Position );
		AttackEvent?.Invoke(true);
	}
	
	private void EndAttack(SpriteComponent obj)
	{
		Log.Info("end attack");
		_isHitting = false;
		IsAttacking = false;
		AttackEvent?.Invoke(IsAttacking);
	}

	protected override void OnFixedUpdate()
	{
		if(_isHitting)
		{
			TryHit();
		}
	}

	private void TryHit()
	{
		// Log.Info("try hit");
		IsAttacking = true;

		// var swordStart = Sprite.GetAttachmentTransform( "swordStart" ).Position; // can't create from Prefab of SpriteComponent maybe it's fixed now...
		var swordStart = Sprite.Transform.Position + new Vector3(0, MotionCore.Collider.Scale.y / 1.66f, 0 );
		// Log.Info($"swordStart: {swordStart} collider height: {MotionCore.Collider.Scale.y / 1.66f}");
		
		float actualRange = Player.DashAbility?.IsDashing == true ? DashRange : Range;
		float length = actualRange * MotionCore.Facing;
		
		_rayStart = swordStart;
		_rayEnd = _rayStart + new Vector2(length, 0);
		
		_hitResult = Scene.Trace
			.Ray(_rayStart, _rayEnd)
			.Size(_bbox)
			.WithAnyTags(AttackTags)
			.Run();
		
		// Log.Info($"rayStart: {_rayStart} rayEnd: {_rayEnd} hit: {_hitResult.Hit}");
		
		if(_hitResult.Hit)
		{
			if(_hitResult.GameObject.Components.TryGet(out IHittable hittable))
			{
				if(_hitTargets.Contains(hittable))
					return;

				var damage = Damage;
				if ( Player.DashAbility?.IsDashing == true )
					damage = DashDamage;
				
				hittable.Hit(damage, OnHitSound, GameObject);
				_hitTargets.Add(hittable);
			}
			else
			{
				Log.Error("sword hit! but no IHittable component found: " + _hitResult.GameObject.Name);
			}
			
			Log.Info("sword hit! reduce health hit: " + _hitResult.GameObject.Name);
		}
		else
		{
			
		}
	}

	private void OnHitSound( SoundEvent soundEvent )
	{
		if ( soundEvent == null )
		{
			Components.Get<SoundPointComponent>().SoundEvent = AttackSound;
			Components.Get<SoundPointComponent>().StartSound();
		}
		else
		{
			Components.Get<SoundPointComponent>().SoundEvent = soundEvent;
			Components.Get<SoundPointComponent>().StartSound();
		}
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineThickness = 15;
		Gizmo.Draw.Line( _rayStart, _rayEnd );
		Gizmo.Draw.LineBBox(_bbox);

		if ( _hitResult.Hit )
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.LineThickness = 15;
			Gizmo.Draw.Line( _rayStart, _rayEnd );
			Gizmo.Draw.LineBBox( _bbox );
		}
	}

	protected override void OnUpdate()
	{
		if(_cooldownTimer > 0)
		{
			_cooldownTimer -= Time.Delta;
		}
	}

	protected override void OnEnabled()
	{
		Sprite.OnAnimationComplete += OnAnimationComplete;
		Sprite.BroadcastEvents["EndAttack"] += EndAttack;
	}

	private void OnAnimationComplete( string obj )
	{
		if(obj.Contains("attack"))
		{
			EndAttack(Sprite);
		}
	}

	private bool CanAttack()
	{
		return _cooldownTimer <= 0;
	}
}
