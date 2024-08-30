using System;

namespace Sandbox.player;

public class CrouchAbility : Component
{
	[Property] MotionCore2D MotionCore { get; set; }
	[Property] BoxCollider Collider { get; set; }

	[Property] private float CrouchHeight = 16f;
	
	
	public bool IsCrouching { get; private set; }
	
	public Action<bool> CrouchEvent;

	public void StartCrouch()
	{
		if(!MotionCore.Grounded || IsCrouching)
			return;
		
		IsCrouching = true;
		Collider.Scale = Collider.Scale.WithY(CrouchHeight);
		Collider.Center = Collider.Center.WithY(CrouchHeight / 2);
		CrouchEvent?.Invoke(true);
	}
	
	public void StopCrouch()
	{
		IsCrouching = false;
		MotionCore.ResetCollider();
		CrouchEvent?.Invoke(false);
	}
}
