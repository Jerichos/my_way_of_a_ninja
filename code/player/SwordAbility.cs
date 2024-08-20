using System;
using Sandbox.enemies;
using SpriteTools;

namespace Sandbox.player;

public class SwordAbility : Component
{
	[Property] public MotionCore2D MotionCore { get; set; }
	[Property] public SpriteComponent Sprite;
	[Property] public int Damage { get; set; } = 1;
	
	[Property] private float Cooldown { get; set; }= 0.2f;
	[Property] public float Range { get; set; } = 50;
	[Property] public TagSet AttackTags { get; set; }
	
	private float _cooldownTimer;
	
	public bool IsAttacking { get; private set; }
	
	public Action<bool> AttackEvent;
	private SceneTraceResult _hitResult;
	private Vector2 _rayStart;
	private Vector2 _rayEnd;
	private readonly BBox _bbox = BBox.FromPositionAndSize( Vector3.Zero, new Vector3( 5, 5, 5 ));

	public void StartAttack()
	{
		if(!CanAttack())
			return;
		
		_cooldownTimer = Cooldown;
		
		Log.Info("start attack");
		TryHit();
		IsAttacking = true;
		AttackEvent?.Invoke(true);
	}
	
	private void EndAttack(SpriteComponent obj)
	{
		Log.Info("end attack");
		IsAttacking = false;
		AttackEvent?.Invoke(false);
	}

	private void TryHit()
	{
		Log.Info("try hit");
		IsAttacking = true;

		// var swordStart = Sprite.GetAttachmentTransform( "swordStart" ).Position; // can't create from Prefab...
		var swordStart = Sprite.Transform.Position + new Vector3(0, 120, 0 );
		Log.Info("swordStart: " + swordStart);

		float length = Range * MotionCore.Facing;
		_rayStart = swordStart;
		_rayEnd = _rayStart + new Vector2(length, 0);
		
		_hitResult = Scene.Trace
			.Ray(_rayStart, _rayEnd)
			.Size(_bbox)
			.WithAnyTags(AttackTags)
			.Run();
		
		Log.Info($"rayStart: {_rayStart} rayEnd: {_rayEnd} hit: {_hitResult.Hit}");
		
		if(_hitResult.Hit)
		{
			if(_hitResult.GameObject.Components.TryGet(out IHittable hittable))
			{
				hittable.Hit(Damage);
			}
			else
			{
				Log.Error("sword hit! but no IHittable component found: " + _hitResult.GameObject.Name);
			}
			
			Log.Info("sword hit! reduce health hit: " + _hitResult.GameObject.Name);
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
		Sprite.BroadcastEvents["EndAttack"] = EndAttack;
		
		// print all broadcast events
		foreach(var e in Sprite.BroadcastEvents)
		{
			Log.Info($"broadcast event: {e.Key}");
		}
	}

	private bool CanAttack()
	{
		return _cooldownTimer <= 0;
	}
}
