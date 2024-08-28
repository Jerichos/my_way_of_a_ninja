using System;

namespace Sandbox.player;

public class PlayerInput : Component
{
	[Property] public Player Player { get; set; }
	
	private MotionCore2D MotionCore => Player.MotionCore;
	private JumpAbility JumpAbility => Player.JumpAbility;
	private DashAbility DashAbility => Player.DashAbility;
	private SwordAbility SwordAbility => Player.SwordAbility;
	private CrouchAbility CrouchAbility => Player.CrouchAbility;
	private MoveAbility MoveAbility => Player.MoveAbility;

	protected override void OnUpdate()
	{
		if(Input.UsingController)
			HandleControllerInput();
		else
		{
			HandleKeyboardInput();
		}
		
		HandleCommonInput();
	}

	private void HandleKeyboardInput()
	{
		if ( Input.Down( "Right" ))
		{
			if(!CrouchAbility.IsCrouching)
				MoveAbility.SetInputX(1);
			
			MotionCore.Facing = 1;
		}
		else if ( Input.Down( "Left" ))
		{
			if(!CrouchAbility.IsCrouching)
				MoveAbility.SetInputX(-1);
			
			MotionCore.Facing = -1;
		}
		else
		{
			MoveAbility.SetInputX(0);
		}
	}

	private void HandleCommonInput()
	{
		if(Input.Pressed("Jump") && !CrouchAbility.IsCrouching)
		{
			JumpAbility.Jump();
		}
		else if ( Input.Released("Jump") )
		{
			JumpAbility.StopJump();
		}
		
		if(Input.Pressed("Dash"))
		{
			DashAbility.StartDash();
		}
		
		if(Input.Pressed("attack1"))
		{
			SwordAbility.StartAttack();
		}
		
		if(Input.Down("Down"))
		{
			CrouchAbility.StartCrouch();
			MoveAbility.SetInputX(0);
		}
		else if ( Input.Released("Down") )
		{
			CrouchAbility.StopCrouch();
		}
	}

	private void HandleControllerInput()
	{
		int inputX = MathF.Abs(Input.AnalogMove.y) > 0.01 ? -MathF.Sign(Input.AnalogMove.y) : 0;
		MotionCore.Facing = inputX;
		MoveAbility.SetInputX(inputX);
	}

}
