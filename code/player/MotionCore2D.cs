using System;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] public BoxCollider Collider { get; private set; }
	
	[Property] private TagSet GroundTags { get; set; }
	[Property] private TagSet WallTags { get; set; }
	
	public Vector2 Velocity { get; private set; }
	public Vector2 Size => Collider.Center * Transform.LocalScale;
	public Vector2 Center => Transform.Position + (Vector3)Size;
	
	private readonly List<IMotionProvider> _motionProviders = new();
	private readonly List<IMotionProvider> _activeProviders = new();

	private SceneTraceResult _groundHitResult;

	private Vector2 _groundPoint;
	
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
				Velocity = Velocity.WithY(0);
				
				// snap to ground point
				var pos = Transform.Position;
				pos.y = _groundPoint.y + 1;
				Transform.Position = pos;
			}
			
			GroundedEvent?.Invoke(_grounded);
		}
	}
	
	private int _facing = 1; // default is right
	public int Facing
	{
		get => _facing;
		set
		{
			if(_facing == value || value == 0)
				return;
			
			_facing = value;
			FacingChangedEvent?.Invoke(_facing);
		}
	}
	
	public Action<int> FacingChangedEvent;

	public Action<bool> GroundedEvent;
	public Action CeilingHitEvent;
	
	public Action<bool> HitCeilingMadafaka;
	public CollisionData Collisions;
	
	private float _skinPortion = 0.95f;
	private float _defaultColliderHeight;

	protected override void OnStart()
	{
		_defaultColliderHeight = Collider.Scale.y;
	}

	protected override void OnFixedUpdate()
	{
		// if contains type jump
		Collisions.Reset();
		CalculateVelocity();

		HandleHorizontalCollisions();
		HandleVerticalCollisions();
		
		Transform.Position += (Vector3)Velocity * Time.Delta;
	}

	private void HandleVerticalCollisions()
	{
		CheckCollisionDown();
		CheckCollisionUp();
	}

	// check if there is ground on the edge
	public bool GroundEdgeCheck(int direction)
	{
		Vector3 startPosition = Transform.Position + new Vector3(Collider.WorldScale().x / 2 * direction, 10,0);
		Vector3 endPosition = startPosition + Util.DownY * 20;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.WithAnyTags(GroundTags)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(1,1,1)))
			.Run();
		
		// Gizmo.Draw.Color = Color.Green;
		// Gizmo.Draw.LineThickness = 20;

		if ( _groundHitResult.Hit )
		{
			// Gizmo.Draw.Color = Color.Red;
			return true;
		}

		// Gizmo.Draw.LineThickness = 20;
		// Gizmo.Draw.Line( startPosition, endPosition );
		return false;
	}
	
	public bool CheckCollision(Vector2 from, Vector2 velocity)
	{
		Vector2 startPosition = from;
		Vector2 endPosition = startPosition + velocity * 20;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.WithAnyTags(GroundTags)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(1,1,1)))
			.Run();
		
		// Gizmo.Draw.Color = Color.Green;
		// Gizmo.Draw.LineThickness = 20;

		if ( _groundHitResult.Hit )
		{
			// Gizmo.Draw.Color = Color.Red;
			return true;
		}

		// Gizmo.Draw.LineThickness = 20;
		// Gizmo.Draw.Line( startPosition, endPosition );
		return false;
	}

	private void HandleHorizontalCollisions()
	{
		if(WallTags.IsEmpty)
			return;
		
		// float skin = 5;
		Vector3 scale = Collider.WorldScale() * _skinPortion;
		float halfWidth = Collider.WorldScale().x / 2;
		
		if(Velocity.x > 0) // check right
		{
			float length = Velocity.x * Time.Delta + halfWidth;
			Vector2 rayStart = Transform.Position + Collider.Center;
			Vector2 rayEnd = rayStart + new Vector2(length, 0);
			
			var hitResult = Scene.Trace
				.Ray(rayStart, rayEnd)
				.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(1,scale.y,1)))
				.WithAnyTags(WallTags)
				.Run();
			
			// Gizmo.Draw.Color = Color.Green;
			// Gizmo.Draw.LineThickness = 5;
			// Gizmo.Draw.Line( rayStart, rayEnd );
			
			if(hitResult.Hit)
			{
				Collisions.Right = true;
				Velocity = Velocity.WithX(0);
				
				// Gizmo.Draw.Color = Color.Red;
				// Gizmo.Draw.LineThickness = 5;
				// Gizmo.Draw.Line( rayStart, hitResult.HitPosition);
				
				// Log.Info("collision right");
			}
			
			// Log.Info($"skin: {skin} length: {length} scale: {scale} hit: {hitResult.Hit}");
		}
		else if ( Velocity.x < 0 ) // check left
		{
			float length = Velocity.x * Time.Delta - halfWidth;
			Vector2 rayStart = Transform.Position + Collider.Center;
			Vector2 rayEnd = rayStart + new Vector2(length, 0);
			
			var hitResult = Scene.Trace
				.Ray(rayStart, rayEnd)
				.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(1,scale.y,1)))
				.WithAnyTags(WallTags)
				.Run();
			
			// Gizmo.Draw.Color = Color.Green;
			// Gizmo.Draw.LineThickness = 5;
			// Gizmo.Draw.Line( rayStart, rayEnd );
			
			if(hitResult.Hit)
			{
				Collisions.Right = true;
				Velocity = Velocity.WithX(0);
				
				// Gizmo.Draw.Color = Color.Red;
				// Gizmo.Draw.LineThickness = 5;
				// Gizmo.Draw.Line( rayStart, hitResult.HitPosition);
				
				// Log.Info("collision left");
			}
		}
	}

	private void CheckCollisionUp()
	{
		if(Velocity.y <= 0 || GroundTags.IsEmpty)
			return;
		
		float skinWidth = 2;
		float length = Velocity.y * Time.Delta + skinWidth;
		
		Vector3 startPosition = Transform.Position + Collider.WorldScale() * Util.UpY + Util.DownY * skinWidth;
		Vector3 endPosition = startPosition + Util.UpY * length;
		
		// Gizmo.Draw.Color = Color.Green;
		// Gizmo.Draw.LineThickness = 5;
		// Gizmo.Draw.Line( startPosition, endPosition );
		
		var hitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(Collider.Scale.x, 1, 1)))
			.WithAnyTags(GroundTags)
			.Run();

		if ( hitResult.Hit )
		{
			Velocity = Velocity.WithY(0);
			CeilingHitEvent?.Invoke();
			HitCeilingMadafaka?.Invoke(true);
			Collisions.Up = true;
			var hitPos = hitResult.HitPosition;
			Transform.Position = Transform.Position.WithY(hitPos.y - _defaultColliderHeight);
		}
	}

	private void CheckCollisionDown()
	{
		// if velocity is up, don't check for ground?
		if ( Velocity.y > 0 || GroundTags.IsEmpty)
		{
			Grounded = false;
			return;
		}

		float skinWidth = 2;
		float length = -Velocity.y * Time.Delta + skinWidth;
		
		Vector3 startPosition = Transform.Position + Util.UpY * skinWidth;
		Vector3 endPosition = Transform.Position + Util.DownY * length;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(Collider.Scale.x * _skinPortion, 1, 1)))
			.WithAnyTags(GroundTags)
			.Run();

		if ( _groundHitResult.Hit )
		{
			_groundPoint = _groundHitResult.HitPosition;
			Velocity = Velocity.WithY(0);
			Collisions.Down = true;
			Grounded = true;
			// Log.Info($"ground hit: {_groundHitResult.HitPosition}");
		}
		else
		{
			Grounded = false;
			// Log.Info("no ground hit");
		}
	}
	
	private void CalculateVelocity()
	{
		Velocity = Vector2.Zero;

		for ( int i = 0; i < _activeProviders.Count; i++ )
			Velocity += _activeProviders[i].Velocity;
	}
	
	public void AddMotionProvider(IMotionProvider provider)
	{
		if(_motionProviders.Contains(provider))
			return;
		
		List<IMotionProvider> providersToRemove = new List<IMotionProvider>();
		
		if (provider.OverrideMotions != null && provider.OverrideMotions.Length > 0 )
		{
			for (int i = _activeProviders.Count - 1; i >= 0; i--)
			{
				var activeProvider = _activeProviders[i];

				if (provider.OverrideMotions.Contains(activeProvider.MotionType))
				{
					activeProvider.CancelMotion();
					providersToRemove.Add(activeProvider);
				}
			}
		}

		foreach (var providerToRemove in providersToRemove)
		{
			_activeProviders.Remove(providerToRemove);
		}

		_motionProviders.Add(provider);
		_activeProviders.Add(provider);
		ReevaluateActiveProviders();
	}
	
	private void ReevaluateActiveProviders()
	{
		foreach (var provider in _activeProviders.ToList())
		{
			if(provider.OverrideMotions == null || provider.OverrideMotions.Length == 0)
				continue;
			
			foreach (var motionType in provider.OverrideMotions)
			{
				var overriddenProvider = _activeProviders.FirstOrDefault(p => p.MotionType == motionType);

				if (overriddenProvider != null)
				{
					overriddenProvider.CancelMotion();
					_activeProviders.Remove(overriddenProvider);
				}
			}
		}
	}
	
	public void RemoveMotionProvider(IMotionProvider provider)
	{
		if(!_motionProviders.Contains(provider))
			return;
		
		_activeProviders.Remove(provider);
		
		if (provider.OverrideMotions != null && provider.OverrideMotions.Length != 0 )
		{
			foreach (var motionType in provider.OverrideMotions)
			{
				bool isStillOverridden = _activeProviders.Any(p => p.OverrideMotions.Contains(motionType));
				
				if (!isStillOverridden)
				{
					for (int i = 0; i < _motionProviders.Count; i++)
					{
						var potentialProvider = _motionProviders[i];

						if (potentialProvider.MotionType == motionType && !_activeProviders.Contains(potentialProvider))
						{
							_activeProviders.Add(potentialProvider);
							potentialProvider.OnMotionRestored();
							break;
						}
					}
				}
			}
		}
		
		_motionProviders.Remove(provider);
		ReevaluateActiveProviders();
	}

	public void Teleport( Vector3 position )
	{
		position.z = 1; // player offset
		Transform.Position = position;
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
