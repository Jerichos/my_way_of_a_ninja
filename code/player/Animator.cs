using SpriteTools;

namespace Sandbox.player;

public sealed class Animator : Component
{
	[Property] SpriteComponent Sprite { get; set; }
	[Property] MotionCore2D MotionCore { get; set; }

	protected override void OnEnabled()
	{
		
	}

	protected override void OnUpdate()
	{
		if(MotionCore.Grounded)
			Sprite.PlayAnimation("run");
		else if(MotionCore.Velocity.y > 0)
			Sprite.PlayAnimation("jump");
		else
			Sprite.PlayAnimation("inAir");
	}
}
