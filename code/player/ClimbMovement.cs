using Sandbox.objects;

namespace Sandbox.player;

public class ClimbMovement : Component, IMotionProvider
{
	[Property] public MotionCore2D MotionCore { get; set; }
	public Vector2 Velocity { get; private set; }
	public MotionType[] OverrideMotions => new[] {MotionType.ALL}; // overrides all
	public MotionType MotionType => MotionType.CLIMB;
	
	private Ladder _ladder; // or something else to climb, ICLimbable?

	public void TryClimbUp()
	{
		if ( _ladder == null )
		{
			Log.Info("nothing to climb on");
			return;
		}
		
		Log.Info($"climbing up {_ladder}");
	}
	
	public void TryClimbDown()
	{
		if ( _ladder == null )
		{
			Log.Info("nothing to climb on");
			return;
		}
		
		Log.Info($"climbing down {_ladder}");
	}
	
	public void OnMotionCanceled()
	{
		Enabled = false;
	}

	public void OnMotionRestored()
	{
		Enabled = true;
	}
	
	private void OnTriggerEnter( Collider obj )
	{
		Log.Info("Ladder entered");
		if(obj.GameObject.Components.TryGet(out Ladder ladder))
		{
			_ladder = ladder;
			Log.Info("Ladder entered");
		}
	}

	private void OnTriggerExit( Collider obj )
	{
		Log.Info("Ladder exited");
		if(obj.GameObject.Components.TryGet(out Ladder ladder))
		{
			_ladder = null; // two ladders should not overlap right, right?
			Log.Info("Ladder exited");
		}
	}

	protected override void OnEnabled()
	{
		MotionCore.Collider.OnTriggerEnter += OnTriggerEnter;
		MotionCore.Collider.OnTriggerExit += OnTriggerExit;
		MotionCore.AddMotionProvider(this);
	}

	protected override void OnDisabled()
	{
		MotionCore.Collider.OnTriggerEnter -= OnTriggerEnter;
		MotionCore.Collider.OnTriggerExit -= OnTriggerExit;
		MotionCore.RemoveMotionProvider(this);
	}
}
