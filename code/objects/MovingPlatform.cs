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
	[Property] public bool MoveOnlyWhenPlayerOn { get; set; } // if true, platform will move only when player is on it, if is not, it will move back

	public Vector2 Velocity => FollowPath.Velocity;
	
	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.PLATFORM;
	
	private Player _lastPlayer; // this is not very good solution...
	private Vector3 _startPosition;
	
	private bool _moveOnlyWhenPlayerOn;

	protected override void OnAwake()
	{
		_moveOnlyWhenPlayerOn = MoveOnlyWhenPlayerOn;
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

			if ( MoveOnlyWhenPlayerOn )
			{
				FollowPath.GoBack();
			}
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
				if ( MoveOnlyWhenPlayerOn )
				{
					FollowPath.GoForward();
				}
			}
			else
			{
				_lastPlayer.MotionCore.IsOnPlatform = false;
				if ( MoveOnlyWhenPlayerOn )
				{
					FollowPath.GoBack();
				}
			}
		}
	}

	public void Respawn()
	{
		MoveOnlyWhenPlayerOn = _moveOnlyWhenPlayerOn;
		Transform.Position = _startPosition;

		if ( _lastPlayer != null )
		{
			_lastPlayer.MotionCore.RemoveMotionProvider(this);
			_lastPlayer.MotionCore.GroundedEvent -= OnGrounded;
		}
		
		_lastPlayer = null;
	}
}
