namespace Sandbox.level;

public class KillZone : Component
{
	[Property] private Collider TriggerCollider { get; set; }
	
	private void OnTriggerEnter( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			player.Kill();
		}
	}
	
	protected override void OnEnabled()
	{
		TriggerCollider.OnTriggerEnter += OnTriggerEnter;
	}
	
	protected override void OnDisabled()
	{
		TriggerCollider.OnTriggerEnter -= OnTriggerEnter;
	}
}
