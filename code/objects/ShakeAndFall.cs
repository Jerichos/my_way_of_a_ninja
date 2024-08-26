using System;
using System.Transactions;
using Sandbox.level;
using SpriteTools;

namespace Sandbox.objects;

// start shaking when player step on it and then fall, then disable
public class ShakeAndFall : Component, IRespawn
{
	[Property] public SpriteComponent Sprite { get; set; }
	[Property] public Collider Collider { get; set; }
	[Property] public float TimeToFall { get; set; } = 2.0f;
	[Property] public float ShakeIntensity { get; set; } = 0.5f;
	[Property] public float FallSpeed { get; set; } = 96.0f;
	[Property] public float DisableIn { get; set; } = 1.0f; // disable after starts falling
	[Property] public bool DisableColliderOnFall { get; set; }
	[Property] public SoundEvent ShakeSound { get; set; }

	private float _timer;
	private bool _falling;
	private Vector3 _startPosition;

	protected override void OnAwake()
	{
		_startPosition = Transform.Position;
		Collider.OnTriggerEnter += OnTriggerEnter;
		Respawn();
	}

	public void Respawn()
	{
		Log.Info("ShakeAndFall respawn");
		Transform.Position = _startPosition;
		Sprite.Transform.LocalPosition = Vector3.Zero;
		
		_falling = false;
		Collider.Enabled = true;
		GameObject.Enabled = true;
	}

	private void OnTriggerEnter(Collider obj)
	{
		if (obj.GameObject.Components.TryGet(out Player player ))
		{
			Sound.Play(ShakeSound, Transform.Position);
			_timer = 0;
			_falling = true;
			Enabled = true;
		}
	}

	protected override void OnFixedUpdate()
	{
		if (_falling)
		{
			_timer += Time.Delta;
			if (_timer < TimeToFall)
			{
				Sprite.Transform.Position = _startPosition + new Vector3((float)(ShakeIntensity * Math.Sin(_timer * 100)), 0, 0);
			}
			else if(_timer > DisableIn)
			{
				GameObject.Enabled = false;
			}
			else
			{
				Transform.Position += new Vector3(0, -FallSpeed * Time.Delta, 0);
				Collider.Enabled = false;
			}
		}
	}

	protected override void OnDestroy()
	{
		Collider.OnTriggerEnter -= OnTriggerEnter;
	}
}
