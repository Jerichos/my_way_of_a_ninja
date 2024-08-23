using System;

namespace Sandbox.player;

public class CameraFollow : Component
{
    [Property] public GameObject Target { get; set; }

    [Property] public float SmoothTime { get; set; } = 0.3f;
    [Property] public Vector3 Offset { get; set; }

    [Property] CameraComponent Camera { get; set; }

    [Property] public Vector2 MinBounds { get; set; }
    [Property] public Vector2 MaxBounds { get; set; }

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

        smoothPosition.x = smoothPosition.x.Clamp(MinBounds.x + halfWidth, MaxBounds.x - halfWidth);
        smoothPosition.y = smoothPosition.y.Clamp(MinBounds.y + halfHeight, MaxBounds.y - halfHeight);

        Transform.Position = smoothPosition;
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        MinBounds = min;
        MaxBounds = max;
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
}
