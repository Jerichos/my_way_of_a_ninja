namespace Sandbox.level;

public class Level : Component
{
	[Property] public GameObject StartCheckpoint { get; set; }

	protected override void OnStart()
	{
		if ( Checkpoint.LastCheckpoint == null )
		{
			if ( StartCheckpoint != null )
			{
				
				StartCheckpoint.Components.Get<Checkpoint>().Activated = true;
			}
			else
			{
				Log.Warning("No start checkpoint set");
			}
		}
	}
}
