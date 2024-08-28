using Sandbox.objects;

namespace Sandbox.level;

public class ChangeMovingPlatformParameters : Component
{
	[Property] private MovingPlatform MovingPlatform {get; set;}

	protected override void OnDisabled()
	{
		MovingPlatform.MoveOnlyWhenPlayerOn = false;
	}
}
