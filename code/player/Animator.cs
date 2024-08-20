using System;
using SpriteTools;

namespace Sandbox.player;

public enum AnimationState
{
	IDLE,
	RUN,
	JUMP,
	IN_AIR,
	DASH,
}

public sealed class Animator : Component
{
	[Property] SpriteComponent Sprite { get; set; }
	[Property] MotionCore2D MotionCore { get; set; }
	[Property] DashAbility DashAbility { get; set; }
	
	private AnimationState _state;

	protected override void OnEnabled()
	{
		
	}
	
	public void SetAnimationState(AnimationState newState)
	{
		if(_state == newState)
			return;
			
		switch ( newState )
		{
			case AnimationState.IDLE:
				Sprite.PlayAnimation("idle");
				break;
			case AnimationState.RUN:
				Sprite.PlayAnimation("run");
				break;
			case AnimationState.JUMP:
				Sprite.PlayAnimation("jump");
				break;
			case AnimationState.IN_AIR:
				Sprite.PlayAnimation("inAir");
				break;
			case AnimationState.DASH:
				Sprite.PlayAnimation("dash");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
			
		Log.Info($"set animation from {_state} to: {newState}");
		_state = newState;
	}

	protected override void OnFixedUpdate()
	{
		if(MotionCore.Velocity.y > 0)
			SetAnimationState(AnimationState.JUMP);
		else if(DashAbility.IsDashing)
			SetAnimationState(AnimationState.DASH);
		else if(MotionCore.Velocity.y < 0)
			SetAnimationState(AnimationState.IN_AIR);
		else if(MotionCore.Velocity.x != 0 && MotionCore.Grounded)
			SetAnimationState(AnimationState.RUN);
		else if(MotionCore.Grounded)
			SetAnimationState(AnimationState.IDLE);
	}
}
