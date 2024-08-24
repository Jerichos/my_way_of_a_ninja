using System;
using Sandbox.objects;

namespace Sandbox.player;


public enum UpgradeType
{
	DOUBLE_JUMP,
	DASH,
	ATTACK,
	PROJECTILE,
}

// quick and dirty solution for skill progression and collecting upgrades
public class PlayerUpgrades : Component
{
	[Property] public UnlockedUpgrades UnlockedUpgrades;
	
	public Action<UnlockedUpgrades> UnlockedUpgradesChangedEvent;

	public void AddUpgrade( Upgrade upgrade )
	{
		switch ( upgrade.Type )
		{
			case UpgradeType.DOUBLE_JUMP:
				UnlockedUpgrades.DoubleJump = true;
				break;
			case UpgradeType.DASH:
				UnlockedUpgrades.Dash = true;
				break;
			case UpgradeType.ATTACK:
				UnlockedUpgrades.Attack = true;
				break;
			case UpgradeType.PROJECTILE:
				UnlockedUpgrades.Projectile = true;
				break;
			default:
				Log.Error($"Unknown upgrade type: {upgrade.Type}");
				break;
		}
		
		UnlockedUpgradesChangedEvent?.Invoke(UnlockedUpgrades);
		
		Log.Info($"Added upgrade: {upgrade.Type}");
	}
}

public struct UnlockedUpgrades
{
	public bool DoubleJump;
	public bool Dash;
	public bool Attack;
	public bool Projectile;

	public bool HasUpgrade( UpgradeType upgrade ) // why I added this? we can remove it and direct access to fields...
	{
		switch ( upgrade )
		{
			case UpgradeType.DOUBLE_JUMP:
				return DoubleJump;
			case UpgradeType.DASH:
				return Dash;
			case UpgradeType.ATTACK:
				return Attack;
			case UpgradeType.PROJECTILE:
				return Projectile;
			default:
				Log.Error($"Unknown upgrade type: {upgrade}");
				return false;
		}
	}
}
