using System;

namespace Sandbox.enemies;

public class MoveToPosition : Component
{
	[Property] private float Speed { get; set; } = 100;
	
	private Vector3 _targetPosition;
	
	public Action OnArrived;

	private bool _isMoving;
	
	public void MoveTo(Vector3 targetPosition, float speed = 100)
	{
		Log.Info("bird target position: " + targetPosition);
		_targetPosition = targetPosition;
		Speed = speed;
		_isMoving = true;
	}

	protected override void OnFixedUpdate()
	{
		if (!_isMoving)
			return;
		
		var direction = (_targetPosition - Transform.Position).Normal;
		var distance = (_targetPosition - Transform.Position).Length;

		if (distance < Speed * Time.Delta)
		{
			Transform.Position = _targetPosition;
			_isMoving = false;
			OnArrived?.Invoke();
			return;
		}

		Transform.Position += direction * Speed * Time.Delta;
	}
}
