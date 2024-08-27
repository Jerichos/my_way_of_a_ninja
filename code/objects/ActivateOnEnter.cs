using Sandbox.level;

namespace Sandbox.objects;

public class ActivateOnEnter : Component, IRespawn
{
	[Property] public Component Target { get; set; }
	[Property] public BoxCollider TriggerCollider { get; set; }
	[Property] public bool IgnoreRespawn { get; set; }

	protected override void OnStart()
	{
		Target.Enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if ( other.GameObject.Components.TryGet( out Player player ) )
		{
			Target.Enabled = true;
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

	public void Respawn()
	{
		Enabled = true;
		Target.Enabled = false;
	}
}
