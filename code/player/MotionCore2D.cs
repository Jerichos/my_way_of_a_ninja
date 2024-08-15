using System;
using System.Diagnostics;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] Rigidbody Rb { get; set; }
	[Property] BoxCollider Collider { get; set; }
	
	[Property] TagSet GroundTags { get; set; }
	[Property] TagSet WallTags { get; set; }

	public Vector3 Velocity => Rb.Velocity;
	
	private Vector3 _groundPoint;
	
	private SceneTraceResult _groundHitResult;
	
	private bool _grounded;
	public bool Grounded 
	{
		get => _grounded;
		private set
		{
			if(_grounded == value)
				return;
			
			_grounded = value;

			if ( _grounded )
			{
				// var velocity = Rb.Velocity;
				// velocity.z = 0;
				// Rb.Velocity = velocity;
				
				// Rb.ClearForces();
				
				// snap to ground point
				// var pos = Rb.PhysicsBody.Position;
				// pos.z = _groundPoint.z + 5;
				// Rb.PhysicsBody.Position = pos;
				
				GroundHitEvent?.Invoke();
			}

			Log.Info($"grounded {_grounded}");
		}
	}

	public Action GroundHitEvent;
	public Action CeilingHitEvent;
	
	public Action<bool> HitCeilingMadafaka;
	public CollisionData Collisions;
	
	private float _skinPortion = 0.95f;

	protected override void OnUpdate()
	{
		if(_grounded && Rb.Velocity.z < 0)
			Rb.Velocity = Rb.Velocity.WithZ(0);
	}

	protected override void OnFixedUpdate()
	{
		Collisions.Reset();
		HandleHorizontalCollisions();
		HandleVerticalCollisions();
	}

	private void HandleVerticalCollisions()
	{
		CheckCollisionDown();
		CheckCollisionUp();
	}

	private void HandleHorizontalCollisions()
	{
		float skin = (Collider.Scale.x - Collider.Scale.x * _skinPortion) / 2;
		Vector3 scale = Collider.Scale * _skinPortion;
		
		float length = Rb.Velocity.x * Time.Delta + skin;
		
		if(Rb.Velocity.x > 0) // check right
		{
			Vector3 rayStart = Rb.PhysicsBody.Position + Collider.Center + Collider.Scale.x /2;
			Vector3 rayEnd = rayStart + Vector3.Forward * length;
			
			var hitResult = Scene.Trace
				.Ray(rayStart, rayEnd)
				.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(0, 0, scale.z)))
				.WithAnyTags(WallTags)
				.Run();
			
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( rayStart, rayEnd );
			
			if(hitResult.Hit)
			{
				Collisions.Right = true;
				
				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.LineThickness = 5;
				Gizmo.Draw.Line( rayStart, hitResult.HitPosition);
			}
			
			Log.Info($"skin: {skin} length: {length} scale: {scale} hit: {hitResult.Hit}");
		}
		else if ( Rb.Velocity.x < 0 ) // check left
		{
			Vector3 rayStart = Rb.PhysicsBody.Position + Collider.Center + Collider.Scale.x /2;
			Vector3 rayEnd = rayStart + Vector3.Right * length;
			
			var hitResult = Scene.Trace
				.Ray(rayStart, rayEnd)
				.Size(BBox.FromPositionAndSize(Vector3.Zero, Collider.Scale))
				.WithAnyTags(WallTags)
				.Run();
			
			if(hitResult.Hit)
			{
				Collisions.Left = true;
			}
		}
	}

	private void CheckCollisionUp()
	{
		if(Rb.Velocity.z < 0)
			return;
		
		float skinWidth = 25;
		float length = Rb.Velocity.z * Time.Delta + skinWidth;
		
		Vector3 startPosition = Rb.PhysicsBody.Position + Collider.Scale * Vector3.Up + Vector3.Down * skinWidth;
		Vector3 endPosition = startPosition + Vector3.Up * length;
		
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineThickness = 5;
		Gizmo.Draw.Line( startPosition, endPosition );
		
		var hitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(Collider.Scale.x, 1, 1)))
			.WithAnyTags(GroundTags)
			.Run();

		if ( hitResult.Hit )
		{
			Rb.Velocity = Rb.Velocity.WithZ(0);
			CeilingHitEvent?.Invoke();
			HitCeilingMadafaka?.Invoke(true);
			Collisions.Up = true;
		}
	}

	private void CheckCollisionDown()
	{
		// if velocity is up, don't check for ground?
		if ( Rb.Velocity.z > 0 )
		{
			Grounded = false;
			return;
		}

		float skinWidth = 25;
		float length = Rb.Velocity.z * Time.Delta + skinWidth + 5;
		
		Vector3 startPosition = Rb.PhysicsBody.Position + Vector3.Up * skinWidth;
		Vector3 endPosition = Rb.PhysicsBody.Position + Vector3.Down * length;
		
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineThickness = 5;
		Gizmo.Draw.Line( startPosition, endPosition );
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(Collider.Scale.x * _skinPortion, 1, 1)))
			.WithAnyTags(GroundTags)
			.Run();

		if ( _groundHitResult.Hit )
		{
			_groundPoint = _groundHitResult.HitPosition;
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line(startPosition, _groundHitResult.HitPosition);
			Collisions.Down = true;
			Grounded = true;
		}
		else
		{
			Grounded = false;
		}
	}

	protected override void DrawGizmos()
	{
		if(_groundHitResult.Hit)
		{
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( Rb.PhysicsBody.Position, _groundHitResult.HitPosition );
		}
		else
		{
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( Rb.PhysicsBody.Position, Rb.PhysicsBody.Position + Vector3.Down * 10 );
		}
	}
	
	public void ApplyHorizontalForce(float force)
	{
		Rb.ApplyForce(new Vector3(force, 0, 0));
	}
	
	public void ApplyHorizontalImpulse(float impulse)
	{
		Rb.ApplyImpulse(new Vector3(impulse, 0, 0));
	}
}

public struct CollisionData
{
	public bool Left;
	public bool Right;
	public bool Up;
	public bool Down;

	public void Reset()
	{
		Left = false;
		Right = false;
		Up = false;
		Down = false;
	}
}
