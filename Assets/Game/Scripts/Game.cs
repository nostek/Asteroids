using UnityEngine;
using UnityEngine.Assertions;
using UnityEventsCenter;
using UnityServiceLocator;

namespace mygame
{
	[DefaultExecutionOrder(-1)]
	public class Game : MonoBehaviour
	{
		[Header("Prefabs Asteroids")]
		[SerializeField] GameObject _prefabAsteroidBig;
		[SerializeField] GameObject _prefabAsteroidMedium;
		[SerializeField] GameObject _prefabAsteroidSmall;

		[Header("Prefabs Player")]
		[SerializeField] GameObject _prefabPlayer;
		[SerializeField] GameObject _prefabPlayerSpawn;
		[SerializeField] GameObject _prefabMissile;

		WorldBoundsManager _worldBoundsManager;
		EntitiesManager _entitiesManager;
		WindowsManager _windowsManager;

		Tweaktable _tweaktable;
		PlayerTweaktable _playerTweaktable;
		WindowsDatabase _windowsDatabase;
		GameSoundsDatabase _soundsDatabase;

		float _halfSizeBigAsteroid, _halfSizeMediumAsteroid, _halfSizeSmallAsteroid;
		float _halfSizePlayer;

		float _nextSpawnTime = 0f;

		readonly LivesCounter _lives = new();
		readonly ScoreCounter _score = new();

		int _invalidSpawnFrame = 0;

		EntityReference _player;

		void Awake()
		{
			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Get(out _entitiesManager)
				.Get(out _windowsManager)
				.Get(out _tweaktable)
				.Get(out _playerTweaktable)
				.Get(out _windowsDatabase)
				.Get(out _soundsDatabase)
				.Done();

			Assert.IsNotNull(_prefabAsteroidBig, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidMedium, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidSmall, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabPlayer, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabPlayerSpawn, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabMissile, "Prefab object is not assigned. Please assign a prefab in the inspector.");
		}

