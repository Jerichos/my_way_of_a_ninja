using Sandbox.player;

namespace Sandbox.enemies;

// Heli is a spider enemy that moves back and forth on a platform, from edge to edge.
public class Heli : Component
{
	[Property] private MotionCore2D MotionCore { get; set; }
}
