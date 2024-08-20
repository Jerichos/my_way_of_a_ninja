using System;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] private BoxCollider Collider { get; set; }
	
	[Property] private TagSet GroundTags { get; set; }
	[Property] private TagSet WallTags { get; set; }
	
	public Vector2 Velocity { get; private set; }
	
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
				GroundHitEvent?.Invoke();
				Log.Info($"snap to: {_groundPoint}");
			}

			Log.Info($"set ground {_grounded}");
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

	public Action GroundHitEvent;
	public Action CeilingHitEvent;
	
	public Action<bool> HitCeilingMadafaka;
	public CollisionData Collisions;
	
	private float _skinPortion = 0.95f;

	protected override void OnFixedUpdate()
	{
		// Log.Info($"1 Velocity: {Velocity}");
		Collisions.Reset();
		CalculateVelocity();
		HandleHorizontalCollisions();
		HandleVerticalCollisions();
		
		// Log.Info($"2 Velocity: {Velocity}");
		Transform.Position += (Vector3)Velocity * Time.Delta;
	}

	private void HandleVerticalCollisions()
	{
		CheckCollisionDown();
		CheckCollisionUp();
	}
	
	

	private void HandleHorizontalCollisions()
	{
		// float skin = 5;
		Vector3 scale = Collider.Scale * _skinPortion;
		float halfWidth = Collider.Scale.x / 2;
		
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
			
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( rayStart, rayEnd );
			
			if(hitResult.Hit)
			{
				Collisions.Right = true;
				Velocity = Velocity.WithX(0);
				
				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.LineThickness = 5;
				Gizmo.Draw.Line( rayStart, hitResult.HitPosition);
				
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
			
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineThickness = 5;
			Gizmo.Draw.Line( rayStart, rayEnd );
			
			if(hitResult.Hit)
			{
				Collisions.Right = true;
				Velocity = Velocity.WithX(0);
				
				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.LineThickness = 5;
				Gizmo.Draw.Line( rayStart, hitResult.HitPosition);
				
				// Log.Info("collision left");
			}
		}
	}

	private void CheckCollisionUp()
	{
		if(Velocity.y <= 0)
			return;
		
		float skinWidth = 25;
		float length = Velocity.y * Time.Delta + skinWidth;
		
		Vector3 startPosition = Transform.Position + Collider.Scale * Util.UpY + Util.DownY * skinWidth;
		Vector3 endPosition = startPosition + Util.UpY * length;
		
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
			Velocity = Velocity.WithY(0);
			CeilingHitEvent?.Invoke();
			HitCeilingMadafaka?.Invoke(true);
			Collisions.Up = true;
		}
	}

	private void CheckCollisionDown()
	{
		// if velocity is up, don't check for ground?
		if ( Velocity.y > 0)
		{
			Grounded = false;
			return;
		}

		float skinWidth = 10;
		float length = -Velocity.y * Time.Delta + skinWidth + 1;
		
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
		{
			Velocity += _activeProviders[i].Velocity;
		}
	}
	
	int _added = 0;
	
	public void AddMotionProvider(IMotionProvider provider)
	{
		List<IMotionProvider> providersToRemove = new List<IMotionProvider>();
    
		for (int i = _activeProviders.Count - 1; i >= 0; i--)
		{
			var activeProvider = _activeProviders[i];

			if (provider.OverrideMotions.Contains(activeProvider.MotionType))
			{
				activeProvider.OnMotionCanceled();
				providersToRemove.Add(activeProvider);
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
		List<IMotionProvider> providersToAddBack = new List<IMotionProvider>();

		foreach (var provider in _activeProviders.ToList())
		{
			foreach (var motionType in provider.OverrideMotions)
			{
				var overriddenProvider = _activeProviders.FirstOrDefault(p => p.MotionType == motionType);

				if (overriddenProvider != null)
				{
					overriddenProvider.OnMotionCanceled();
					_activeProviders.Remove(overriddenProvider);
				}
			}
		}
	}
	
	public void RemoveMotionProvider(IMotionProvider provider)
	{
		_activeProviders.Remove(provider);

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

		_motionProviders.Remove(provider);
		ReevaluateActiveProviders();
	}

	public void Teleport( Vector3 position )
	{
		position.z = 1; // player offset
		var prevPosition = Transform.Position;
		Log.Info("teleport from: " + prevPosition + " to: " + position);
		Transform.Position = position;
		Log.Info("new position: " + Transform.Position);
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
