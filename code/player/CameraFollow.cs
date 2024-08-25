using System;
using System.Collections;
using Sandbox.player;

public class CameraFollow : Component
{
    [Property] public GameObject Target { get; set; }

    [Property] public float SmoothTime { get; set; } = 0.3f;
    [Property] public Vector3 Offset { get; set; }

    [Property] CameraComponent Camera { get; set; }

    [Property] public Vector2 MinBounds { get; set; }
    [Property] public Vector2 MaxBounds { get; set; }

    private Vector2 _targetMinBounds;
    private Vector2 _targetMaxBounds;
    private bool _smoothTransition = false;
    private const float TRANSITION_SPEED_MULTIPLIER = 2f;

    protected override void OnFixedUpdate()
    {
        if (Target == null)
            return;

        var targetPosition = Target.Transform.Position + Offset;
        var smoothPosition = Vector3.Lerp(Transform.Position, targetPosition, SmoothTime);

        float height = Camera.OrthographicHeight;
        float halfHeight = height / 2;

        float width = Camera.OrthographicHeight * Screen.Aspect;
        float halfWidth = width / 2;

        // Smoothly transition bounds if smoothTransition is active
        if (_smoothTransition)
        {
            MinBounds = Vector2.Lerp(MinBounds, _targetMinBounds, Time.Delta * TRANSITION_SPEED_MULTIPLIER / SmoothTime);
            MaxBounds = Vector2.Lerp(MaxBounds, _targetMaxBounds, Time.Delta * TRANSITION_SPEED_MULTIPLIER / SmoothTime);

            // Stop transition when close enough to the target bounds
            if ((MinBounds - _targetMinBounds).LengthSquared < 0.001f && (MaxBounds - _targetMaxBounds).LengthSquared < 0.001f)
            {
                MinBounds = _targetMinBounds;
                MaxBounds = _targetMaxBounds;
                _smoothTransition = false;
            }
        }

        smoothPosition.x = smoothPosition.x.Clamp(MinBounds.x + halfWidth, MaxBounds.x - halfWidth);
        smoothPosition.y = smoothPosition.y.Clamp(MinBounds.y + halfHeight, MaxBounds.y - halfHeight);

        Transform.Position = smoothPosition;
    }

    public void SetBounds(Vector2 min, Vector2 max, bool smoothly = false)
    {
        if (smoothly)
        {
            _targetMinBounds = min;
            _targetMaxBounds = max;
            _smoothTransition = true;
        }
        else
        {
            MinBounds = min;
            MaxBounds = max;
        }
    }

    public void SetTarget(GameObject target, bool teleport = false)
    {
        Target = target;
        if (teleport)
            Transform.Position = target.Transform.Position + Offset;
    }

    public bool IsOnCamera(Vector3 position)
    {
        var height = Camera.OrthographicHeight;
        var width = Camera.OrthographicHeight * Screen.Aspect;

        // Implement your logic here
        return false;
    }
}