		void Start()
		{
			_lives.SetLives(_playerTweaktable.PlayerLives);

			//Assuming all transforms are uniformed scaled
			_halfSizeBigAsteroid = _prefabAsteroidBig.transform.localScale.x * .5f;
			_halfSizeMediumAsteroid = _prefabAsteroidMedium.transform.localScale.x * .5f;
			_halfSizeSmallAsteroid = _prefabAsteroidSmall.transform.localScale.x * .5f;
			_halfSizePlayer = _prefabPlayer.transform.localScale.x * .5f;
			var halfSizePlayerSpawn = _prefabPlayerSpawn.transform.localScale.x * .5f;
			var halfSizeMissile = _prefabMissile.transform.localScale.x * .5f;

			_entitiesManager.RegisterEntity(GameEntities.AsteroidBig, _prefabAsteroidBig, _halfSizeBigAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.AsteroidMedium, _prefabAsteroidMedium, _halfSizeMediumAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.AsteroidSmall, _prefabAsteroidSmall, _halfSizeSmallAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.Missile, _prefabMissile, halfSizeMissile);
			_entitiesManager.RegisterEntity(GameEntities.Player, _prefabPlayer, _halfSizePlayer, ensureCapacity: 1); //Only want one of these
			_entitiesManager.RegisterEntity(GameEntities.PlayerSpawn, _prefabPlayerSpawn, halfSizePlayerSpawn, ensureCapacity: 1); //Only want one of these

			//Missiles have a lifetime
			_entitiesManager.RegisterEntityLifetime(GameEntities.Missile, _tweaktable.MissilesSecondsToLive);

			//Asteroids VS Asteroids
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidBig, OnBigAsteroid); //Makes two medium asteroids
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidMedium, OnMediumAsteroid); //Makes two small asteroids
			/*_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnDespawn);*/ //Do not collide small VS small
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidMedium, OnMediumAsteroid, GameEntities.AsteroidBig, OnBigAsteroid); //Medium turns to small and Big turns to medium
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnInvertDirection, GameEntities.AsteroidMedium, OnMediumAsteroid); //Small moves in an opposite direction and Medium turns to small
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnInvertDirection, GameEntities.AsteroidBig, OnBigAsteroid); //Small moves in an opposite direction and Big turns to medium

			//Missiles VS Asteroids
			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsBigAsteroid, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsMediumAsteroid, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsSmallAsteroid, GameEntities.AsteroidSmall, OnNoop);

			//Player VS Asteroids
			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidSmall, OnNoop);

			//We use the PlayerSpawn entity for detecting Asteroids in the spawn area.
			//It runs on the job+burst system for maximum performance.
			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidSmall, OnNoop);
			_entitiesManager.Spawn(GameEntities.PlayerSpawn, Vector2.zero, Vector2.zero);

			//Spawn an initial number of big asteroids on start
			for (int i = 0; i < _tweaktable.AsteroidsInitialSpawnCount; i++)
				_entitiesManager.Spawn(
					GameEntities.AsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(_halfSizeBigAsteroid),
					Random.insideUnitCircle.normalized * _tweaktable.RandomBigAsteroidSpeedBetween
				);

			_nextSpawnTime = _tweaktable.GetNextAsteroidSpawnDelayOverTime();

			//Wait a second before we spawn the Player, so the user has time to get ready
			TrySpawnPlayerAsync(1f).SafeExecute();
		}

		void Update()
		{
			//Run update for entities first
			_entitiesManager.RunUpdate(_worldBoundsManager.Bounds);

			UpdateNextSpawnTime();
		}

		void UpdateNextSpawnTime()
		{
			if (!(Time.time > _nextSpawnTime) || !_player.IsValid())
				return;

			_nextSpawnTime = _tweaktable.GetNextAsteroidSpawnDelayOverTime();

			_entitiesManager.Spawn(
				GameEntities.AsteroidBig,
				_worldBoundsManager.GetRandomInsideBounds(_halfSizeBigAsteroid, _player.GetPosition(), _halfSizePlayer * 2f), // *2f, so we have a little more room to move
				Random.insideUnitCircle.normalized * _tweaktable.RandomBigAsteroidSpeedBetween
			);
		}

		#region PLAYER

		//Uses Awaitable instead of Coroutines which does not create any garbage.
		//In production, I would probably use UniTask as it currently has a lot more support.
		async Awaitable TrySpawnPlayerAsync(float timeDelayUntilSpawn)
		{
			EventsCenter.Invoke(new GameEvents.Player.WaitingForSpawn(true));

			if (timeDelayUntilSpawn > 0f)
			{
				await Awaitable.WaitForSecondsAsync(timeDelayUntilSpawn);
				if (this == null)
					return;
			}

			//if anything is inside player spawn, we don't want to spawn this frame
			while (Time.frameCount <= _invalidSpawnFrame)
				await Awaitable.NextFrameAsync();

			if (this == null)
				return;

			EventsCenter.Invoke(new GameEvents.Player.WaitingForSpawn(false));

			SpawnPlayer();
		}

		void SpawnPlayer()
		{
			// Spawn the player at the center of the world bounds
			_player = _entitiesManager.SpawnAsPermanent(GameEntities.Player, Vector2.zero, Vector2.zero);
			_player.GetGameObject().GetComponent<Player>().Entity = _player;
		}

		void OnPlayerHit(EntityReference player, EntityReference asteroid)
		{
			player.Despawn();
			_player = default;

			Log.D("We died");

			_soundsDatabase.PlayExplosionBig();

			bool isGameOver = _lives.TakeLife();
			if (isGameOver)
			{
				OnGameOver();
				return;
			}

			TrySpawnPlayerAsync(1f).SafeExecute();
		}

		void OnGameOver()
		{
			//I would use something else when saving important data. But trivial data like this PlayerPrefs is fine (except for WebGL+itch.io)
			PlayerPrefs.SetInt(GameConstants.PlayerPrefsLastScore, _score.Score);

			_windowsManager.OpenWindow(_windowsDatabase.WindowGameOver);
		}

		#endregion

		#region COLLISION SOLVERS

		void OnPlayerSpawnHit(EntityReference _, EntityReference __)
		{
			_invalidSpawnFrame = Time.frameCount + 1; //We have an Asteroid inside the spawn area this frame, invalidate this and next frame
		}

		void OnMissileVsBigAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			_soundsDatabase.PlayExplosionBig();
			SplitAsteroid(asteroid, missile, GameEntities.AsteroidMedium, _halfSizeMediumAsteroid, _tweaktable.RandomMediumAsteroidSpeedBetween);

			_score.AddPoints(_tweaktable.PointsForBigAsteroid);
		}

		void OnMissileVsMediumAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			_soundsDatabase.PlayExplosionMedium();
			SplitAsteroid(asteroid, missile, GameEntities.AsteroidSmall, _halfSizeSmallAsteroid, _tweaktable.RandomSmallAsteroidSpeedBetween);

			_score.AddPoints(_tweaktable.PointsForMediumAsteroid);
		}

		void OnMissileVsSmallAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			_soundsDatabase.PlayExplosionSmall();

			_score.AddPoints(_tweaktable.PointsForSmallAsteroid);
		}

		void OnBigAsteroid(EntityReference asteroid, EntityReference otherCollider)
		{
			asteroid.Despawn();

			_soundsDatabase.PlayExplosionBig();
			SwapAsteroidTo(asteroid, otherCollider, GameEntities.AsteroidMedium, _tweaktable.RandomMediumAsteroidSpeedBetween);
		}

		void OnMediumAsteroid(EntityReference asteroid, EntityReference otherCollider)
		{
			asteroid.Despawn();

			_soundsDatabase.PlayExplosionMedium();
			SwapAsteroidTo(asteroid, otherCollider, GameEntities.AsteroidSmall, _tweaktable.RandomSmallAsteroidSpeedBetween);
		}

		void OnInvertDirection(EntityReference asteroid, EntityReference otherCollider)
		{
			var speed = asteroid.GetDirectionAndSpeed().magnitude;
			var pos = asteroid.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			asteroid.SetDirectionAndSpeed(dir * speed);
		}

		void OnNoop(EntityReference _, EntityReference __)
		{
		}

		#endregion

		#region ASTEROID ACTIONS

		void SplitAsteroid(EntityReference asteroid, EntityReference other, int entityKey, float halfSize, float randomSpeed)
		{
			var pos = asteroid.GetPosition();
			var posOther = other.GetPosition();

			Vector2 dir = (posOther - pos).normalized;
			Vector2 right = Vector3.Cross(Vector3.forward, dir);

			_entitiesManager.Spawn(entityKey, pos + right * halfSize, right * randomSpeed);
			_entitiesManager.Spawn(entityKey, pos - right * halfSize, -right * randomSpeed);
		}

		void SwapAsteroidTo(EntityReference asteroid, EntityReference other, int entityKey, float randomSpeed)
		{
			var pos = asteroid.GetPosition();
			var dir = (pos - other.GetPosition()).normalized;
			_entitiesManager.Spawn(entityKey, pos, dir * randomSpeed);
		}

		#endregion
	}
}
