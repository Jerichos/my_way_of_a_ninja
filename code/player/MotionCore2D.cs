using System;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] BoxCollider Collider { get; set; }
	
	[Property] TagSet GroundTags { get; set; }
	[Property] TagSet WallTags { get; set; }
	
	public Vector2 Velocity { get; private set; }
	
	private readonly List<IMotionProvider> _motionProviders = new();
	private readonly List<IMotionProvider> _activeProviders = new();
	private MotionTypeMatrix _motionMatrix = new();

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
				
				Log.Info("collision right");
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
				
				Log.Info("collision left");
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
			Log.Info($"ground hit: {_groundHitResult.HitPosition}");
		}
		else
		{
			Grounded = false;
			Log.Info("no ground hit");
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
	
	private void OnMotionProvidersChanged()
	{
		_activeProviders.Clear();

		for (int i = 0; i < _motionProviders.Count; i++)
		{
			IMotionProvider currentProvider = _motionProviders[i];
			bool shouldAdd = true;

			for (int j = 0; j < _activeProviders.Count; j++)
			{
				IMotionProvider activeProvider = _activeProviders[j];

				// Check the matrix to determine if the current provider should be combined
				if (!_motionMatrix.ShouldCombine(currentProvider.MotionType, activeProvider.MotionType))
				{
					shouldAdd = false;
					currentProvider.OnVelocityIgnored();
					break;
				}

				if (!_motionMatrix.ShouldCombine(activeProvider.MotionType, currentProvider.MotionType))
				{
					// If the current provider cancels the active one, remove the active one
					_activeProviders.RemoveAt(j);
					j--; // Adjust index after removal
				}
			}

			if (shouldAdd)
			{
				_activeProviders.Add(currentProvider);
			}
		}
	}
	
	public void AddMotionProvider(IMotionProvider provider)
	{
		_motionProviders.Add(provider);
		OnMotionProvidersChanged();
	}
	
	public void RemoveMotionProvider(IMotionProvider provider)
	{
		_motionProviders.Remove(provider);
		OnMotionProvidersChanged();
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
