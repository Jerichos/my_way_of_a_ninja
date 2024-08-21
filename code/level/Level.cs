using System;
using Sandbox.player;

namespace Sandbox.level;

public class Level : Component
{
	[Property] public GameObject StartCheckpoint { get; set; }
	[Property] public PrefabFile PlayerPrefab { get; set; }
	[Property] public Player Player { get; set; }
	[Property] public CameraFollow CameraFollow { get; set; }
	[Property] public bool MovePlayerToCheckpoint { get; set; } = true;
	[Property] public Weather Weather { get; set; }

	public Vector2 LastCheckpointPosition => Checkpoint.LastCheckpoint?.Transform.Position ?? Transform.Position;
	
	protected override void OnStart()
	{
		LevelStart();
	}
	
	private void LevelStart()
	{
		Log.Info("start level");
		StartCheckpoint.Components.Get<Checkpoint>().Activated = true;
		SpawnPlayer();
	}
	
	private void RespawnPlayer()
	{
		Log.Info("respawn player");
		SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		Player newPlayer;
		Log.Info("spawn player from prefab file");
		if ( Player == null )
		{
			Log.Info("spawning new player because player is null");
			newPlayer = GameObject.Clone(PlayerPrefab).Components.Get<Player>();
			Player = newPlayer;
		}
		else
		{
			Log.Info("spawning player instance from level");
			newPlayer = Player;
		}
		
		Vector3 newPosition;
		if(Checkpoint.LastCheckpoint != null)
		{
			newPosition = Checkpoint.LastCheckpoint.Transform.Position;
			Log.Info("new position from checkpoint");
		}
		else
		{
			newPosition = Transform.Position;
			Log.Info("new position from level");
		}
		
		newPlayer.Teleport(newPosition);
		CameraFollow.SetTarget( newPlayer.GameObject, true );

		newPlayer.OnRespawn();
		newPlayer.DeathEvent -= RespawnPlayer;
		newPlayer.DeathEvent += RespawnPlayer;

		if(Weather != null)
		{
			Weather.AddToPlayer(newPlayer.MotionCore);
			Weather.SetDirection(0);
			Log.Info("added weather to player");
		}
		else
		{
			Log.Info("No weather for this level");
		}
		
		
		Log.Info($"set player position to {newPosition}");
	}
}
