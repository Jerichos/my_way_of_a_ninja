using System;
using System.Diagnostics;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] Rigidbody Rb { get; set; }
	[Property] BoxCollider Collider { get; set; }
	[Property] TagSet GroundTags { get; set; }

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
				var velocity = Rb.Velocity;
				velocity.z = 0;
				Rb.Velocity = velocity;
				
				Rb.ClearForces();
				
				// snap to ground point
				var pos = Rb.PhysicsBody.Position;
				pos.z = _groundPoint.z + 1;
				Rb.PhysicsBody.Position = pos;
				
				GroundedEvent?.Invoke();
			}

			Log.Info($"grounded {_grounded}");
		}
	}

	public Action GroundedEvent;
	
	protected override void OnFixedUpdate()
	{
		Grounded = CheckGrounded();
	}

	private bool CheckGrounded()
	{
		// if velocity is up, don't check for ground?
		if(Rb.Velocity.z > 0)
			return false;

		float skinWidth = 25;
		float length = Rb.Velocity.z * Time.Delta + skinWidth;
		
		Vector3 startPosition = Rb.PhysicsBody.Position + Vector3.Up * skinWidth;
		Vector3 endPosition = Rb.PhysicsBody.Position + Vector3.Down * length;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.WithAnyTags(GroundTags)
			.Run();

		if ( _groundHitResult.Hit )
		{
			_groundPoint = _groundHitResult.HitPosition;
			return true;
		}

		return false;
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
}
