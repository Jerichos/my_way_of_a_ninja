using System;

namespace Sandbox.player;

public class MoveAbility : Component
{
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] public float MaxAcceleration { get; set; } = 1000;
	[Property] public float MaxDeceleration { get; set; } = 1000;
	[Property] public float MaxVelocity { get; set; } = 100;
	[Property] public float MinVelocity { get; set; } = 20;
	
	[Property] public Curve AccelerationCurve { get; set; }
	[Property] public Curve DecelerationCurve { get; set; }
	

	private int _inputX;
	public int InputX
	{
		get => _inputX;
		private set
		{
			if (_inputX == value)
				return;
			
			_inputX = value;
			InputXChangedEvent?.Invoke(_inputX);
		}
	}
	
	private float _lastInputX;
	private float _movedForce;
	
	public Action<int> InputXChangedEvent;
	
	protected override void OnUpdate()
	{
		InputX = 0;

		if(Input.Down("Right"))
			InputX = 1;
		else if(Input.Down("Left"))
			InputX = -1;
	}
	
	protected override void OnFixedUpdate()
	{
		HandleAcceleration();
		HandleDeceleration();
	}

	private void HandleAcceleration()
	{
		if(_inputX == 0)
			return;
		
		// if there is collision in that direction we can't move
		if ( (_inputX == 1 && MotionCore.Collisions.Right) || (_inputX == -1 && MotionCore.Collisions.Left) )
		{
			Log.Info("Collision in direction: " + _inputX);
			return;
		}
		
		// if we reached maximum velocity to one direction we can move only in the opposite direction
		if(Math.Abs(MotionCore.Velocity.x) > MaxVelocity && Math.Abs(Math.Sign(MotionCore.Velocity.x) - _inputX) < 0.1f)
			return;
		
		// apply the force
		var forceT = AccelerationCurve.Evaluate(Math.Abs(MotionCore.Velocity.x) / MaxVelocity);
		var force = forceT * MaxAcceleration * _inputX;
		_movedForce += force;
		MotionCore.ApplyHorizontalForce(force);
		_lastInputX = _inputX;
	}
	
	private void HandleDeceleration()
	{
		if(_inputX != 0 || MotionCore.Velocity.x == 0)
			return;
		
		// decelerate towards 0
		var t = DecelerationCurve.Evaluate(Math.Abs(MotionCore.Velocity.x) / MaxVelocity);
		t = 1 - t;
		var force = t * MaxDeceleration * -Math.Sign(MotionCore.Velocity.x);
		_movedForce += force;
		MotionCore.ApplyHorizontalForce(force);
		
		// if near 0 stop the movement
		if(Math.Abs(MotionCore.Velocity.x) < MinVelocity)
		{
			// Log.Info($"BREAK!");
			MotionCore.ApplyHorizontalImpulse(-MotionCore.Velocity.x);
			_movedForce = 0;
		}
		
		// Log.Info($"decelerate force: {force} velocity: {MotionCore.Velocity.x} t: {t}");
	}
}
