﻿using System;

namespace Sandbox.player;

public enum MotionType
{
	GRAVITY,
	JUMP,
	MOVE,
	DASH,
	ENVIRONMENT,
	KNOCKBACK
}

public interface IMotionProvider
{
	public Vector2 Velocity { get; }
	public MotionType[] OverrideMotions { get; }
	public void OnMotionCanceled();
	public void OnMotionRestored();
	public MotionType MotionType { get; }
}
