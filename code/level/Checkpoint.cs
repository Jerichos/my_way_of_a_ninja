using SpriteTools;

namespace Sandbox.level;

public class Checkpoint : Component
{
	[Property] SpriteComponent SpriteComponent { get; set; }
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
				Log.Info($"checkpoint activated: {_activated} - {this}");
			}
			else
			{
				SpriteComponent.PlayAnimation("deactivated");
			}
		}
	}
	
	public static Checkpoint LastCheckpoint { get; set; }
	
	private void OnTriggerEnter(Collider other)
	{
		if(_activated)
			return;
		
		if (other.GameObject.Components.Get<Player>() == null)
			return;
		
		Sound.Play(SoundEvent, Transform.Position);
		Activated = true;
	}
	
	protected override void OnEnabled()
	{
		Collider.OnTriggerEnter += OnTriggerEnter;
	}

	protected override void OnDisabled()
	{
		Collider.OnTriggerEnter -= OnTriggerEnter;
	}
}
