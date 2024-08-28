using System;

namespace Sandbox.level;

// new are will set new level/camera bounds
// level will check for new area and set new bounds
public class NewArea : Component
{
	[Property] public BoxCollider BoundsCollider { get; set; }
	[Property] public float TransitionMultiplier { get; set; } = 2.0f;
	[Property] public bool BossArea { get; set; }
	
	// if null, don't change sound
	// if not null, transition to this sound
	[Property] public SoundEvent AreaSound { get; set; }
	[Property] public bool MuteSound { get; set; }
	[Property] public bool WeatherEnabled { get; set; }
	[Property] private GameObject[] EnableOnEnter { get; set; }
	[Property] private Component[] DisableOnEnter { get; set; }
	
	[Property] private bool DontDisableOnExit { get; set; }
	
	// from collider bounds
	public Vector2 MinBounds => new Vector2((BoundsCollider.Center.x - BoundsCollider.Scale.x / 2), 
		BoundsCollider.Center.y - BoundsCollider.Scale.y / 2) + (Vector2)Transform.Position;
	public Vector2 MaxBounds => new Vector2((BoundsCollider.Center.x + BoundsCollider.Scale.x / 2), 
		BoundsCollider.Center.y + BoundsCollider.Scale.y / 2) + (Vector2)Transform.Position;

	public Action<NewArea> EnterEvent;
	public Action<NewArea> ExitEvent;
	
	protected override void OnEnabled()
	{
		BoundsCollider.OnTriggerEnter += OnTriggerEnter;
		BoundsCollider.OnTriggerExit += OnTriggerExit;
		BoundsCollider.Enabled = true;
	}
	
	protected override void OnDisabled()
	{
		BoundsCollider.OnTriggerEnter -= OnTriggerEnter;
		BoundsCollider.OnTriggerExit -= OnTriggerExit;
		BoundsCollider.Enabled = false;
	}

	private void OnTriggerEnter( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			Log.Info("New area entered: " + GameObject.Name);
			OnAreaEnter();
		}
	}
	
	private void OnTriggerExit( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			Log.Info("New area exited " + GameObject.Name);
			OnAreaExit();
		}
	}

	private void OnAreaExit()
	{
		if(!DontDisableOnExit)
			for ( int i = 0; i < EnableOnEnter.Length; i++ )
				EnableOnEnter[i].Enabled = false;
		
		if(DisableOnEnter != null)
			for ( int i = 0; i < DisableOnEnter.Length; i++ )
				DisableOnEnter[i].Enabled = true;
			
		ExitEvent?.Invoke(this);
	}

	private void OnAreaEnter()
	{
		if(EnableOnEnter != null)
			for ( int i = 0; i < EnableOnEnter.Length; i++ )
				EnableOnEnter[i].Enabled = true;
		
		if(DisableOnEnter != null)
			for ( int i = 0; i < DisableOnEnter.Length; i++ )
				DisableOnEnter[i].Enabled = false;
		
		EnterEvent?.Invoke(this);
	}
}
