using System;
using Sandbox.objects;

namespace Sandbox.player;


public enum ItemType
{
	DOUBLE_JUMP,
	DASH,
	ATTACK,
	PROJECTILE,
	MAX_HEALTH,
	HEALTH,
}

// quick and dirty solution for collectibles
public class Inventory : Component
{
	private Items _items; // this remains on death and restart
	private Items _pendingItems; // this resets on restart, and on save collected items are moved to Items. So the player must reach checkpoint to save the progress

	private readonly List<Collectible> _pendingCollectible = new();
	
	public Action<Inventory> AddedItemEvent;
	public Action SavedItemsEvent;

	public void AddUpgrade( Collectible collectible )
	{
		switch ( collectible.Type )
		{
			case ItemType.DOUBLE_JUMP:
				_pendingItems.DoubleJump = true;
				break;
			case ItemType.DASH:
				_pendingItems.Dash = true;
				break;
			case ItemType.ATTACK:
				_pendingItems.Sword = true;
				break;
			case ItemType.PROJECTILE:
				_pendingItems.Projectile = true;
				break;
			case ItemType.MAX_HEALTH:
				_pendingItems.MaxHealth++;
				break;
			case ItemType.HEALTH:
				Log.Error("Health upgrade is not implemented yet");
				break;
			default:
				Log.Error($"Unknown upgrade type: {collectible.Type}");
				break;
		}
		
		_pendingCollectible.Add(collectible);
		AddedItemEvent?.Invoke(this);
		
		Log.Info($"Added upgrade: {collectible.Type}");
	}
	
	public void ResetPendingItems()
	{
		_pendingItems = _items;
		_pendingCollectible.Clear();
	}

	public void SaveProgress()
	{
		// if we have true for an item in pending, move it to Items
		if(_pendingItems.DoubleJump)
			_items.DoubleJump = true;
		if(_pendingItems.Dash)
			_items.Dash = true;
		if(_pendingItems.Sword)
			_items.Sword = true;
		if(_pendingItems.Projectile)
			_items.Projectile = true;
		if(_pendingItems.MaxHealth > 0)
			_items.MaxHealth = _pendingItems.MaxHealth;

		for ( int i = 0; i < _pendingCollectible.Count; i++ )
			_pendingCollectible[i].Saved = true;
		
		SavedItemsEvent?.Invoke();
	}

	public bool HasUpgrade( ItemType itemType, out int value)
	{
		value = 0;
		switch ( itemType )
		{
			case ItemType.DOUBLE_JUMP:
				Log.Info($"_items.DoubleJump: {_items.DoubleJump} _pendingItems.DoubleJump: {_pendingItems.DoubleJump}");
				return _items.DoubleJump || _pendingItems.DoubleJump;
			case ItemType.DASH:
				return _items.Dash || _pendingItems.Dash;
			case ItemType.ATTACK:
				return _items.Sword || _pendingItems.Sword;
			case ItemType.PROJECTILE:
				return _items.Projectile || _pendingItems.Projectile;
			case ItemType.MAX_HEALTH:
				value = _items.MaxHealth;
				return _items.MaxHealth > 0;
			case ItemType.HEALTH:
				Log.Error("Health upgrade is not implemented yet");
				return false;
			default:
				Log.Error($"Unknown upgrade type: {itemType}");
				return false;
		}
	}
}

public struct Items
{
	public bool DoubleJump;
	public bool Dash;
	public bool Sword;
	public bool Projectile;
	public int MaxHealth;
}
