using System;

namespace Sandbox.player;

public class CrouchAbility : Component
{
	[Property] MotionCore2D MotionCore { get; set; }
	[Property] BoxCollider Collider { get; set; }
	
	private readonly float _crouchScale = 2.1f;
	
	public bool IsCrouching { get; private set; }
	
	public Action<bool> CrouchEvent;

	public void StartCrouch()
	{
		if(!MotionCore.Grounded || IsCrouching)
			return;
		
		IsCrouching = true;
		Collider.Scale = Collider.Scale.WithY(Collider.Scale.y / _crouchScale);
		Collider.Center = Collider.Center.WithY(Collider.Center.y / _crouchScale);
		CrouchEvent?.Invoke(true);
	}
	
	public void StopCrouch()
	{
		IsCrouching = false;
		Collider.Scale = Collider.Scale.WithY(Collider.Scale.y * _crouchScale);
		Collider.Center = Collider.Center.WithY(Collider.Center.y * _crouchScale);
		CrouchEvent?.Invoke(false);
	}
}
