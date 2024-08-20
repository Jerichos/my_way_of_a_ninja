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
	[Property] SwordAbility SwordAbility { get; set; }
	
	private AnimationState _state;

	protected override void OnEnabled()
	{
		SwordAbility.AttackEvent += (_) => SetAnimationState(_state, true);
	}
	
	public void SetAnimationState(AnimationState newState, bool force = false)
	{
		if(_state == newState && !force)
			return;
			
		switch ( newState )
		{
			case AnimationState.IDLE:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "idle_attack" : "idle" , force);
				break;
			case AnimationState.RUN:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "run_attack" : "run" , force);
				break;
			case AnimationState.JUMP:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "inAir_attack" : "jump" , force);
				break;
			case AnimationState.IN_AIR:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "inAir_attack" : "inAir" , force);
				break;
			case AnimationState.DASH:
				Sprite.PlayAnimation("dash");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
			
		Log.Info($"set animation from {_state} to: {newState}. IsAttacking: {SwordAbility.IsAttacking}");
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
