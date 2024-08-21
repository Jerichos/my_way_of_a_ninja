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
	CROUCH,
}

public sealed class Animator : Component
{
	[Property] private SpriteComponent Sprite { get; set; }
	[Property] private Player Player { get; set; }
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private DashAbility DashAbility { get; set; }
	[Property] private SwordAbility SwordAbility { get; set; }
	[Property] private CrouchAbility CrouchAbility { get; set; }
	
	// on hit player will be in hit animation, which is fade in and out n times during n seconds
	[Property] private int BlinkCount;
	[Property] private float BlinkDuration;
	
	private int _blinkCounter;
	private float _blinkTimer;
	
	private AnimationState _state;

	protected override void OnEnabled()
	{
		SwordAbility.AttackEvent += (_) => SetAnimationState(_state, true);
		Player.HitEvent += () =>
		{
			_blinkCounter = 0;
			_blinkTimer = 0;
		};
		
		_blinkCounter = BlinkCount;
	}
	
	// TODO: there is a bug when animation changes, usually to/from attack animation. It flickers for a frame, like transition is not right.
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
			case AnimationState.CROUCH:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "crouch_attack" : "crouch" , force);
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

	protected override void OnUpdate()
	{
		if(CrouchAbility.IsCrouching)
			SetAnimationState(AnimationState.CROUCH);
		else if(MotionCore.Velocity.y > 0)
			SetAnimationState(AnimationState.JUMP);
		else if(DashAbility.IsDashing)
			SetAnimationState(AnimationState.DASH);
		else if(MotionCore.Velocity.y < 0)
			SetAnimationState(AnimationState.IN_AIR);
		else if(MotionCore.Velocity.x != 0 && MotionCore.Grounded)
			SetAnimationState(AnimationState.RUN);
		else if(MotionCore.Grounded)
			SetAnimationState(AnimationState.IDLE);

		HandleHitAnimation();
	}

	private void HandleHitAnimation()
	{
		if(_blinkCounter >= BlinkCount)
			return;
		
		_blinkTimer += Time.Delta;
		
		if(_blinkTimer >= BlinkDuration)
		{
			
			_blinkTimer = 0;
			_blinkCounter++;
			
			if ( _blinkCounter >= BlinkCount )
			{
				var c = Sprite.FlashTint;
				c.a = 0;
				Sprite.FlashTint = c;
				return;
			}
		}
		
		float alpha = (_blinkTimer / BlinkDuration) * 0.5f;
		var color = Sprite.FlashTint;
		color.a = alpha;
		Sprite.FlashTint = color;
	}
}
