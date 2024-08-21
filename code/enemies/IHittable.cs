using System;

namespace Sandbox.enemies;

public interface IHittable
{
	void Hit(int damage, Action<SoundEvent> soundCallback);
}
