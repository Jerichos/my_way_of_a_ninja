using System;
using Sandbox.enemies;
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

	public Action RestartEvent;
	public Action<Vector2, Vector2> BoundsChangedEvent;
	private IEnumerable<Checkpoint> _checkpoints;
	private IEnumerable<NewArea> _newAreas;
	private List<IRespawn> _respawnables = new();

	private BigBossBird _levelBoss;
	public BigBossBird SpawnedBoss { get; private set; }
	public Action<BigBossBird> BossSpawnedEvent; // null if reset

	private NewArea _currentArea;
	// if DontTeleportPlayerToCheckpointOnce is true, player will be spawned at the level's position set in the editor, until first checkpoint is activated
	private Vector3 _editorPosition;

	protected override void OnAwake()
	{
		_levelBoss = Components.Get<BigBossBird>(FindMode.InDescendants);
		_levelBoss.GameObject.Enabled = false;

		if ( _levelBoss == null )
		{
			Log.Error("Level boss not found");
		}
		
		_checkpoints = Components.GetAll<Checkpoint>( FindMode.InDescendants );

		Checkpoint.CheckpointActivatedEvent += OnCheckpointActivated;
		Checkpoint.LastCheckpoint = StartCheckpoint;
		if (Player != null && DontTeleportPlayerToCheckpoint)
		{
			Checkpoint.LastCheckpoint = null;
			_editorPosition = Player.Transform.Position;
		}
		_respawnables = Components.GetAll<IRespawn>(FindMode.InDescendants).ToList();
		_newAreas = Components.GetAll<NewArea>(FindMode.InDescendants);

		for ( int i = _respawnables.Count - 1; i < 0; i-- )
		{
			if(_respawnables[i].IgnoreRespawn)
				_respawnables.RemoveAt(i);
		}

		foreach (var newArea in _newAreas)
		{
			newArea.OnEnter += OnNewAreaEnter;
		}
		
		LevelStart();
	}

	private void OnCheckpointActivated( Checkpoint checkpoint )
	{
		if ( checkpoint.QuitePlace )
		{
			SoundBox.StopSound(); // TODO: lerp volume down
		}
	}

	private void OnNewAreaEnter( NewArea area )
	{
		if(_currentArea == area)
			return;

		if ( _currentArea != null )
		{
			_currentArea.Enabled = false;
		}
		else
		{
			_currentArea = area;
		}
		
		MinBounds = area.MinBounds;
		MaxBounds = area.MaxBounds;
		CameraFollow.SetBounds( MinBounds, MaxBounds, true, area.TransitionMultiplier);
		BoundsChangedEvent?.Invoke(MinBounds, MaxBounds);
		
		if(area.AreaSound != null && !Player.IsInGracePeriod)
		{
			if ( _currentArea.AreaSound != area.AreaSound )
			{
				SoundBox.StopSound();
				StartSoundAsync();
				SoundBox.SoundEvent = area.AreaSound;
			}
			else
			{
				Log.Info("dont start sound, already playing");
			}
			
		}
		
		Weather.Enabled = area.WeatherEnabled;
		Weather.RestartWeather();
		
		if ( area.BossArea )
		{
			SpawnedBoss = _levelBoss.GameObject.Clone().Components.Get<BigBossBird>();
			SpawnedBoss.Init(this);
			SpawnedBoss.Transform.Position = _levelBoss.Transform.Position;
			SpawnedBoss.GameObject.Enabled = true;
			SpawnedBoss.Player = Player;
			BossSpawnedEvent?.Invoke(SpawnedBoss);
			CameraFollow.MoveToBoundsDontFollowAnymore( MinBounds, MaxBounds, area.TransitionMultiplier );
		}
		
		_currentArea = area;
	}
	// async method to to StartSound()

	public void StartWeather()
	{
		Weather.Enabled = true;
		Weather.RestartWeather();
	}
	
	public void StopWeather()
	{
		Weather.Enabled = false;
		Weather.RestartWeather();
	}

	private async void StartSoundAsync()
	{
		// wait 1 second
		await Task.Delay(3000);
		SoundBox.StartSound();
	}

	private void LevelStart()
	{
		if(Checkpoint.LastCheckpoint != null)
			StartCheckpoint.Activated = true;
		
		SpawnPlayer();
	}
	
	private void OnPlayerDeath()
	{
		SoundBox.StopSound();
		DeathAnimation.StartFadeIn();
		// RestartEvent?.Invoke();
		// SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		_currentArea = null;
		foreach ( var area in _newAreas )
		{
			area.Enabled = true;
		}
		
		Player newPlayer;
		if ( Player == null )
		{
			newPlayer = GameObject.Clone(PlayerPrefab).Components.Get<Player>();
			Player = newPlayer;
		}
		else
		{
			newPlayer = Player;
		}
		
		// set spawn position
		Vector3 spawnPosition;
		if(Checkpoint.LastCheckpoint != null)
		{
			spawnPosition = Checkpoint.LastCheckpoint.Transform.Position;
			if ( Checkpoint.LastCheckpoint.QuitePlace )
				SoundBox.StopSound(); // TODO: lerp volume down
			else
				SoundBox.StartSound();
			
			Log.Info("is last checkpoint quite place: " + Checkpoint.LastCheckpoint.QuitePlace);
		}
		else if(DontTeleportPlayerToCheckpoint && Player != null)
		{
			spawnPosition = _editorPosition;
		}
		else
		{
			spawnPosition = Transform.Position;
		}
		
		newPlayer.Teleport(spawnPosition);
		
		CameraFollow.SetTarget( newPlayer.GameObject, true );
		CameraFollow.SetBounds( MinBounds, MaxBounds );

		newPlayer.OnRespawn();
		newPlayer.DeathEvent -= OnPlayerDeath;
		newPlayer.DeathEvent += OnPlayerDeath;
		newPlayer.Inventory.AddedItemEvent -= OnAddedItem;
		newPlayer.Inventory.AddedItemEvent += OnAddedItem;
		newPlayer.Inventory.SavedItemsEvent -= OnSavedProgress;
		newPlayer.Inventory.SavedItemsEvent += OnSavedProgress;
		
		if(Weather != null)
		{
			Weather.OnComponentEnabled -= OnWeatherEnabled; // TODO: move to on disable, also other events. Needed for multiple levels
			Weather.OnComponentEnabled += OnWeatherEnabled;
			Weather.RestartWeather();
		}
		
		DeathAnimation.StartFadeOut();
		
		DeathAnimation.AnimationFadeFinishedEvent -= OnDeathFadeFinished;
		DeathAnimation.AnimationFadeFinishedEvent += OnDeathFadeFinished;
		
		BossSpawnedEvent?.Invoke(null);
		
		if(SpawnedBoss != null)
		{
			SpawnedBoss.GameObject.Destroy();
		}
		
		foreach (var checkpoint in _checkpoints)
			checkpoint.PendingItem(false);
	}

	private void OnWeatherEnabled()
	{
		if ( Weather.Enabled )
		{
			Weather.AddToPlayer(Player.MotionCore);
			Weather.SetDirection(0);
		}
		else
		{
			Weather.RemoveFromPlayer(Player.MotionCore);
		}
	}

	private void OnSavedProgress()
	{
		foreach (var checkpoint in _checkpoints)
			checkpoint.PendingItem(false);
	}

	private void OnAddedItem( Inventory obj )
	{
		foreach (var checkpoint in _checkpoints)
			checkpoint.PendingItem(true);
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
			float offset = 32;
			float verticalOffset = 64;
			if ( Player.Transform.Position.x < MinBounds.x - offset ||
			     Player.Transform.Position.x > MaxBounds.x + offset ||
			     Player.Transform.Position.y < MinBounds.y - verticalOffset ||
			     Player.Transform.Position.y > MaxBounds.y + verticalOffset )
			{
				Player.Kill();
			}
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
