using Unity.Collections;
using Unity.Jobs;
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

		JobHandle _jobCollisionsBigVsBig;
		JobHandle _jobCollisionsMediumVsMedium;
		JobHandle _jobCollisionsMediumVsBig;
		JobHandle _jobCollisionsSmallVsSmall;
		JobHandle _jobCollisionsSmallVsMedium;
		JobHandle _jobCollisionsSmallVsBig;

		NativeArray<int> _bigVsBigCollisions;
		NativeArray<int> _mediumVsMediumCollisions;
		NativeArray<int> _mediumVsBigCollisions;
		NativeArray<int> _smallVsSmallCollisions;
		NativeArray<int> _smallVsMediumCollisions;
		NativeArray<int> _smallVsBigCollisions;

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

		void OnDestroy()
		{
			_jobCollisionsBigVsBig.Complete();
			_jobCollisionsMediumVsMedium.Complete();
			_jobCollisionsMediumVsBig.Complete();
			_jobCollisionsSmallVsSmall.Complete();
			_jobCollisionsSmallVsMedium.Complete();
			_jobCollisionsSmallVsBig.Complete();

			if (_bigVsBigCollisions.IsCreated) _bigVsBigCollisions.Dispose();
			if (_mediumVsMediumCollisions.IsCreated) _mediumVsMediumCollisions.Dispose();
			if (_mediumVsBigCollisions.IsCreated) _mediumVsBigCollisions.Dispose();
			if (_smallVsSmallCollisions.IsCreated) _smallVsSmallCollisions.Dispose();
			if (_smallVsMediumCollisions.IsCreated) _smallVsMediumCollisions.Dispose();
			if (_smallVsBigCollisions.IsCreated) _smallVsBigCollisions.Dispose();
		}

		void Start()
		{
			_entitiesManager.RegisterEntity(_prefabAsteroidBig);
			_entitiesManager.RegisterEntity(_prefabAsteroidMedium);
			_entitiesManager.RegisterEntity(_prefabAsteroidSmall);

			for (int i = 0; i < 200; i++)
				_entitiesManager.Spawn(
					_prefabAsteroidBig,
					_worldBoundsManager.GetRandomInsideBounds(1f),
					Random.insideUnitCircle.normalized * Random.Range(1f, 3f) //Random direction and a random speed from 1- to 3 units per second
				);
		}

		void Update()
		{
			/*
			_jobCollisionsBigVsBig.Complete();
			_jobCollisionsMediumVsMedium.Complete();
			_jobCollisionsMediumVsBig.Complete();
			_jobCollisionsSmallVsSmall.Complete();
			_jobCollisionsSmallVsMedium.Complete();
			_jobCollisionsSmallVsBig.Complete();

			IterateOverAndDispose(_bigVsBigCollisions, _poolAsteroidsBig, OnBigAsteroid);
			IterateOverAndDispose(_mediumVsMediumCollisions, _poolAsteroidsMedium, OnMediumAsteroid);
			IterateOverAndDispose(_mediumVsBigCollisions, _poolAsteroidsMedium, OnMediumAsteroid, _poolAsteroidsBig, OnBigAsteroid);
			IterateOverAndDispose(_smallVsSmallCollisions, _poolAsteroidsSmall, OnNoop);
			IterateOverAndDispose(_smallVsMediumCollisions, _poolAsteroidsSmall, OnNoop, _poolAsteroidsMedium, OnMediumAsteroid);
			IterateOverAndDispose(_smallVsBigCollisions, _poolAsteroidsSmall, OnNoop, _poolAsteroidsBig, OnBigAsteroid);

			_poolAsteroidsBig.FlushFreeIndices();
			_poolAsteroidsMedium.FlushFreeIndices();
			_poolAsteroidsSmall.FlushFreeIndices();

			//Schedule the collision jobs, but do not complete them until the next frame.
			_jobCollisionsBigVsBig = _poolAsteroidsBig.ScheduleCollisionsVs(_poolAsteroidsBig, out _bigVsBigCollisions);
			_jobCollisionsMediumVsMedium = _poolAsteroidsMedium.ScheduleCollisionsVs(_poolAsteroidsMedium, out _mediumVsMediumCollisions);
			_jobCollisionsMediumVsBig = _poolAsteroidsMedium.ScheduleCollisionsVs(_poolAsteroidsBig, out _mediumVsBigCollisions);
			_jobCollisionsSmallVsSmall = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsSmall, out _smallVsSmallCollisions);
			_jobCollisionsSmallVsMedium = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsMedium, out _smallVsMediumCollisions);
			_jobCollisionsSmallVsBig = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsBig, out _smallVsBigCollisions);
			*/
		}

		void IterateOverAndDispose(NativeArray<int> collisions, EntityPool pool, System.Action<Vector2, Vector2> onCollision)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var otherIndex = collisions[i] - 1; // Convert to 0-based index

					var posA = pool.GetPositionAtIndex(i);
					var posB = pool.GetPositionAtIndex(otherIndex);

					pool.Despawn(i);
					pool.Despawn(otherIndex);

					onCollision(posA, posB);
				}

			collisions.Dispose();
		}

		void IterateOverAndDispose(NativeArray<int> collisions, EntityPool poolA, System.Action<Vector2, Vector2> onCollisionA, EntityPool poolB, System.Action<Vector2, Vector2> onCollisionB)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var otherIndex = collisions[i] - 1; // Convert to 0-based index

					var posA = poolA.GetPositionAtIndex(i);
					var posB = poolB.GetPositionAtIndex(otherIndex);

					poolA.Despawn(i);
					poolB.Despawn(otherIndex);

					onCollisionA(posA, posB);
					onCollisionB(posB, posA);
				}

			collisions.Dispose();
		}

		void OnBigAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			// _poolAsteroidsMedium.Spawn(position, dir * Random.Range(1f, 3f));
		}

		void OnMediumAsteroid(Vector2 position, Vector2 otherPosition)
		{
			var dir = (position - otherPosition).normalized;
			// _poolAsteroidsSmall.Spawn(position, dir * Random.Range(1f, 3f));
		}

		void OnNoop(Vector2 position, Vector2 otherPosition)
		{
		}
	}
}
