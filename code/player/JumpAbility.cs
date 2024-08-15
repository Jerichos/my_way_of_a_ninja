
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
				
				Rb.ApplyForce(Vector3.Up * force);
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

	// if there is vertical collision or something else that stops the jump
	private void CancelJump()
	{
		if(!IsJumping)
			return;
		
		Log.Info("Jump canceled!");
		IsJumping = false;
	}
	
	private bool CanJump()
	{
		return !IsJumping && MotionCore.Grounded;
	}

	protected override void OnEnabled()
	{
		Log.Info("JumpAbility enabled");
		MotionCore.CeilingHitEvent += CancelJump;
	}
	
	protected override void OnDisabled()
	{
		Log.Info("JumpAbility disabled");
		MotionCore.CeilingHitEvent -= CancelJump;
	}
}
