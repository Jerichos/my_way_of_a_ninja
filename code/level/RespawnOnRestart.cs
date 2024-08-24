namespace Sandbox.level;

public class RespawnOnRestart : Component, IRespawn
{
	private Vector3 _startPosition;
	
	protected override void OnAwake()
	{
		_startPosition = Transform.Position;
	}
	
	public void Respawn()
	{
		Transform.Position = _startPosition;
		Enabled = true;
	}
}
