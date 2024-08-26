﻿using System;
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

	protected override void OnAwake()
	{
		if(PathInit != null)
			SetPath(PathInit);
	}

	public void SetPath( Vector2[] path, bool loop , int direction = 1)
	{
		Log.Info("path set pathLength: " + path.Length);
		Direction = direction;
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
		// Check if we have a valid path to follow
		if (_path == null || _path.Length == 0)
			return;

		if ( Transform == null )
		{
			Log.Error("Transform is null");
			return;
		}

		// Calculate the direction towards the next point
		Vector2 target = _path[_currentPoint];
		Vector2 direction = (target - (Vector2)Transform.Position).Normal;

		// Move the object
		Velocity = direction * Speed;

		// Check if we reached the target point
		if (Vector2.Distance(Transform.Position, target) < 5f)
		{
			// Move to the next point
			_currentPoint += Direction;

			// Handle looping
			if (_currentPoint >= _path.Length || _currentPoint < 0)
			{
				if (Loop)
				{
					_currentPoint = (_currentPoint + _path.Length) % _path.Length;
				}
				else
				{
					Enabled = false;
					Velocity = Vector2.Zero;

					if ( GameObject.Components.TryGet(out ActivateOnEnter activate, FindMode.InSelf) )
					{
						activate.Enabled = false;
					}
					// If not looping, clamp to the start or end of the path
					// _currentPoint = Math.Clamp(_currentPoint, 0, _path.Length - 1);
					// Direction = -Direction; // Reverse direction
				}
			}
		}
		
		// Log.Info($"FollowPath Velocity: {Velocity} CurrentPoint: {_currentPoint}");
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
}
