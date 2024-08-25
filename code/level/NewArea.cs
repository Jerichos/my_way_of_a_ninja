using System;

namespace Sandbox.level;

// new are will set new level/camera bounds
// level will check for new area and set new bounds
public class NewArea : Component
{
	[Property] public BoxCollider TriggerCollider { get; set; }
	
	// from collider bounds
	public Vector2 MinBounds => new Vector2((TriggerCollider.Center.x - TriggerCollider.Scale.x / 2), 
		TriggerCollider.Center.y - TriggerCollider.Scale.y / 2) + (Vector2)Transform.Position;
	public Vector2 MaxBounds => new Vector2((TriggerCollider.Center.x + TriggerCollider.Scale.x / 2), 
		TriggerCollider.Center.y + TriggerCollider.Scale.y / 2) + (Vector2)Transform.Position;

	public Action<NewArea> OnEnter;
	
	protected override void OnEnabled()
	{
		TriggerCollider.OnTriggerEnter += OnTriggerEnter;
	}
	
	protected override void OnDisabled()
	{
		TriggerCollider.OnTriggerEnter -= OnTriggerEnter;
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
