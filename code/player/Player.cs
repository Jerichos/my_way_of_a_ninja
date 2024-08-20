using System;
using Sandbox;
using Sandbox.player;
using SpriteTools;

public sealed class Player : Component
{
	[Property] private SpriteComponent SpriteComponent { get; set; }
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private MoveAbility MoveAbility { get; set; }
	[Property] private JumpAbility JumpAbility { get; set; }
	[Property] private DashAbility DashAbility { get; set; }
	[Property] private SwordAbility SwordAbility { get; set; }

	[Property] public SAttribute<int> Health { get; private set; } = 10;
	
	protected override void OnEnabled()
	{
		MotionCore.FacingChangedEvent += OnFacingChanged;
		OnFacingChanged(MotionCore.Facing);
	}

	protected override void OnUpdate()
	{
		HandleInput();
	}

	private void HandleInput()
	{
		if(Input.Pressed("Jump"))
		{
			JumpAbility.Jump();
		}
		else if ( Input.Released("Jump") )
		{
			JumpAbility.StopJump();
		}
		
		if(Input.Pressed("Run"))
		{
			DashAbility.StartDash();
		}
		
		if(Input.Pressed("attack1"))
		{
			SwordAbility.StartAttack();
		}
	}

	private void OnFacingChanged( int facing)
	{
		SpriteComponent.SpriteFlags = facing == 1 ? SpriteFlags.None : SpriteFlags.HorizontalFlip;
	}
	
	public void Teleport(Vector3 position)
	{
		MotionCore.Teleport(position);
	}
}
