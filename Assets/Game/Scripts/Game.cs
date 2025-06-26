using UnityEngine;
using UnityEngine.Assertions;
using UnityEventsCenter;
using UnityServiceLocator;

namespace mygame
{
	[DefaultExecutionOrder(-1)] //We want the setup to run before any other scripts
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
		Tweaktable _tweaktable;

		float _halfSizeBigAsteroid, _halfSizeMediumAsteroid, _halfSizeSmallAsteroid;

		int _score = 0;
		int _lives = 0;

		int _invalidSpawnFrame = 0;

		void Awake()
		{
			GetComponent<ServiceBehaviour>().Install();

			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Get(out _entitiesManager)
				.Get(out _tweaktable)
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
			//Assuming all transforms are uniformed scaled
			_halfSizeBigAsteroid = _prefabAsteroidBig.transform.localScale.x * .5f;
			_halfSizeMediumAsteroid = _prefabAsteroidMedium.transform.localScale.x * .5f;
			_halfSizeSmallAsteroid = _prefabAsteroidSmall.transform.localScale.x * .5f;
			var halfSizeMissile = _prefabMissile.transform.localScale.x * .5f;
			var halfSizePlayer = _prefabPlayer.transform.localScale.x * .5f;
			var halfSizePlayerSpawn = _prefabPlayerSpawn.transform.localScale.x * .5f;

			_lives = _tweaktable.PlayerLives;
			EventsCenter.Invoke(new GameEvents.LivesChangedEvent(_lives)); //So UI can update with the dynamic value

			_entitiesManager.RegisterEntity(GameEntities.AsteroidBig, _prefabAsteroidBig, _halfSizeBigAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.AsteroidMedium, _prefabAsteroidMedium, _halfSizeMediumAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.AsteroidSmall, _prefabAsteroidSmall, _halfSizeSmallAsteroid);
			_entitiesManager.RegisterEntity(GameEntities.Missile, _prefabMissile, halfSizeMissile);
			_entitiesManager.RegisterEntity(GameEntities.Player, _prefabPlayer, halfSizePlayer, ensureCapacity: 1); //Only want one of these
			_entitiesManager.RegisterEntity(GameEntities.PlayerSpawn, _prefabPlayerSpawn, halfSizePlayerSpawn, ensureCapacity: 1); //Only want one of these

			_entitiesManager.RegisterEntityLifetime(GameEntities.Missile, _tweaktable.MissilesSecondsToLive);

			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidBig, OnBigAsteroid); //Makes two medium
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidMedium, OnMediumAsteroid); //Makes two small
			/*_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnDespawn);*/ //Do not collide
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidMedium, OnMediumAsteroid, GameEntities.AsteroidBig, OnBigAsteroid); //Medium turns to small and Big turns to medium
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnInvertDirection, GameEntities.AsteroidMedium, OnMediumAsteroid); //Small moves in opposite direction and Medium turns to small
			_entitiesManager.RegisterCollisionSolver(GameEntities.AsteroidSmall, OnInvertDirection, GameEntities.AsteroidBig, OnBigAsteroid); //Small moves in opposite direction and Big turns to medium

			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsBigAsteroid, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsMediumAsteroid, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Missile, OnMissileVsSmallAsteroid, GameEntities.AsteroidSmall, OnNoop);

			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.Player, OnPlayerHit, GameEntities.AsteroidSmall, OnNoop);

			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidBig, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidMedium, OnNoop);
			_entitiesManager.RegisterCollisionSolver(GameEntities.PlayerSpawn, OnPlayerSpawnHit, GameEntities.AsteroidSmall, OnNoop);

			_entitiesManager.Spawn(GameEntities.PlayerSpawn, Vector2.zero, Vector2.zero);

			for (int i = 0; i < 3; i++)
				_entitiesManager.Spawn(
					GameEntities.AsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(_halfSizeBigAsteroid), //Assuming its uniform scale
					Random.insideUnitCircle.normalized * Random.Range(_tweaktable.RandomBigAsteroidSpeedBetween.x, _tweaktable.RandomBigAsteroidSpeedBetween.y)
				);

			SpawnPlayer();
		}

		#region PLAYER

		//Uses Awaitable instead of Coroutines which should not create any garbage.
		//In production I would probably use UniTask as it currently has a lot more support.
		async Awaitable TrySpawnPlayerAsync(float timeDelayUntilSpawn)
		{
			if (timeDelayUntilSpawn > 0f)
				await Awaitable.WaitForSecondsAsync(timeDelayUntilSpawn);

			//if anything is inside player spawn, we dont want to spawn
			while (Time.frameCount <= _invalidSpawnFrame)
				await Awaitable.NextFrameAsync();

			SpawnPlayer();
		}

		void SpawnPlayer()
		{
			// Spawn the player at the center of the world bounds
			var entity = _entitiesManager.Spawn(GameEntities.Player, Vector2.zero, Vector2.zero).ToPermanent(); //Use ToPermanent(). Slower access, but a stable index.
			entity.GetGameObject().GetComponent<Player>().Entity = entity;
		}

		void OnPlayerHit(EntityReference player, EntityReference asteroid)
		{
			player.Despawn();

			Debug.Log("We died");

			_lives--;
			EventsCenter.Invoke(new GameEvents.LivesChangedEvent(_lives)); //So UI can update with the dynamic value

			if (_lives > 0)
				_ = TrySpawnPlayerAsync(1f); //_ = to suppress async warning
		}

		#endregion

		#region COLLISION SOLVERS

		void OnPlayerSpawnHit(EntityReference _, EntityReference __)
		{
			_invalidSpawnFrame = Time.frameCount + 1;
		}

		void OnMissileVsBigAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			var pos = missile.GetPosition();
			var posOther = asteroid.GetPosition();

			var dir = (pos - posOther).normalized;
			var right = Vector3.Cross(Vector3.forward, dir);

			var speed = Random.Range(_tweaktable.RandomMediumAsteroidSpeedBetween.x, _tweaktable.RandomMediumAsteroidSpeedBetween.y);

			_entitiesManager.Spawn(GameEntities.AsteroidMedium, posOther + (Vector2)right * _halfSizeMediumAsteroid, right * speed);
			_entitiesManager.Spawn(GameEntities.AsteroidMedium, posOther - (Vector2)right * _halfSizeMediumAsteroid, -right * speed);

			_score += _tweaktable.PointsForBigAsteroid;
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForBigAsteroid, _score));
		}

		void OnMissileVsMediumAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			var pos = missile.GetPosition();
			var posOther = asteroid.GetPosition();

			var dir = (pos - posOther).normalized;
			var right = Vector3.Cross(Vector3.forward, dir);

			var speed = Random.Range(_tweaktable.RandomSmallAsteroidSpeedBetween.x, _tweaktable.RandomSmallAsteroidSpeedBetween.y);

			_entitiesManager.Spawn(GameEntities.AsteroidSmall, posOther + (Vector2)right * _halfSizeSmallAsteroid, right * speed);
			_entitiesManager.Spawn(GameEntities.AsteroidSmall, posOther - (Vector2)right * _halfSizeSmallAsteroid, -right * speed);

			_score += _tweaktable.PointsForMediumAsteroid;
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForMediumAsteroid, _score));
		}

		void OnMissileVsSmallAsteroid(EntityReference missile, EntityReference asteroid)
		{
			missile.Despawn();
			asteroid.Despawn();

			_score += _tweaktable.PointsForSmallAsteroid;
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForSmallAsteroid, _score));
		}

		void OnBigAsteroid(EntityReference collider, EntityReference otherCollider)
		{
			collider.Despawn();

			var speed = Random.Range(_tweaktable.RandomMediumAsteroidSpeedBetween.x, _tweaktable.RandomMediumAsteroidSpeedBetween.y);
			var pos = collider.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			_entitiesManager.Spawn(GameEntities.AsteroidMedium, pos, dir * speed);
		}

		void OnMediumAsteroid(EntityReference collider, EntityReference otherCollider)
		{
			collider.Despawn();

			var speed = Random.Range(_tweaktable.RandomSmallAsteroidSpeedBetween.x, _tweaktable.RandomSmallAsteroidSpeedBetween.y);
			var pos = collider.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			_entitiesManager.Spawn(GameEntities.AsteroidSmall, pos, dir * speed);
		}

		void OnInvertDirection(EntityReference collider, EntityReference otherCollider)
		{
			var speed = collider.GetDirectionAndSpeed().magnitude;
			var pos = collider.GetPosition();
			var dir = (pos - otherCollider.GetPosition()).normalized;
			collider.SetDirectionAndSpeed(dir * speed);
		}

		void OnNoop(EntityReference _, EntityReference __)
		{
		}

		#endregion
	}
}
