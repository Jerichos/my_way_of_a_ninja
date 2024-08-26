using System;

namespace Sandbox.level;

// new are will set new level/camera bounds
// level will check for new area and set new bounds
public class NewArea : Component
{
	[Property] public BoxCollider BoundsCollider { get; set; }
	[Property] public BoxCollider EnterCollider { get; set; }
	[Property] public BoxCollider ExitCollider { get; set; }
	[Property] public float TransitionMultiplier { get; set; } = 2.0f;
	
	// if null, don't change sound
	// if not null, transition to this sound
	[Property] public SoundEvent AreaSound { get; set; }
	
	[Property] public bool MuteSound { get; set; }
	
	[Property] public bool WeatherEnabled { get; set; }
	
	// from collider bounds
	public Vector2 MinBounds => new Vector2((BoundsCollider.Center.x - BoundsCollider.Scale.x / 2), 
		BoundsCollider.Center.y - BoundsCollider.Scale.y / 2) + (Vector2)Transform.Position;
	public Vector2 MaxBounds => new Vector2((BoundsCollider.Center.x + BoundsCollider.Scale.x / 2), 
		BoundsCollider.Center.y + BoundsCollider.Scale.y / 2) + (Vector2)Transform.Position;

	public Action<NewArea> OnEnter;
	
	protected override void OnEnabled()
	{
		BoundsCollider.OnTriggerEnter += OnTriggerEnter;
		BoundsCollider.Enabled = true;
	}
	
	protected override void OnDisabled()
	{
		BoundsCollider.OnTriggerEnter -= OnTriggerEnter;
		BoundsCollider.Enabled = false;
	}

	private void OnTriggerEnter( Collider obj )
	{
		if ( obj.GameObject.Components.TryGet(out Player player) )
		{
			Log.Info("New area entered");
			OnEnter?.Invoke(this);
		}
	}
}
