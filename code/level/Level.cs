using System;
using Sandbox.player;

namespace Sandbox.level;

public class Level : Component
{
	[Property] public Checkpoint StartCheckpoint { get; set; }
	[Property] public PrefabFile PlayerPrefab { get; set; }
	[Property] public Player Player { get; set; }
	[Property] public CameraFollow CameraFollow { get; set; }
	[Property] public bool DontTeleportPlayerToCheckpoint { get; set; } = true;
	[Property] public Weather Weather { get; set; }
	[Property] public SoundBoxComponent SoundBox { get; set; }
	[Property] public DeathAnimation DeathAnimation { get; set; }
	
	[Property] public Vector2 MinBounds { get; set; }
	[Property] public Vector2 MaxBounds { get; set; }

	public Vector2 LastCheckpointPosition => Checkpoint.LastCheckpoint?.Transform.Position ?? Transform.Position;
	
	public event Action RestartEvent;
	
	List<IRespawn> _respawnables = new();
	
	// if DontTeleportPlayerToCheckpointOnce is true, player will be spawned at the level's position set in the editor, until first checkpoint is activated
	private Vector3 _editorPosition;

	protected override void OnAwake()
	{
		if (Player != null && DontTeleportPlayerToCheckpoint)
		{
			Checkpoint.LastCheckpoint = null;
			Log.Info("player is not null, but DontTeleportPlayerToCheckpoint is true");
			_editorPosition = Player.Transform.Position;
		}
	}

	protected override void OnStart()
	{
		_respawnables = Components.GetAll<IRespawn>(FindMode.InDescendants).ToList();
		LevelStart();
	}
	
	private void LevelStart()
	{
		Log.Info("start level");
		if(Checkpoint.LastCheckpoint != null)
			StartCheckpoint.Components.Get<Checkpoint>().Activated = true;
		
		SpawnPlayer();
	}
	
	private void OnPlayerDeath()
	{
		Log.Info("respawn player");
		SoundBox.StopSound();
		DeathAnimation.StartFadeIn();
		// RestartEvent?.Invoke();
		// SpawnPlayer();
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
		
		// set spawn position
		Vector3 spawnPosition;
		if(Checkpoint.LastCheckpoint != null)
		{
			spawnPosition = Checkpoint.LastCheckpoint.Transform.Position;
			Log.Info("spawn position from checkpoint");
		}
		else if(DontTeleportPlayerToCheckpoint && Player != null)
		{
			spawnPosition = _editorPosition;
			Log.Info("spawn position from editor");
		}
		else
		{
			spawnPosition = Transform.Position;
			Log.Error("FIXME: No spawn set. ");
		}
		
		newPlayer.Teleport(spawnPosition);
		
		CameraFollow.SetTarget( newPlayer.GameObject, true );
		CameraFollow.SetBounds( MinBounds, MaxBounds );

		newPlayer.OnRespawn();
		newPlayer.DeathEvent -= OnPlayerDeath;
		newPlayer.DeathEvent += OnPlayerDeath;
		
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
		
		SoundBox.StartSound();
		DeathAnimation.StartFadeOut();
		
		DeathAnimation.AnimationFadeFinishedEvent -= OnDeathFadeFinished;
		DeathAnimation.AnimationFadeFinishedEvent += OnDeathFadeFinished;
		Log.Info($"set player position to {spawnPosition}");
	}
	
	private void RespawnAll()
	{
		foreach (var respawnable in _respawnables)
		{
			respawnable.Respawn();
		}
	}

	private void OnDeathFadeFinished( bool fadeIn )
	{
		if(fadeIn)
		{
			RestartEvent?.Invoke();
			RespawnAll();
			SpawnPlayer();
		}
		else
		{
			Player.Enabled = true;
		}
	}

	protected override void OnFixedUpdate()
	{
		// kill player if out of bounds
		if(Player != null)
		{
			float offset = 128;
			if(Player.Transform.Position.x < MinBounds.x - offset || Player.Transform.Position.x > MaxBounds.x + offset || Player.Transform.Position.y < MinBounds.y - offset || Player.Transform.Position.y > MaxBounds.y + offset)
				Player.Kill();
		}
	}
	
	protected override void DrawGizmos()
	{
		// Define the corner points relative to the object's position
		Vector3 botLeft = new Vector3(MinBounds.x, MinBounds.y, 0) - Transform.Position;
		Vector3 botRight = new Vector3(MaxBounds.x, MinBounds.y, 0) - Transform.Position;
		Vector3 topLeft = new Vector3(MinBounds.x, MaxBounds.y, 0) - Transform.Position;
		Vector3 topRight = new Vector3(MaxBounds.x, MaxBounds.y, 0) - Transform.Position;

		// Apply the rotation to each corner point
		botLeft = Transform.LocalRotation * botLeft;
		botRight = Transform.LocalRotation * botRight;
		topLeft = Transform.LocalRotation * topLeft;
		topRight = Transform.LocalRotation * topRight;
		
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineThickness = 2;
		
		// Draw the lines in world space
		Gizmo.Draw.Line(botLeft, botRight);
		Gizmo.Draw.Line(botRight, topRight);
		Gizmo.Draw.Line(topRight, topLeft);
		Gizmo.Draw.Line(topLeft, botLeft);
	}
}
