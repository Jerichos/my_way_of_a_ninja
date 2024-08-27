namespace Sandbox.enemies;

public class PathInit : Component
{
	[Property] public bool FromChildren { get; set; }
	[Property] public GameObject PathPointsParent { get; set; } // if FromChildren is true, this will be used to find children
	[Property] public Vector2[] Path { get; set; }
	[Property] public bool Loop { get; set; }

	protected override void OnAwake()
	{
		if ( FromChildren )
		{
			PopulateFromChildren();
		}

		if ( PathPointsParent != null )
		{
			var children = PathPointsParent.Children;
			for ( int i = 0; i < children.Count; i++ )
			{
				children[i].Destroy();
			}
			
			PathPointsParent.Destroy();
		}
	}
	
	private void PopulateFromChildren()
	{
		if ( PathPointsParent == null )
		{
			return;
		}
		
		var children = PathPointsParent.Children;
		Path = new Vector2[children.Count];
		for ( int i = 0; i < children.Count; i++ )
		{
			Path[i] = children[i].Transform.Position;
		}
		
		// must have at least 2 points
		if ( Path.Length < 2 )
		{
			Log.Error($"Path must have at least 2 points {GameObject}");
		}
	}

	protected override void OnValidate()
	{
		if(!FromChildren)
			return;
		
		PopulateFromChildren();
	}

	protected override void DrawGizmos()
	{
		if(Path == null || Path.Length == 0)
			return;
		
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.LineThickness = 3;
		
		for ( int i = 0; i < Path.Length - 1; i++ ) // TODO: path is not draw correctly
		{
			var a = Path[i] - (Vector2)Transform.Position;
			var b = Path[i + 1] - (Vector2)Transform.Position;
			Gizmo.Draw.Line(a, b);
			Gizmo.Draw.SolidCircle(a, 5);
		}
		
		if(Loop)
		{
			var a = Path[^1]- (Vector2)Transform.Position;
			var b = Path[0] - (Vector2)Transform.Position;
			Gizmo.Draw.Line(a, b);
		}
	}
}
