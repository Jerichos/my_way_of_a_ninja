using Sandbox;

public sealed class Player : Component
{
	[Property] private Rigidbody Rb { get; set; }

	[Property] private float HorizontalSpeed { get; set; } = 5000f;
	[Property] private Curve Curve { get; set; }
	[Property] private float MaxHorizontalSpeed { get; set; } = 200;
	
	[Property] TagSet GroundTags { get; set; }
	
	private float _horizontalInput;
	
	protected override void OnUpdate()
	{
		// _horizontalInput = 0;
		// if(Input.Down("Right"))
		// {
		// 	_horizontalInput = 1;
		// 	Log.Info("Right");
		// }
		// else if(Input.Down("Left"))
		// {
		// 	_horizontalInput = -1;
		// 	Log.Info("Left");
		// }
	}
	
	protected override void OnFixedUpdate()
	{
	}
	
	private void HandleMovement()
	{
		var moveForce = _horizontalInput * HorizontalSpeed;
		if(Rb.Velocity.x > MaxHorizontalSpeed)
		{
			moveForce = 0;
		}
		else if(Rb.Velocity.x < -MaxHorizontalSpeed)
		{
			moveForce = 0;
		}
		else
		{
			Vector3 moveForceVector = new Vector3(moveForce, 0, 0);
			Rb.ApplyForce(moveForceVector);
			Log.Info("apply move force: " + moveForce + " velocity: " + Rb.Velocity.x + " Time.Delta: " + Time.Delta);
		}
	}
}
