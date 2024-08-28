using System;
using Sandbox.level;
using Sandbox.objects;
using Sandbox.player;

namespace Sandbox.enemies;

public class FollowPath : Component, IMotionProvider, IRespawn
{
	[Property] private MotionCore2D MotionProvider { get; set; }
	[Property] private PathInit PathInit { get; set; }
	[Property] private float Speed { get; set; } = 100;
	[Property] private bool IgnoreGravity { get; set; }
	[Property] private bool Loop { get; set; }
	[Property] [Range(-1, 1)] private int Direction = 1;
	[Property] public bool IgnoreRespawn { get; set; }
	[Property] public bool DontStartFromFirstPoint { get; set; }

	private Vector2[] _path;
	private int _currentPoint;
	
	public Vector2 Velocity { get; private set; }
	public MotionType[] OverrideMotions => IgnoreGravity? new[] { MotionType.GRAVITY }: Array.Empty<MotionType>();
	public MotionType MotionType => MotionType.MOVE;
	
	private int _defaultDirection;

	protected override void OnAwake()
	{
		_defaultDirection = Direction;
		if(PathInit != null)
			SetPath(PathInit);
	}

	public void SetPath( Vector2[] path, bool loop , int direction = 1)
	{
		Direction = direction;
		_defaultDirection = direction;
		_path = new Vector2[path.Length];
		for ( int i = 0; i < path.Length; i++ )
		{
			_path[i] = path[i];
		}
		
		Loop = loop;
		_currentPoint = 0;
		if(!DontStartFromFirstPoint)
			Transform.Position = _path[_currentPoint];
		
		MotionProvider.AddMotionProvider(this);
	}

	public void SetPath( PathInit pathInit )
	{
		SetPath(pathInit.Path, pathInit.Loop);
	}

	protected override void OnFixedUpdate()
	{
	    if (_path == null || _path.Length == 0)
	        return;

	    if (Transform == null)
	        return;

	    Vector2 currentTarget = _path[_currentPoint];
	    Vector2 directionToTarget = (currentTarget - (Vector2)Transform.Position).Normal;

	    int nextPoint = _currentPoint + Direction;
	    if (nextPoint >= 0 && nextPoint < _path.Length)
	    {
	        Vector2 nextTarget = _path[nextPoint];
	        Vector2 directionToNextTarget = (nextTarget - (Vector2)Transform.Position).Normal;

	        if (Vector2.Distance(Transform.Position, nextTarget) < Vector2.Distance(Transform.Position, currentTarget))
	        {
	            _currentPoint = nextPoint;
	            directionToTarget = directionToNextTarget;
	            currentTarget = nextTarget;
	        }
	    }

	    Velocity = directionToTarget * Speed;

	    if (Vector2.Distance(Transform.Position, currentTarget) < 5f)
	    {
	        _currentPoint += Direction;

	        if (_currentPoint >= _path.Length || _currentPoint < 0)
	        {
	            if (Loop)
	            {
	                Direction = -Direction;
	                _currentPoint += Direction;

	                _currentPoint = Math.Clamp(_currentPoint, 0, _path.Length - 1);
	            }
	            else
	            {
	                _currentPoint = Math.Clamp(_currentPoint, 0, _path.Length - 1);
	                Velocity = Vector2.Zero;
	                Enabled = false;
	            }
	        }
	    }
	}

	public void CancelMotion()
	{
		Velocity = new Vector2(0, 0);
		Enabled = false;
	}

	public void OnMotionRestored()
	{
		Enabled = true;
	}

	protected override void OnDisabled()
	{
		CancelMotion();
	}

	protected override void OnValidate()
	{
		if ( Components.TryGet( out PathInit pathInit, FindMode.InChildren ) )
		{
			if ( pathInit.Path == null || pathInit.Path.Length < 2 )
			{
				Log.Error("Path must have at least 2 points");
			}
		}

		// dir must be 1 or -1
		Direction = Math.Clamp(Direction, -1, 1);
	}

	public void Respawn()
	{
		_currentPoint = 0;
		Transform.Position = _path[_currentPoint];
	}

	public void GoBack()
	{
		Direction = -_defaultDirection;
		Log.Info("move back");
		Enabled = true;  // Ensure that movement resumes if it was previously disabled
	}

	public void GoForward()
	{
		Direction = _defaultDirection;
		Log.Info("move forward");
		Enabled = true;  // Ensure that movement resumes if it was previously disabled
	}
}
