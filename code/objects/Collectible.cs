using System;
using Sandbox.level;
using Sandbox.player;

namespace Sandbox.objects;

public class Collectible : Component, ICollectible, IRespawn
{
	[Property] public Collider TriggerCollider { get; set; }
	[Property] public GameObject OnCollectEffect { get; set; }
    [Property] public ItemType Type { get; set; }
    [Property] public bool IgnoreRespawn { get; set; }
    
    public bool Saved { get; set; } // level knows it should not respawn
    
	public void Collect( Player player )
	{
		player.Inventory.AddUpgrade( this );
		
		if(OnCollectEffect != null)
		{
			var effect = OnCollectEffect.Clone( Transform.Position );
			effect.Transform.Position = Transform.Position;
		}
		
		GameObject.Enabled = false;
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

	public void Respawn()
	{
		if(Saved)
			return;
		
		GameObject.Enabled = true;
	}
}
