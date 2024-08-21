using Sandbox.player;

namespace Sandbox.enemies
{
// Dynamically move around the environment
// NOTE: not working
// NOTE2: fuck this, no time for this shit. I will implement manual move around instead...
public class MoveAround : Component, IMotionProvider
{
    [Property] private MotionCore2D MotionCore { get; set; }
    [Property] private Vector2 Direction { get; set; } = Util.RightX; // default direction
    [Property] private float Speed { get; set; } = 300;

    public Vector2 Velocity { get; private set; }
    public MotionType[] OverrideMotions => new[] { MotionType.GRAVITY };
    public MotionType MotionType => MotionType.MOVE;

    private bool _stickToGround;
    private Vector2 _currentDirection;

    protected override void OnFixedUpdate()
    {
        // Wait until grounded if not yet sticking to the ground
        if (!MotionCore.Grounded && !_stickToGround)
        {
            MotionCore.RemoveMotionProvider(this);
            return;
        }

        // Recursively find a valid direction
        if (GetDirectionRec(Direction, out Vector2 newDirection))
        {
            _currentDirection = newDirection;
        }

        // Apply the movement
        Velocity = _currentDirection * Speed;
    }

    private bool GetDirectionRec(Vector2 checkDirection, out Vector2 newDirection)
    {
        // Define available directions
        Vector2[] availableDirections = 
        {
            new( 1,  0),  // Right
            new( 0,  1),  // Up
            new(-1,  0), // Left
            new( 0, -1)  // Down
        };

        foreach (var direction in availableDirections)
        {
            // Check if the direction is clear of collisions
            if (!MotionCore.CheckCollision(MotionCore.Center, direction * Speed * Time.Delta * 50))
            {
                newDirection = direction;
                Log.Info($"set direction: {newDirection}");
                return true; // Valid direction found
            }
        }

        newDirection = Vector2.Zero;
        return false; // No valid direction found
    }

    public void OnMotionCanceled()
    {
        Enabled = false;
    }

    public void OnMotionRestored()
    {
        Enabled = true;
    }

    protected override void OnEnabled()
    {
        MotionCore.GroundHitEvent += OnGroundHit;
        _stickToGround = false;
        _currentDirection = Direction; // Set the initial direction
    }

    protected override void OnDisabled()
    {
        MotionCore.GroundHitEvent -= OnGroundHit;
    }

    private void OnGroundHit()
    {
        if (_stickToGround)
            return;

        MotionCore.AddMotionProvider(this);
        _stickToGround = true;
    }
}
}
