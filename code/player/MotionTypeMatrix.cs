using System;

namespace Sandbox.player;

public class MotionTypeMatrix
{
	private readonly Dictionary<(MotionType, MotionType), Func<bool>> _rules;

	public MotionTypeMatrix()
	{
		_rules = new Dictionary<(MotionType, MotionType), Func<bool>>()
		{
			// Gravity is canceled when Jump is active
			{ (MotionType.GRAVITY, MotionType.JUMP), () => false },
			{ (MotionType.JUMP, MotionType.GRAVITY), () => true },

			// Move is ignored when Dash is active
			{ (MotionType.MOVE, MotionType.DASH), () => false },
			{ (MotionType.DASH, MotionType.MOVE), () => true },

			// Dash is ignored when Jump is active
			{ (MotionType.DASH, MotionType.JUMP), () => false },
			{ (MotionType.JUMP, MotionType.DASH), () => true },

			// Example: Other types default to true (additive)
			// This is a fallback for unlisted combinations
			{ (MotionType.ENVIRONMENT, MotionType.GRAVITY), () => true },
			{ (MotionType.GRAVITY, MotionType.ENVIRONMENT), () => true }
			// Add other rules as necessary...
		};
	}

	public bool ShouldCombine(MotionType type1, MotionType type2)
	{
		if (_rules.TryGetValue((type1, type2), out var rule))
		{
			return rule();
		}

		// Default behavior when no specific rule exists
		return true;
	}
}
