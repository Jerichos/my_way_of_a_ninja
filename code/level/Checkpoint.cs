using SpriteTools;

namespace Sandbox.level;

public class Checkpoint : Component
{
	[Property] SpriteComponent SpriteComponent { get; set; }
	[Property] SpriteComponent PendingSpriteComponent { get; set; }
	[Property] BoxCollider Collider { get; set; }
	[Property] SoundEvent SoundEvent { get; set; }

	private bool _activated;
	public bool Activated
	{
		get => _activated;
		set
		{
			if(_activated == value)
				return;
			
			_activated = value;

			if (_activated )
			{
				if(LastCheckpoint != null && LastCheckpoint != this)
					LastCheckpoint.Activated = false;
				
				LastCheckpoint = this;
				SpriteComponent.PlayAnimation("activated");
				PendingSpriteComponent.PlaybackSpeed = 1;
			}
			else
			{
				SpriteComponent.PlayAnimation("deactivated");
				PendingSpriteComponent.PlaybackSpeed = 0;
			}
		}
	}
	
	public static Checkpoint LastCheckpoint { get; set; }
	
	private void OnTriggerEnter(Collider other)
	{
		if ( other.GameObject.Components.TryGet( out Player player ) )
		{
			player.Inventory.SaveProgress();
			
			if(_activated)
				return;
			
			player.RestoreHealth();
			Sound.Play(SoundEvent, Transform.Position);
			Activated = true;
		}
	}
	
	protected override void OnEnabled()
	{
		Collider.OnTriggerEnter += OnTriggerEnter;
	}

	protected override void OnDisabled()
	{
		Collider.OnTriggerEnter -= OnTriggerEnter;
	}

	public void PendingItem( bool isPending )
	{
		if ( PendingSpriteComponent == null )
		{
			Log.Error($"PendingSpriteComponent is not set {GameObject}");
			return;
		}
		
		
		PendingSpriteComponent.Enabled = isPending;
	}
}
