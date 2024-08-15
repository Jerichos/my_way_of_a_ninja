using Sandbox;
using Sandbox.player;
using SpriteTools;

public sealed class Player : Component
{
	[Property] private SpriteComponent SpriteComponent { get; set; }
	[Property] private MotionCore2D MotionCore { get; set; }
	[Property] private MoveAbility MoveAbility { get; set; }
	
	// facing
	private int _facing = 1; // default is right

	public int Facing
	{
		get => _facing;
		set
		{
			if(_facing == value)
				return;
			
			_facing = value;
			SpriteComponent.SpriteFlags = _facing == 1 ? SpriteFlags.None : SpriteFlags.HorizontalFlip;
		}
	}


	protected override void OnEnabled()
	{
		if ( MoveAbility != null )
		{
			MoveAbility.InputXChangedEvent += OnInputXChanged;
			OnInputXChanged(MoveAbility.InputX);
		}
	}

	private void OnInputXChanged( int inputX )
	{
		if(inputX == 0)
			return;
		
		Facing = inputX;
	}
}
