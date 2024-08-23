using System;

namespace Sandbox.player
{
    public class Knockback : Component, IMotionProvider
    {
        [Property] public MotionCore2D MotionCore;
        [Property] public Vector2 Direction;
        [Property] public float MaxDistance;
        [Property] public float Duration;
        [Property] public Curve VelocityCurve;
        [Property] public SoundEvent KnockbackSound;
    
        public Vector2 Velocity { get; private set; }
        public MotionType[] OverrideMotions => new [] { MotionType.JUMP, MotionType.MOVE, MotionType.GRAVITY, MotionType.DASH };

        private float _t;
        private float _distance;
        private float _moreDistance;
        
        public Action KnockbackEndEvent;
    
        public void Activate(Vector2 direction, float moreDistance = 0)
        {
            _t = 0;
            _distance = 0;
            Direction = direction;
            Enabled = true;
            _moreDistance = moreDistance;
            
            if(KnockbackSound != null)
				Sound.Play(KnockbackSound);
        }
    
        protected override void OnFixedUpdate()
        {
	        _t += Time.Delta / Duration;
	        _distance += MotionCore.Velocity.y * Time.Delta;
	        
	        float actualMaxDistance = MaxDistance + _moreDistance;
			
	        // Ensure _t doesn't exceed 1 (end of the dash)
	        if (_t > 1)
	        {
		        _t = 1;
		        EndKnockback();
	        }

	        float curveVelocity = VelocityCurve.Evaluate(_t);
	        float requiredVelocity = actualMaxDistance / Duration;
	        var velocity = requiredVelocity * curveVelocity;

	        _distance += velocity * Time.Delta;

	        if (_distance >= actualMaxDistance)
	        {
		        _distance = actualMaxDistance;
		        _t = 1;  // Ensure the dash finishes
	        }
			
	        // Apply the velocity in the direction the character is facing
	        Velocity = Direction * velocity;
	        Log.Info($"Knockback _t: {_t} distance: {_distance} MaxDistance: {actualMaxDistance} Velocity: {Velocity}");
        }

        private void EndKnockback()
        {
            Enabled = false;
            Velocity = Vector2.Zero;  // Ensure velocity is zero when knockback ends
            KnockbackEndEvent?.Invoke();
        }
    
        protected override void OnEnabled()
        {
            MotionCore.AddMotionProvider(this);
            Activate(Direction);
        }

        protected override void OnDisabled()
        {
            MotionCore.RemoveMotionProvider(this);
        }

        public void CancelMotion()
        {
            Enabled = false;
        }

        public void OnMotionRestored()
        {
            Enabled = true;
        }

        public MotionType MotionType => MotionType.KNOCKBACK;
    }
}
