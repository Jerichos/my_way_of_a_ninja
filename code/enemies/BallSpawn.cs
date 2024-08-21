using SpriteTools;

namespace Sandbox.enemies;

public class BallSpawn : Component
{
	[Property] private PathInit PathInit { get; set; }
	[Property] GameObject BallPrefab { get; set; }
	[Property] private int StartDirection { get; set; } = 1;

	protected override void OnAwake()
	{
		var newBall = BallPrefab.Clone();
		
		if(newBall.Components.TryGet(out FollowPath followPath))
		{
			followPath.SetPath(PathInit.Path, PathInit.Loop, StartDirection);
		}
		else
		{
			Log.Error("FollowPath not found");
		}
	}

	protected override void DrawGizmos()
	{
		// is there way to draw a sprite of a prefab?

		if ( BallPrefab.Components.TryGet( out SpriteComponent spriteComponent ) )
		{
			Texture texture = spriteComponent.Sprite.GetPreviewTexture();
			Gizmo.Draw.Sprite(Transform.Position, 50, texture);
		}
		else
		{
			Gizmo.Draw.Color = Color.Magenta;
			Gizmo.Draw.LineSphere(Transform.LocalPosition, 50);
		}
	}
}
