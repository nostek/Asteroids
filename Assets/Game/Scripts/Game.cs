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
		[SerializeField] GameObject _prefabMissile;

		WorldBoundsManager _worldBoundsManager;
		EntitiesManager _entitiesManager;
		Tweaktable _tweaktable;

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
			Assert.IsNotNull(_prefabMissile, "Prefab object is not assigned. Please assign a prefab in the inspector.");
		}

		void Start()
		{
			_entitiesManager.RegisterEntity(_prefabAsteroidBig);
			_entitiesManager.RegisterEntity(_prefabAsteroidMedium);
			_entitiesManager.RegisterEntity(_prefabAsteroidSmall);
			_entitiesManager.RegisterEntity(_prefabMissile);

			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid, _prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop, _prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop, _prefabAsteroidBig, OnBigAsteroid);

			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileBigAsteroid, _prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileMediumAsteroid, _prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabMissile, OnMissileSmallAsteroid, _prefabAsteroidSmall, OnNoop);

			for (int i = 0; i < 3; i++)
				_entitiesManager.Spawn(
					_prefabAsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(_prefabAsteroidBig.transform.localScale.x), //Assuming its uniform scale
					Random.insideUnitCircle.normalized * Random.Range(_tweaktable.RandomBigAsteroidSpeedBetween.x, _tweaktable.RandomBigAsteroidSpeedBetween.y)
				);

			// Spawn the player at the center of the world bounds
			Instantiate(_prefabPlayer);
		}

		void OnMissileBigAsteroid(Vector2 position, Vector2 otherPosition)
		{
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForBigAsteroid));
		}

		void OnMissileMediumAsteroid(Vector2 position, Vector2 otherPosition)
		{
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForMediumAsteroid));
		}

		void OnMissileSmallAsteroid(Vector2 position, Vector2 otherPosition)
		{
			EventsCenter.Invoke(new GameEvents.AddPointsEvent(_tweaktable.PointsForSmallAsteroid));
		}

		void OnBigAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			_entitiesManager.Spawn(_prefabAsteroidMedium, position, dir * Random.Range(_tweaktable.RandomMediumAsteroidSpeedBetween.x, _tweaktable.RandomMediumAsteroidSpeedBetween.y));
		}

		void OnMediumAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			_entitiesManager.Spawn(_prefabAsteroidSmall, position, dir * Random.Range(_tweaktable.RandomSmallAsteroidSpeedBetween.x, _tweaktable.RandomSmallAsteroidSpeedBetween.y));
		}

		void OnNoop(Vector2 position, Vector2 otherPosition)
		{
		}
	}
}
