using System;

namespace Sandbox.player;

public sealed class MotionCore2D : Component
{
	[Property] public BoxCollider Collider { get; private set; }
	
	[Property] private TagSet GroundTags { get; set; }
	[Property] private TagSet WallTags { get; set; }
	[Property] private TagSet PlatformTags { get; set; } // there is no collision but you can ground on them
	
	public Vector2 Velocity { get; private set; }
	public Vector2 Size => Collider.Center * Transform.LocalScale;
	public Vector2 Center => Transform.Position + (Vector3)Size;
	
	private readonly List<IMotionProvider> _motionProviders = new();
	private readonly List<IMotionProvider> _activeProviders = new();

	private SceneTraceResult _groundHitResult;

	private Vector2 _groundPoint;
	public GameObject GroundObject { get; private set; }

	public bool IsOnPlatform; // TODO: not nice solution, so what
	
	private bool _grounded;
	public bool Grounded 
	{
		get => _grounded;
		private set
		{
			if(_grounded == value)
				return;
			
			if(IsOnPlatform)
				return;
			
			_grounded = value;

			if ( _grounded )
			{
				// float distance =  _groundPoint.y - Transform.Position.y;
				// float newVelocity = distance / Time.Delta;
				// Velocity = Velocity.WithY(newVelocity);
				
				GroundObject = _groundHitResult.GameObject;
			}
			else
			{
				GroundObject = null;
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

	private const float SKIN = 0.99f;
	private float _defaultColliderHeight;

	protected override void OnStart()
	{
		if(Collider != null)
			_defaultColliderHeight = Collider.Scale.y;
	}

	protected override void OnFixedUpdate()
	{
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
		Vector3 startPosition = Transform.Position + new Vector3(Collider.WorldScale().x / 2f * direction, 10,0);
		Vector3 endPosition = startPosition + Util.DownY * 30;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.WithAnyTags(GroundTags)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(1,1,1)))
			.Run();
		
		if ( _groundHitResult.Hit )
			return true;

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
		
		if ( _groundHitResult.Hit )
		{
			return true;
		}

		return false;
	}

	private void HandleHorizontalCollisions()
	{
		if (WallTags == null || WallTags.IsEmpty)
			return;

		float width = Collider.WorldScale().x / 2f;
		float height = Collider.WorldScale().y * 0.5f;

		if (Velocity.x != 0) // check both directions
		{
			bool isRight = Velocity.x > 0;
			float length = Math.Abs(Velocity.x) * Time.Delta + width;
			Vector3 direction = isRight ? Util.RightX : Util.LeftX;

			IEnumerable<SceneTraceResult> hitResult = Scene.Trace
				.Ray(Collider.WorldCenter(), Collider.WorldCenter() + direction * length)
				.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(0, height, 0)))
				.WithAnyTags(WallTags)
				.RunAll();
			
			foreach ( var hit in hitResult)
			{
				if(IsItPlatform(hit))
					continue;
				
				if (isRight)
				{
					Collisions.Right = true;
					Transform.Position = Transform.Position.WithX(hit.HitPosition.x - width);
				}
				else
				{
					Collisions.Left = true;
					Transform.Position = Transform.Position.WithX(hit.HitPosition.x + width);
				}

				Velocity = Velocity.WithX(0);
				break;
			}
		}
	}
	
	private bool IsItPlatform(SceneTraceResult hitResult)
	{
		if ( PlatformTags != null && hitResult.GameObject.Tags.HasAny( PlatformTags ) )
		{
			return true;
		}
		
		return false;
	}

	private void CheckCollisionUp()
	{
		if(Velocity.y <= 0 || GroundTags.IsEmpty)
			return;
		
		float width = Collider.WorldScale().x * SKIN;
		float height = Collider.WorldScale().y / 2f;
		float length = (Velocity.y * Time.Delta) + height;

		Vector3 endPosition = Collider.WorldCenter() + Util.UpY * length;
		
		var hitResult = Scene.Trace
			.Ray(Collider.WorldCenter(), endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(width, 1, 1)))
			.WithAnyTags(GroundTags)
			.Run();

		if ( hitResult.Hit )
		{
			if(IsItPlatform(hitResult))
				return;
			
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
		if ( Velocity.y > 0 || GroundTags.IsEmpty)
		{
			Grounded = false;
			return;
		}
		
		float width = Collider.WorldScale().x * SKIN;
		float height = Collider.WorldScale().y / 2f;
		float length = (-Velocity.y * Time.Delta) + height + 0.04f; // add 0.04f if Velocity.y is 0
		
		Vector3 startPosition = Collider.WorldCenter();
		Vector3 endPosition = startPosition + Util.DownY * length;
		
		_groundHitResult = Scene.Trace
			.Ray(startPosition, endPosition)
			.Size(BBox.FromPositionAndSize(Vector3.Zero, new Vector3(width, 0, 0)))
			.WithAnyTags(GroundTags)
			.Run();
		
		if ( _groundHitResult.Hit )
		{
			_groundPoint = _groundHitResult.HitPosition;
			Collisions.Down = true;
			Grounded = true;
			
			float distance =  _groundPoint.y - Transform.Position.y;
			float newVelocity = distance / Time.Delta;
			Velocity = Velocity.WithY(newVelocity);
		}
		else
		{
			Grounded = false;
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
