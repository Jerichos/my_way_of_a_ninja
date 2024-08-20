using System;

namespace Sandbox.player;

// dash horizontally
public class DashAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] public bool CanDashInAir { get; set; } = true;
	[Property] public float MaxDistance { get; set; } = 1000;
	[Property] public float DashIn { get; set; } = 1;
	[Property] public Curve VelocityCurve { get; set; }
	[Property] SoundEvent DashSound { get; set; }
	
	public Vector2 Velocity { get; private set; }
	public MotionType MotionType => MotionType.DASH;
	public MotionType[] OverrideMotions => new[] {MotionType.MOVE, MotionType.JUMP, MotionType.ENVIRONMENT, MotionType.GRAVITY};

	public bool IsDashing => _isIsDashing;
	
	private float _t;
	private float _distance;
	private bool _isIsDashing;
	
	protected override void OnUpdate()
	{
		if(Input.Pressed("Run") && CanDash())
		{
			_isIsDashing = true;
			_t = 0;
			_distance = 0;
			MotionCore.AddMotionProvider(this);
			Components.Get<SoundPointComponent>().SoundEvent = DashSound;
			Components.Get<SoundPointComponent>().StartSound();
			Log.Info("start dash");
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if(_isIsDashing)
		{
			_t += Time.Delta / DashIn;
			if(_distance < MaxDistance && MotionCore.Collisions.Right == false && MotionCore.Collisions.Left == false)
			{
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
				_isIsDashing = false;
				Velocity = Vector2.Zero;
				MotionCore.RemoveMotionProvider(this);
				Log.Info($"end dash distance: {_distance}");
			}
			
			Log.Info($"DASH t: {_t} force: {Velocity.x} velocity: {Velocity.x}");
		}
	}

	private bool CanDash()
	{
		if(!CanDashInAir && !MotionCore.Grounded || _isIsDashing)
			return false;
		
		return true;
	}
	
	public void OnMotionRestored()
	{
		
	}
	
	public void OnMotionCanceled()
	{
		Log.Info("DASH Cancel");
		Velocity = Vector2.Zero;
		_isIsDashing = false;
		MotionCore.RemoveMotionProvider(this);
	}
}
