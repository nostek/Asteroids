using UnityEngine;
using UnityEngine.Assertions;
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

		WorldBoundsManager _worldBoundsManager;
		EntitiesManager _entitiesManager;

		void Awake()
		{
			GetComponent<ServiceBehaviour>().Install();

			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Get(out _entitiesManager)
				.Done();

			Assert.IsNotNull(_prefabAsteroidBig, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidMedium, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidSmall, "Prefab object is not assigned. Please assign a prefab in the inspector.");
		}

		void Start()
		{
			_entitiesManager.RegisterEntity(_prefabAsteroidBig);
			_entitiesManager.RegisterEntity(_prefabAsteroidMedium);
			_entitiesManager.RegisterEntity(_prefabAsteroidSmall);

			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidMedium, OnMediumAsteroid, _prefabAsteroidBig, OnBigAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop, _prefabAsteroidMedium, OnMediumAsteroid);
			_entitiesManager.RegisterCollisionSolver(_prefabAsteroidSmall, OnNoop, _prefabAsteroidBig, OnBigAsteroid);

			for (int i = 0; i < 10; i++)
				_entitiesManager.Spawn(
					_prefabAsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(1f),
					Random.insideUnitCircle.normalized * Random.Range(1f, 3f) //Random direction and a random speed from 1- to 3 units per second
				);
		}

		void OnBigAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			_entitiesManager.Spawn(_prefabAsteroidMedium, position, dir * Random.Range(1f, 3f));
		}

		void OnMediumAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			_entitiesManager.Spawn(_prefabAsteroidSmall, position, dir * Random.Range(1f, 3f));
		}

		void OnNoop(Vector2 position, Vector2 otherPosition)
		{
		}
	}
}
