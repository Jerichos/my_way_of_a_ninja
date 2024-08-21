namespace Sandbox.enemies;

public class PathInit : Component
{
	[Property] public bool FromChildren { get; set; }
	[Property] public Vector2[] Path { get; set; }
	[Property] public bool Loop { get; set; }

	protected override void OnAwake()
	{
		if ( FromChildren )
		{
			PopulateFromChildren();
		}
		
		var children = GameObject.Children;
		for ( int i = 0; i < children.Count; i++ )
		{
			children[i].Destroy();
		}
	}
	
	private void PopulateFromChildren()
	{
		var children = GameObject.Children;
		Path = new Vector2[children.Count];
		for ( int i = 0; i < children.Count; i++ )
		{
			Path[i] = children[i].Transform.Position;
		}
	}

	protected override void OnStart()
	{
		Destroy();
	}

	protected override void OnValidate()
	{
		if(!FromChildren)
			return;
		
		PopulateFromChildren();
		
		Log.Info($"found {Path.Length} children");
	}
	
	protected override void DrawGizmos()
	{
		if(Path == null || Path.Length == 0)
			return;
		
		Gizmo.Draw.Color = Color.Magenta;
		Gizmo.Draw.LineThickness = 5;
		
		for ( int i = 0; i < Path.Length - 1; i++ )
		{
			var a = Path[i] - (Vector2)Transform.Position;
			var b = Path[i + 1] - (Vector2)Transform.Position;
			Gizmo.Draw.Line(a, b);
		}
		
		if(Loop)
		{
			Gizmo.Draw.Line(Path[Path.Length - 1] - (Vector2)Transform.Position, Path[0] - (Vector2)Transform.Position);
		}
	}
}
