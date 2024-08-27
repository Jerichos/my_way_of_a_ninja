using System;
using SpriteTools;

namespace Sandbox.enemies;

public enum BirdBossPositions
{
	RightBot,
	RightMid,
	RightTop,
	LeftBot,
	LeftMid,
	LeftTop,
}

public enum Side
{
	Right,
	Left
}

public enum BirdBossPhase
{
	Intro,
	Phase1, // fly on platform
	Phase2,
	Phase3
}

public class BigBossBird : Component
{
	[Property] private SpriteComponent Sprite { get; set; }
	[Property] private float Speed { get; set; } = 360;
	[Property] private SoundEvent EagleSound { get; set; }
	[Property] public Enemy BirdEnemy { get; set; }
	[Property] private MoveToPosition MoveToPosition {get; set;}
	
	public Player Player { get; set; }
	private Vector3 PlayerPosition => Player.Transform.Position + new Vector3(0, 32, 0);
	
	private const string IDLE_ANIM = "idle";
	private const string FLY_ANIM = "fly"; // when flying on player
	
	[Property] public GameObject RightBotPosition { get; private set; }
	[Property] public GameObject RightMidPosition { get; private set; }
	[Property] public GameObject RightTopPosition { get; private set; }
	[Property] public GameObject LeftBotPosition { get; private set; }
	[Property] public GameObject LeftMidPosition { get; private set; }
	[Property] public GameObject LeftTopPosition { get; private set; }
	
	BirdBossPositions _currentPosition;
	BirdBossPhase _currentPhase;
	
	Side _currentSide => _currentPosition switch
	{
		BirdBossPositions.RightBot or BirdBossPositions.RightMid or BirdBossPositions.RightTop => Side.Right,
		BirdBossPositions.LeftBot or BirdBossPositions.LeftMid or BirdBossPositions.LeftTop => Side.Left,
		_ => throw new ArgumentOutOfRangeException()
	};
	
	bool _flyingAtPlayer;
	private float _timer;
	private float _delay = 1.0f;
	
	private float _returnToPossitionSpeed = 100;
	private float _flyAtPlayerSpeed = 300;
	private float _targetPlayerSpeed = 200;
	
	private bool IsAtHeightWithPlayer => Math.Abs(BirdEnemy.Transform.Position.y - PlayerPosition.y) < 10;
	
	protected override void OnStart()
	{
		Sound.Play(EagleSound);
		MoveToPosition.OnArrived += OnArrived;
		BirdEnemy.HitEvent += OnHit;
		BirdEnemy.DeadEvent += OnDead;
		SetPhase(BirdBossPhase.Intro, true);
	}

	private void OnDead()
	{
		Enabled = false;
	}

	private void ResetTimer()
	{
		_timer = 0;
	}

	protected override void OnFixedUpdate()
	{
		_timer += Time.Delta;
		
		if ( _currentPhase == BirdBossPhase.Phase1 && _timer > _delay )
		{
			// Log.Info($"bird IsAtHeightWithPlayer: {IsAtHeightWithPlayer} IsFlyingAtPlayer: {_flyingAtPlayer}");
			if ( IsAtHeightWithPlayer && !_flyingAtPlayer )
			{
				_flyingAtPlayer = true;
				// Log.Info($" bird started flying at player");
				Sprite.PlayAnimation(FLY_ANIM);
				Speed = _flyAtPlayerSpeed;
				
				// move bird to opposite side of positions at the same height
				if ( _currentPosition is BirdBossPositions.RightBot or BirdBossPositions.RightMid or BirdBossPositions.RightTop )
				{
					Vector3 position = LeftBotPosition.Transform.Position;
					position.y = BirdEnemy.Transform.Position.y;
					MoveToPosition.MoveTo(position, Speed);
					_currentPosition = BirdBossPositions.LeftBot;
				}
				else
				{
					Vector3 position = RightBotPosition.Transform.Position;
					position.y = BirdEnemy.Transform.Position.y;
					MoveToPosition.MoveTo(position, Speed);
					_currentPosition = BirdBossPositions.RightBot;
				}
			}
			else if(!_flyingAtPlayer)
			{
				// move bird to player height position
				Speed = _targetPlayerSpeed;
				
				Vector3 position = BirdEnemy.Transform.Position;
				position.y = PlayerPosition.y;
				MoveToPosition.MoveTo(position, Speed);
			}
		}
	}

