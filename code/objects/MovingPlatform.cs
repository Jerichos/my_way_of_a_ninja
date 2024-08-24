using System;
using Sandbox.enemies;
using Sandbox.player;

namespace Sandbox.objects;

// if player steps on this platform, it will move
public class MovingPlatform : Component, IMotionProvider
{
	[Property] private FollowPath FollowPath { get; set; }
	[Property] private Collider TriggerCollider { get; set; }

	public Vector2 Velocity => FollowPath.Velocity;
	
	public MotionType[] OverrideMotions => Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.PLATFORM;
	
	private Player _lastPlayer; // this is not very good solution...
	
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
		if ( isGrounded && _lastPlayer != null && _lastPlayer.MotionCore.GroundObject == GameObject)
		{
			_lastPlayer.MotionCore.AddMotionProvider(this);
		}
	}
}
