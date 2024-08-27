using System;
using Sandbox.enemies;
using Sandbox.level;
using Sandbox.player;

namespace Sandbox.objects;

// if player steps on this platform, it will move
public class MovingPlatform : Component, IMotionProvider, IRespawn
{
	[Property] private FollowPath FollowPath { get; set; }
	[Property] private Collider TriggerCollider { get; set; }
	[Property] public bool IgnoreRespawn { get; set; }

	public Vector2 Velocity => FollowPath.Velocity;
	
	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.PLATFORM;
	
	private Player _lastPlayer; // this is not very good solution...
	private Vector3 _startPosition;

	protected override void OnAwake()
	{
		_startPosition = Transform.Position;
	}

	public void CancelMotion()
	{
	}

	public void OnMotionRestored()
	{
	}

	protected override void OnEnabled()
	{
		TriggerCollider.OnTriggerEnter += OnTriggerEnter;
		TriggerCollider.OnTriggerExit += OnTriggerExit;
	}
	
	protected override void OnDisabled()
	{
		TriggerCollider.OnTriggerEnter -= OnTriggerEnter;
		TriggerCollider.OnTriggerExit -= OnTriggerExit;
	}

	private void OnTriggerExit( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			player.MotionCore.GroundedEvent -= OnGrounded;
			player.MotionCore.RemoveMotionProvider(this);
			player.MotionCore.IsOnPlatform = false;
			_lastPlayer = null;
		}
	}

	private void OnTriggerEnter( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			player.MotionCore.GroundedEvent += OnGrounded;
			_lastPlayer = player;
			OnGrounded(player.MotionCore.Grounded);
		}
	}

	private void OnGrounded( bool isGrounded )
	{
		if ( _lastPlayer != null )
		{
			if ( isGrounded && _lastPlayer.MotionCore.GroundObject == GameObject)
			{
				_lastPlayer.MotionCore.IsOnPlatform = true;
				_lastPlayer.MotionCore.AddMotionProvider(this);
			}
			else
			{
				_lastPlayer.MotionCore.IsOnPlatform = false;
			}
		}
	}

	public void Respawn()
	{
		Transform.Position = _startPosition;

		if ( _lastPlayer != null )
		{
			_lastPlayer.MotionCore.RemoveMotionProvider(this);
			_lastPlayer.MotionCore.GroundedEvent -= OnGrounded;
		}
		
		_lastPlayer = null;
	}
}
