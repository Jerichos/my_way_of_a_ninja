using System;

namespace Sandbox.player;

// dash horizontally
public class DashAbility : Component, IMotionProvider
{
	[Property] private Player Player { get; set; }
	private MotionCore2D MotionCore => Player.MotionCore;
	
	[Property] public float Cooldown { get; set; } = 2; // TODO: UI this
	
	[Property] public bool CanDashInAir { get; set; } = true;
	[Property] public float MaxDistance { get; set; } = 1000;
	[Property] public float DashIn { get; set; } = 1;
	[Property] public Curve VelocityCurve { get; set; }
	[Property] private SoundEvent DashSound { get; set; }
	[Property] private ParticleEmitter DashEffect { get; set; }
	[Property] private Texture DashTexture { get; set; }
	[Property] private Texture DashTextureFlipped { get; set; }
	
	public Vector2 Velocity { get; private set; }
	public MotionType MotionType => MotionType.DASH;
	public MotionType[] OverrideMotions => new[] {MotionType.MOVE, MotionType.JUMP, MotionType.GRAVITY};

	private float _colliderDashHeight = 32;
	private float _defaultColliderHeight;
	private float _defaultColliderCenterY;

	public bool IsDashing => _isIsDashing;
	
	private float _t;
	private float _distance;
	private float _cooldownTimer;
	
	private bool _isIsDashing;

	protected override void OnStart()
	{
		_defaultColliderHeight = MotionCore.Collider.Scale.y;
		_defaultColliderCenterY = MotionCore.Collider.Center.y;
		DashEffect.Enabled = false;
	}

	public void StartDash()
	{
		if(!CanDash())
			return;
		
		_isIsDashing = true;
		_t = 0;
		_cooldownTimer = Cooldown;
		_distance = 0;
		MotionCore.AddMotionProvider(this);
		
		// set collider height and center
		MotionCore.Collider.Scale = MotionCore.Collider.Scale.WithY(_colliderDashHeight);
		MotionCore.Collider.Center = MotionCore.Collider.Center.WithY(_colliderDashHeight / 2);
		
		Sound.Play(DashSound, Transform.Position);

		DashEffect.Enabled = true;
	}
	
	protected override void OnFixedUpdate()
	{
		if(_cooldownTimer > 0)
		{
			_cooldownTimer -= Time.Delta;
		}
		
		if(_isIsDashing)
		{
			_t += Time.Delta / DashIn;
			if(_distance < MaxDistance && MotionCore.Collisions.Right == false && MotionCore.Collisions.Left == false)
			{
				// Ensure _t doesn't exceed 1 (end of the dash)
				if (_t > 1)
				{
					_t = 1;
				}

				// Evaluate the curve based on normalized time
				float curveVelocity = VelocityCurve.Evaluate(_t);

				// Calculate the velocity required to reach the MaxDistance in the given dashIn time
				float requiredVelocity = MaxDistance / DashIn;

				// Actual velocity is scaled by the curve
				var velocity = requiredVelocity * curveVelocity;

				// Update the distance traveled
				_distance += velocity * Time.Delta;

				// Apply the velocity in the direction the character is facing
				Velocity = Util.RightX * velocity * MotionCore.Facing;

				// Check if the dash should be completed
				if (_distance >= MaxDistance)
				{
					_distance = MaxDistance;
					_t = 1;  // Ensure the dash finishes
				}
			}
			else
			{
				Velocity = Vector2.Zero;
				_isIsDashing = false;
				MotionCore.RemoveMotionProvider(this);
				
				// reset collider height and center
				MotionCore.Collider.Scale = MotionCore.Collider.Scale.WithY(_defaultColliderHeight);
				MotionCore.Collider.Center = MotionCore.Collider.Center.WithY(_defaultColliderCenterY);
				
				DashEffect.Enabled = false;
			}
		}
	}

	private bool CanDash()
	{
		if(Player.Inventory.Enabled && !Player.Inventory.HasUpgrade(ItemType.DASH, out var value))
			return false;
		
		if(!CanDashInAir && !MotionCore.Grounded || _isIsDashing || _cooldownTimer > 0)
			return false;
		
		return true;
	}
	
	public void OnMotionRestored()
	{
		
	}

	private void OnFacingChanged( int facing )
	{
		if(facing == 1)
			DashEffect.Components.Get<ParticleSpriteRenderer>().Texture = DashTexture;
		else
			DashEffect.Components.Get<ParticleSpriteRenderer>().Texture = DashTextureFlipped;
	}
	
	protected override void OnEnabled()
	{
		MotionCore.FacingChangedEvent += OnFacingChanged;
	}

	protected override void OnDisabled()
	{
		MotionCore.FacingChangedEvent -= OnFacingChanged;
	}


	public void CancelMotion()
	{
		Velocity = Vector2.Zero;
		_isIsDashing = false;
		MotionCore.RemoveMotionProvider(this);
		DashEffect.Enabled = false;
	}
}
