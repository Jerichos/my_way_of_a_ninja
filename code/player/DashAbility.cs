using System;

namespace Sandbox.player;

// dash horizontally
public class DashAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] public bool CanDashInAir { get; set; } = true;
	[Property] public float MaxDistance { get; set; } = 1000;
	[Property] public float MaxVelocity { get; set; } = 1000;
	[Property] public float DashIn { get; set; } = 1;
	[Property] public Curve VelocityCurve { get; set; }
	
	public Vector2 Velocity { get; private set; }
	public int Priority { get; private set; }
	public bool Additive => false;
	public MotionType MotionType => MotionType.DASH;

	private int _defaultPriority = 0;
	private int _dashPriority = 1;
	
	private float _t;
	private float _distance;
	private bool _isDashing;
	
	protected override void OnUpdate()
	{
		if(Input.Pressed("Run") && CanDash())
		{
			_isDashing = true;
			_t = 0;
			_distance = 0;
			Priority = _dashPriority;
			MotionCore.AddMotionProvider(this);
			Log.Info("start dash");
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if(_isDashing)
		{
			_t += Time.Delta / DashIn;
			if(_distance < MaxDistance && MotionCore.Collisions.Right == false && MotionCore.Collisions.Left == false)
			{
				Log.Info($"still dashing? _t: {_t} _distance {_distance}");

				// Ensure _t doesn't exceed 1 (end of the dash)
				if (_t > 1)
				{
					_t = 1;
				}

				// Evaluate the curve based on normalized time
				float curveVelocity = VelocityCurve.Evaluate(_t);

				// Calculate the velocity required to reach the MaxDistance in the given dashIn time
				float requiredVelocity = MaxDistance / DashIn;

				// Actual velocity is scaled by the curve
				var velocity = requiredVelocity * curveVelocity;

				// Update the distance traveled
				_distance += velocity * Time.Delta;

				// Apply the velocity in the direction the character is facing
				Velocity = Util.RightX * velocity * MotionCore.Facing;

				// Check if the dash should be completed
				if (_distance >= MaxDistance)
				{
					_distance = MaxDistance;
					_t = 1;  // Ensure the dash finishes
				}
			}
			else
			{
				_isDashing = false;
				Velocity = Vector2.Zero;
				Priority = _defaultPriority;
				MotionCore.RemoveMotionProvider(this);
				Log.Info($"end dash distance: {_distance}");
			}
			
			Log.Info($"DASH t: {_t} force: {Velocity.x} velocity: {Velocity.x} is there collision?: {MotionCore.Collisions.Right} {MotionCore.Collisions.Left}");
		}
	}

	private bool CanDash()
	{
		if(!CanDashInAir && !MotionCore.Grounded || _isDashing)
			return false;
		
		return true;
	}
	
	public void OnVelocityIgnored()
	{
		Velocity = Vector2.Zero;
	}
}
