using System;
using Sandbox;
using Sandbox.player;
using SpriteTools;

public sealed class Player : Component
{
	[Property] private SpriteComponent SpriteComponent { get; set; }
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private MoveAbility MoveAbility { get; set; }
	
	protected override void OnEnabled()
	{
		MotionCore.FacingChangedEvent += OnFacingChanged;
		OnFacingChanged(MotionCore.Facing);
	}

	private void OnFacingChanged( int facing)
	{
		SpriteComponent.SpriteFlags = facing == 1 ? SpriteFlags.None : SpriteFlags.HorizontalFlip;
	}
}
