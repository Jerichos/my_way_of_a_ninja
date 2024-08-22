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
	[Property] private Player Player { get; set; }
	
	private SpriteComponent Sprite => Player.SpriteComponent;
	private MotionCore2D MotionCore => Player.MotionCore;
	private DashAbility DashAbility => Player.DashAbility;
	private SwordAbility SwordAbility => Player.SwordAbility;
	private CrouchAbility CrouchAbility => Player.CrouchAbility;
	
	// on hit player will be in hit animation, which is fade in and out n times during n seconds
	[Property] private int HitBlinkCount { get; set; } = 3;
	[Property] float BlinkDuration { get; set; } = 1; // TODO: move it to player, and make player invincible for this duration
	
	private int _blinkCounter;
	private float _blinkTimer;
	
	private AnimationState _state;

	
	
	// TODO: there is a bug when animation changes, usually from attack to non-attack animation. The sprite goes wider in size for a frame.
	private void SetAnimationState(AnimationState newState, bool force = false)
	{
		string animationSet;
		switch ( newState )
		{
			case AnimationState.IDLE:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "idle_attack" : "idle" , force);
				animationSet = SwordAbility.IsAttacking ? "idle_attack" : "idle";
				break;
			case AnimationState.RUN:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "run_attack" : "run" , force);
				animationSet = SwordAbility.IsAttacking ? "run_attack" : "run";
				break;
			case AnimationState.JUMP:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "inAir_attack" : "jump" , force);
				animationSet = SwordAbility.IsAttacking ? "inAir_attack" : "jump";
				break;
			case AnimationState.IN_AIR:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "inAir_attack" : "inAir" , force);
				animationSet = SwordAbility.IsAttacking ? "inAir_attack" : "inAir";
				break;
			case AnimationState.CROUCH:
				Sprite.PlayAnimation(SwordAbility.IsAttacking ? "crouch_attack" : "crouch" , force);
				animationSet = SwordAbility.IsAttacking ? "crouch_attack" : "crouch";
				break;
			case AnimationState.DASH:
				Sprite.PlayAnimation("dash");
				animationSet = "dash";
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
			
		Log.Info($"set animation from {_state} to: {newState} animationSet: {animationSet}. IsAttacking: {SwordAbility.IsAttacking} force: {force}");
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
	
	// This causes flickering when attack ends
	private void OnAttackEvent( bool isAttacking )
	{
		if(isAttacking)
			SetAnimationState(_state, true);
		// else
		// 	SetAnimationState(_state, false);
	}
	
	protected override void OnEnabled()
	{
		SwordAbility.AttackEvent += OnAttackEvent;
		Player.HitEvent += () =>
		{
			_blinkCounter = 0;
			_blinkTimer = 0;
		};
		
		_blinkCounter = HitBlinkCount;
	}
	
	protected override void OnDisabled()
	{
		SwordAbility.AttackEvent -= OnAttackEvent;
		Player.HitEvent -= () =>
		{
			_blinkCounter = 0;
			_blinkTimer = 0;
		};
	}

	private void HandleHitAnimation()
	{
		if(_blinkCounter >= HitBlinkCount)
			return;
		
		_blinkTimer += Time.Delta;
		
		if(_blinkTimer >= BlinkDuration)
		{
			
			_blinkTimer = 0;
			_blinkCounter++;
			
			if ( _blinkCounter >= HitBlinkCount )
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
