namespace Sandbox.enemies;

public class ContactDamage : Component
{
	[Property] private BoxCollider ContactDamageCollider { get; set; }
	
	[Property] private int Damage { get; set; } = 1;
	
	private void OnTriggerEnter(Collider other )
	{
		if ( other.GameObject.Components.TryGet( out Player player ) )
		{
			player.TakeDamage(Damage, this);
		}
		
		// Log.Info("ContactDamage OnTriggerEnter");
	}
	
	protected override void OnEnabled()
	{
		ContactDamageCollider.OnTriggerEnter += OnTriggerEnter;
	}
	
	protected override void OnDisabled()
	{
		ContactDamageCollider.OnTriggerEnter -= OnTriggerEnter;
	}
}
