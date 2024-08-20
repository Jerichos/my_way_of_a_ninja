using System;
using SpriteTools;

namespace Sandbox.player;

public class SwordAbility : Component
{
	[Property] public MotionCore2D MotionCore { get; set; }
	[Property] public SpriteComponent Sprite;
	[Property] public int Damage { get; set; } = 1;
	
	[Property] public float Width { get; set; } = 200;
	[Property] public float Height { get; set; } = 100;
	
	[Property] public TagSet AttackTags { get; set; }
	
	public bool IsAttacking { get; private set; }
	
	public Action<bool> AttackEvent;
	
	public void StartAttack()
	{
		if(!CanAttack())
			return;
		
		Log.Info("start attack");
		IsAttacking = true;
		AttackEvent?.Invoke(true);
	}
	
	private void EndAttack(SpriteComponent obj)
	{
		Log.Info("end attack");
		IsAttacking = false;
		AttackEvent?.Invoke(false);
	}

	private void TryHit(SpriteComponent obj)
	{
		Log.Info("try hit");
		IsAttacking = true;

		Vector3 scale = new Vector3(100, 100, 10);
		var bbox = BBox.FromPositionAndSize( Vector3.Zero, new Vector3( 1, scale.y, 1 ));
		float halfWidth = scale.x / 2;
		
		float length = halfWidth * MotionCore.Facing;
		Vector2 rayStart = Transform.Position + new Vector3(0, 25, 0);
		Vector2 rayEnd = rayStart + new Vector2(length, 0);
		
		var hitResult = Scene.Trace
			.Ray(rayStart, rayEnd)
			.Size(bbox)
			.WithAnyTags(AttackTags)
			.Run();
		
		Log.Info($"rayStart: {rayStart} rayEnd: {rayEnd} hit: {hitResult.Hit}");
		
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineThickness = 5;
		Gizmo.Draw.Line( rayStart, rayEnd );
		Gizmo.Draw.LineBBox(bbox);
		
		if(hitResult.Hit)
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( rayStart, rayEnd );
			Gizmo.Draw.LineBBox(bbox);
			Log.Info("sword hit! reduce health hit: " + hitResult.GameObject.Name);
		}
	}

	protected override void OnEnabled()
	{
		Sprite.BroadcastEvents["TryHit"] = TryHit;
		Sprite.BroadcastEvents["EndAttack"] = EndAttack;
		
		// print all broadcast events
		foreach(var e in Sprite.BroadcastEvents)
		{
			Log.Info($"broadcast event: {e.Key}");
		}
	}

	private bool CanAttack()
	{
		return !IsAttacking;
	}
}
