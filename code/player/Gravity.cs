namespace Sandbox.player;


public sealed class Gravity : Component
{
	[Property] Rigidbody Rb { get; set; }
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] float GravityForce { get; set; } = 9.8f;
	[Property] float MaxVelocity { get; set; } = 10;
	[Property] Vector3 GravityDirection { get; set; } = new Vector3(0, 0, -1);
	
	[Property] Curve GravityCurve { get; set; } // apply gravity force portion between current and max velocity
	
	protected override void OnFixedUpdate()
	{
		// apply gravity force until max velocity of gravity direction is reached
		if(MotionCore.Grounded || Rb.Velocity.z > 0)
			return;
		
		if(Rb.Velocity.Dot(GravityDirection) < MaxVelocity)
		{
			var gravityT = GravityCurve.Evaluate(Rb.Velocity.Dot(GravityDirection) / MaxVelocity);
			var gravityForce = gravityT * GravityForce;
			Rb.ApplyImpulse(GravityDirection * gravityForce);
		}
	}
}
