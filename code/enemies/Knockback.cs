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
    
        public void Activate(Vector2 direction)
        {
            _t = 0;
            _distance = 0;
            Direction = direction;
            Enabled = true;
        }
    
        protected override void OnFixedUpdate()
        {
	        _t += Time.Delta / Duration;
	        _distance += MotionCore.Velocity.y * Time.Delta;
			
	        // Ensure _t doesn't exceed 1 (end of the dash)
	        if (_t > 1)
	        {
		        _t = 1;
		        EndKnockback();
	        }

	        // Evaluate the curve based on normalized time
	        float curveVelocity = VelocityCurve.Evaluate(_t);

	        // Calculate the velocity required to reach the MaxDistance in the given dashIn time
	        float requiredVelocity = MaxDistance / Duration;

	        // Actual velocity is scaled by the curve
	        var velocity = requiredVelocity * curveVelocity;

	        // Update the distance traveled
	        _distance += velocity * Time.Delta;

	        // Check if the dash should be completed
	        if (_distance >= MaxDistance)
	        {
		        _distance = MaxDistance;
		        _t = 1;  // Ensure the dash finishes
	        }
			
	        // Apply the velocity in the direction the character is facing
	        Velocity = Direction * velocity;
	        Log.Info($"Knockback _t: {_t} distance: {_distance} MaxDistance: {MaxDistance} Velocity: {Velocity}");
        }

        private void EndKnockback()
        {
            Enabled = false;
            Velocity = Vector2.Zero;  // Ensure velocity is zero when knockback ends
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

        public void OnMotionCanceled()
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
