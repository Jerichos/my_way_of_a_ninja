namespace Sandbox.player;

public class CameraFollow : Component
{
	[Property] public GameObject Target { get; set; }
	
	[Property] public float SmoothTime { get; set; } = 0.3f;
	[Property] public Vector3 Offset { get; set; }

	protected override void OnFixedUpdate()
	{
		if(Target == null)
			return;
		
		var targetPosition = Target.Transform.Position + Offset;
		var smoothPosition = Vector3.Lerp(Transform.Position, targetPosition, SmoothTime);
		Transform.Position = smoothPosition;
	}

	public void SetTarget( GameObject target, bool teleport = false )
	{
		Target = target;
		if(teleport)
			Transform.Position = target.Transform.Position + Offset;
	}
}
