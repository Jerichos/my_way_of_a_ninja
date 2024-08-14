
namespace Sandbox.player;

public sealed class JumpAbility : Component
{
	[Property] MotionCore2D MotionCore { get; set; }
	[Property] Rigidbody Rb { get; set; }

	[Property] public float JumpHeight = 500f; // jump up until height is reached
	[Property] public float JumpForce = 100f; // apply force to jump
	
	[Property] Curve JumpCurve { get; set; }
	
	public bool IsJumping { get; private set; }
	
	private float _jumpHeightReached;
	
	protected override void OnUpdate()
	{
		if(Input.Pressed("Jump") && CanJump())
		{
			Log.Info("Jump start!");
			IsJumping = true;
			_jumpHeightReached = 0;
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if(IsJumping)
		{
			if(_jumpHeightReached < JumpHeight)
			{
				float jumpTime = JumpCurve.Evaluate(_jumpHeightReached / JumpHeight);
				
				var force = JumpForce * jumpTime;
				
				Log.Info("jumpTime: " + jumpTime + " force: " + force);
				
				Rb.ApplyImpulse(Vector3.Up * force);
			}
			else
			{
				Log.Info("jump height reached");
				IsJumping = false;
				var velocity = Rb.Velocity;
				velocity.z = 0;
				Rb.Velocity = velocity;
			}
			
			_jumpHeightReached += Rb.Velocity.z * Time.Delta;
		}
	}
	
	private bool CanJump()
	{
		return !IsJumping && MotionCore.Grounded;
	}
}
