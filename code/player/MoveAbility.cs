using System;

namespace Sandbox.player;

public class MoveAbility : Component, IMotionProvider
{
	[Property] MotionCore2D MotionCore { get; set; }
	
	[Property] public float MaxSpeedIn { get; set; } = 1;
	[Property] public float MaxAcceleration { get; set; } = 1000;
	[Property] public float MaxDeceleration { get; set; } = 1000;
	[Property] public float MaxVelocity { get; set; } = 100;
	
	[Property] public Curve AccelerationCurve { get; set; }
	[Property] public Curve DecelerationCurve { get; set; }
	
	public Vector2 Velocity { get; private set; }
	public int Priority => 0;
	public bool Additive => true;
	public MotionType MotionType => MotionType.MOVE;
	
	private int _inputX;
	public int InputX
	{
		get => _inputX;
		private set
		{
			if (_inputX == value)
				return;

			_time = 0;
			_inputX = value;
			MotionCore.Facing = _inputX;
			Log.Info("Changed inputX: " + _inputX);
		}
	}
	
	private float _lastInputX;
	private float _movedForce;
	private float _time;
	
	protected override void OnUpdate()
	{
		if ( Input.Down( "Right" ) )
		{
			InputX = 1;
		}
		else if ( Input.Down( "Left" ) )
			InputX = -1;
		else
			InputX = 0;
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
		
		_time += Time.Delta / MaxSpeedIn;
		
		if(_time > 1)
			_time = 1;
		
		float force = AccelerationCurve.Evaluate(_time) * MaxAcceleration;
		Log.Info($"Move Ability t: {_time} force: {force} velocity: {Velocity.x}");
		Velocity = new Vector2(force * _inputX, 0);
	}
	
	private void HandleDeceleration()
	{
		if(_inputX != 0 || MotionCore.Velocity.x == 0)
			return;

		Velocity = Vector2.Zero;
		
		// TODO: implement deceleration or leave it be because there is no time. So what
	}
	
	public void OnVelocityIgnored()
	{
		Velocity = Vector2.Zero;
	}

	protected override void OnEnabled()
	{
		MotionCore.AddMotionProvider(this);
	}
	
	protected override void OnDisabled()
	{
		MotionCore.RemoveMotionProvider(this);
	}
}
