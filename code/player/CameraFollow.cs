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
    private  float _transitionSpeedMultiplier = 2f;
private bool _moveToBoundsAndStop = false;

    protected override void OnFixedUpdate()
    {
        if (_moveToBoundsAndStop)
        {
            // Move camera to center within new bounds
            var centerBounds = new Vector3(
                (_targetMinBounds.x + _targetMaxBounds.x) / 2,
                (_targetMinBounds.y + _targetMaxBounds.y) / 2,
                Transform.Position.z); // Maintain current Z position

            // Move the camera smoothly to the center of the new bounds
            Transform.Position = Vector3.Lerp(Transform.Position, centerBounds, Time.Delta * _transitionSpeedMultiplier);

            // Check if the camera is close enough to the target position
            if ((Transform.Position - centerBounds).LengthSquared < 0.001f)
            {
                Transform.Position = centerBounds;
                _moveToBoundsAndStop = false;
                Target = null; // Stop following the player
            }

            return; // Skip the rest of the update if moving to bounds
        }

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
            MinBounds = Vector2.Lerp(MinBounds, _targetMinBounds, Time.Delta * _transitionSpeedMultiplier / SmoothTime);
            MaxBounds = Vector2.Lerp(MaxBounds, _targetMaxBounds, Time.Delta * _transitionSpeedMultiplier / SmoothTime);

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

    public void SetBounds(Vector2 min, Vector2 max, bool smoothly = false, float smoothMultiplier = 2f)
    {
	    _moveToBoundsAndStop = false;
        if (smoothly)
        {
            _targetMinBounds = min;
            _targetMaxBounds = max;
            _smoothTransition = true;
            _transitionSpeedMultiplier = smoothMultiplier;
        }
        else
        {
            MinBounds = min;
            MaxBounds = max;
            _smoothTransition = false;
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

        return false;
    }

    public void MoveToBoundsDontFollowAnymore(Vector2 minBounds, Vector2 maxBounds, float speedMultiplier)
    {
        _targetMinBounds = minBounds;
        _targetMaxBounds = maxBounds;
        _moveToBoundsAndStop = true;
        _transitionSpeedMultiplier = speedMultiplier; // You can adjust this speed as needed

        // Optionally, you could reset bounds immediately to avoid weird clamping during the move
        MinBounds = minBounds;
        MaxBounds = maxBounds;
    }
}
