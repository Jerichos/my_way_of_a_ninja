using System;
using Sandbox.player;

namespace Sandbox.objects;

public class Upgrade : Component, ICollectible
{
	[Property] public Collider TriggerCollider { get; set; }
	[Property] public GameObject OnCollectEffect { get; set; }
    [Property] public UpgradeType Type { get; set; }
    
	public void Collect( Player player )
	{
		player.Upgrades.AddUpgrade( this );
		
		if(OnCollectEffect != null)
		{
			var effect = OnCollectEffect.Clone( Transform.Position );
			effect.Transform.Position = Transform.Position;
		}
		
		GameObject.Destroy();
	}

	private void OnTriggerEnter( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			Collect( player );
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

	protected override void OnValidate()
	{
		GameObject.Name = $"Upgrade_{Type}";
	}
}
