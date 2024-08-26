using Sandbox.level;
using SpriteTools;

namespace Sandbox.enemies;

public class BallSpawn : Component, IRespawn
{
	[Property] private PathInit PathInit { get; set; }
	[Property] GameObject BallPrefab { get; set; }
	[Property] private int StartDirection { get; set; } = 1;
	[Property] private float Velocity { get; set; } = 64;
	[Property] public bool IgnoreRespawn { get; set; }

	private GameObject newBall;

	protected override void OnAwake()
	{
		Spawn();
	}

	public void Spawn()
	{
		if(newBall == null)
		{
			newBall = BallPrefab.Clone();
		}
		
		
		if (Components.TryGet(out Level level, FindMode.InAncestors))
		{
			newBall.SetParent(level.GameObject);
			Log.Info("Enemy component is a child of a Level component.");
		}
		else
		{
			// TODO(bug): log is not invoked in OnAwake
			Log.Warning("Enemy component must be a child of a Level component for respawn. " + GameObject);
		}
		
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

	public void Respawn()
	{
		Spawn();
	}
}
