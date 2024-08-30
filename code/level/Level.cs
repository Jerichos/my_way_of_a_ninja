using System;
using System.Threading.Tasks;
using Sandbox.enemies;
using Sandbox.player;

namespace Sandbox.level;

public class Level : Component
{
	[Property] public Checkpoint StartCheckpoint { get; set; }
	[Property] public PrefabFile PlayerPrefab { get; set; }
	[Property] public Player Player { get; set; }
	[Property] public CameraFollow CameraFollow { get; set; }
	[Property] public Weather Weather { get; set; }
	[Property] public SoundBoxComponent SoundBox { get; set; }
	[Property] public DeathAnimation DeathAnimation { get; set; }
	[Property] public bool DontTeleportPlayerToCheckpoint { get; set; } = true;
	
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
	private Vector3 _editorPosition;

	// default parameters - the ones that are set in the editor
	private Vector2 _minBounds;
	private Vector2 _maxBounds;
	private SoundEvent _soundEvent;
	private bool _isWeatherEnabled;
	
	// delatyed music start task
	private Task _startSoundTask;

	protected override void OnAwake()
	{
		#if !DEBUG
			DontTeleportPlayerToCheckpoint = false;
		#endif
		
		_minBounds = MinBounds;
		_maxBounds = MaxBounds;
		_soundEvent = SoundBox.SoundEvent;
		_isWeatherEnabled = Weather.Enabled;
		
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
			Log.Warning("!!! Player will be spawned at the level's position set in the editor, until first checkpoint is activated !!!");
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
			newArea.EnterEvent += SetNewArea;
			newArea.ExitEvent += OnAreaExit;
		}
		
		LevelStart();
	}

	private void OnAreaExit( NewArea area )
	{
		if ( _currentArea == area )
		{
			Log.Info("level area exit, it is same as current area");
			SetNewArea(null);
		}
		else
		{
			Log.Info("level area exit, it is not same as current area");

			if ( _currentArea == null )
			{
				Log.Info("current area is null");
			}
		}
	}

	protected override void OnStart()
	{
		if (Player != null && DontTeleportPlayerToCheckpoint)
		{
			Log.Warning("!!! Player will be spawned at the level's position set in the editor, until first checkpoint is activated !!!");
		}
	}

	private void OnCheckpointActivated( Checkpoint checkpoint )
	{
		if ( checkpoint.QuitePlace )
		{
			Log.Info("stop music on reached quite place");
			SoundBox.StopSound(); // TODO: lerp volume down
		}
	}

	private void SetNewArea( NewArea area )
	{
		if(Player.IsDead) // you cannot enter new area when you are dead
			return;
		
		if(area == null)
			return;
		
		if(_currentArea == area) // you cannot enter the same area again, can you?
			return;

		if ( _currentArea == null )
		{
			_currentArea = area;
		}
		else
		{
			if(_currentArea.BossArea)
			{
				return;
			}
		}

		MinBounds = area.MinBounds;
		MaxBounds = area.MaxBounds;
		CameraFollow.SetBounds( MinBounds, MaxBounds, true, area.TransitionMultiplier);
		BoundsChangedEvent?.Invoke(MinBounds, MaxBounds);
			
		if(area.AreaSound != null && !Player.IsDead)
		{
			if ( _currentArea.AreaSound != area.AreaSound)
			{
				Log.Info("stop music before playing new one");
				SoundBox.StopSound();
				Log.Info("play area music: " + area.AreaSound.Sounds[0].ResourceName);
				SoundBox.SoundEvent = area.AreaSound;
				SoundBox.StartSound();
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

	private async void StartSoundAsync(int delay = 1)
	{
		Log.Info("1 start sound async");
		_startSoundTask = Task.Delay(delay);
		await _startSoundTask;
		if(_startSoundTask.IsCompleted)
		{
			Log.Info("2 start sound async");
			SoundBox.StartSound();
		}
	}

	private void LevelStart()
	{
		if(Checkpoint.LastCheckpoint != null)
			StartCheckpoint.Activated = true;
		
		SpawnPlayer();
	}
	
	private void OnPlayerDeath()
	{
		Log.Info("stop music on player death");
		SoundBox.StopSound();
		DeathAnimation.StartFadeIn();
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
			{
				Log.Info("stop music on quite place");
				SoundBox.StopSound(); // TODO: lerp volume down
			}
			else
			{
				SoundBox.StartSound();
			}
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
		
		if(SpawnedBoss != null && SpawnedBoss.GameObject.IsValid)
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