	private void SetPhase( BirdBossPhase phase, bool force = false)
	{
		if(_currentPhase == phase && !force)
			return;
		
		switch ( phase )
		{
			case BirdBossPhase.Intro:
				Speed = _flyAtPlayerSpeed;
				Sprite.PlayAnimation(FLY_ANIM);
				SetPosition(BirdBossPositions.RightBot , true);
				Sprite.SpriteFlags = SpriteFlags.None;
				break;
			case BirdBossPhase.Phase1:
				break;
			case BirdBossPhase.Phase2:
				break;
			case BirdBossPhase.Phase3:
				break;
		}
		
		// Log.Info("bird phase changed: " + phase);
		_currentPhase = phase;
	}

	private void OnHit( int obj )
	{
		if(_currentPhase is BirdBossPhase.Phase1)
		{
			SetPhase(BirdBossPhase.Phase1);
		}
	}

	private void OnArrived()
	{
		ResetTimer();
		Sprite.PlayAnimation(IDLE_ANIM);
		
		if(_currentPhase == BirdBossPhase.Intro)
		{
			SetPhase(BirdBossPhase.Phase1);
		}

		if ( _currentPhase == BirdBossPhase.Phase1 )
		{
		}
		
		// Log.Info($"bird arrived {_currentPhase} at position: {_currentPosition}");

		if ( _flyingAtPlayer )
		{
			if ( _currentPosition is BirdBossPositions.LeftMid or BirdBossPositions.RightMid )
			{
				// Log.Info("bird stopped flying at player");
				_flyingAtPlayer = false;
			}
			
			if(_currentSide == Side.Right)
			{
				Speed = _returnToPossitionSpeed;
				SetPosition(BirdBossPositions.RightMid, true);
			}
			else
			{
				Speed = _returnToPossitionSpeed;
				SetPosition(BirdBossPositions.LeftMid, true);
			}
		}
		
		UpdateFacing();
	}

	private void UpdateFacing()
	{
		int facing = 1;
		if (_currentPosition == BirdBossPositions.RightBot || _currentPosition == BirdBossPositions.RightMid || _currentPosition == BirdBossPositions.RightTop)
		{
			facing = -1;
		}
		Sprite.SpriteFlags = facing == 1 ? SpriteFlags.None : SpriteFlags.HorizontalFlip;
	}

	private void SetPosition( BirdBossPositions birdPosition, bool force = false)
	{
		if(_currentPosition == birdPosition && !force)
			return;
		
		UpdateFacing();
		
		switch ( birdPosition )
		{
			case BirdBossPositions.RightBot:
				MoveToPosition.MoveTo(RightBotPosition.Transform.Position, Speed);
				break;
			case BirdBossPositions.RightMid:
				MoveToPosition.MoveTo(RightMidPosition.Transform.Position, Speed);
				break;
			case BirdBossPositions.RightTop:
				MoveToPosition.MoveTo(RightTopPosition.Transform.Position, Speed);
				break;
			case BirdBossPositions.LeftBot:
				MoveToPosition.MoveTo(LeftBotPosition.Transform.Position, Speed);
				break;
			case BirdBossPositions.LeftMid:
				MoveToPosition.MoveTo(LeftMidPosition.Transform.Position, Speed);
				break;
			case BirdBossPositions.LeftTop:
				MoveToPosition.MoveTo(LeftTopPosition.Transform.Position, Speed);
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof(birdPosition), birdPosition, null );
		}
		
		// Log.Info($"bird changed position from {_currentPosition} to {birdPosition}");
		_currentPosition = birdPosition;
	}
}
