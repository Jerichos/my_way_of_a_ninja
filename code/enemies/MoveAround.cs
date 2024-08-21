using Sandbox.player;

namespace Sandbox.enemies;

// move around the platform
public class MoveAround : Component, IMotionProvider
{
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private int Direction { get; set; } = 1;
	[Property] private float Speed { get; set; } = 300;
	
	public Vector2 Velocity { get; private set; }
	public MotionType[] OverrideMotions => new[] { MotionType.GRAVITY };
	public MotionType MotionType => MotionType.MOVE;

	protected override void OnAwake()
	{
		MotionCore.AddMotionProvider(this);
	}

	protected override void OnFixedUpdate()
	{
		if(MotionCore.Collisions.Right || MotionCore.Collisions.Left)
		{
			Direction *= -1;
		}
		
		Velocity = new Vector2(Direction * Speed, 0);
	}

	public void OnMotionCanceled()
	{
		Enabled = false;
	}

	public void OnMotionRestored()
	{
		Enabled = true;
	}

}
