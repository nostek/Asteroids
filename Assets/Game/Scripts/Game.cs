using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
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

		PooledObjectManager _poolAsteroidsBig;
		PooledObjectManager _poolAsteroidsMedium;
		PooledObjectManager _poolAsteroidsSmall;

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

		void Awake()
		{
			GetComponent<ServiceBehaviour>().Install();

			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Done();

			Assert.IsNotNull(_prefabAsteroidBig, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidMedium, "Prefab object is not assigned. Please assign a prefab in the inspector.");
			Assert.IsNotNull(_prefabAsteroidSmall, "Prefab object is not assigned. Please assign a prefab in the inspector.");

			_poolAsteroidsBig = new PooledObjectManager(_prefabAsteroidBig);
			_poolAsteroidsMedium = new PooledObjectManager(_prefabAsteroidMedium);
			_poolAsteroidsSmall = new PooledObjectManager(_prefabAsteroidSmall);
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

			_poolAsteroidsBig?.Dispose();
			_poolAsteroidsMedium?.Dispose();
			_poolAsteroidsSmall?.Dispose();
		}

		void Start()
		{
			for (int i = 0; i < 100; i++)
				_poolAsteroidsBig.Spawn(
					_worldBoundsManager.GetRandomInsideBounds(1f),
					Random.insideUnitCircle.normalized * Random.Range(1f, 3f) //Random direction and a random speed from 1- to 3 units per second
				);
		}

		void Update()
		{
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

			//We want these jobs to run in parallel, so we schedule them and complete them in the correct order.
			var jobBig = _poolAsteroidsBig.ScheduleUpdate(_worldBoundsManager.Bounds);
			var jobMedium = _poolAsteroidsMedium.ScheduleUpdate(_worldBoundsManager.Bounds);
			var jobSmall = _poolAsteroidsSmall.ScheduleUpdate(_worldBoundsManager.Bounds);
			jobBig.Complete();
			jobMedium.Complete();
			jobSmall.Complete();

			//Schedule the collision jobs, but do not complete them until the next frame.
			_jobCollisionsBigVsBig = _poolAsteroidsBig.ScheduleCollisionsVs(_poolAsteroidsBig, out _bigVsBigCollisions);
			_jobCollisionsMediumVsMedium = _poolAsteroidsMedium.ScheduleCollisionsVs(_poolAsteroidsMedium, out _mediumVsMediumCollisions);
			_jobCollisionsMediumVsBig = _poolAsteroidsMedium.ScheduleCollisionsVs(_poolAsteroidsBig, out _mediumVsBigCollisions);
			_jobCollisionsSmallVsSmall = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsSmall, out _smallVsSmallCollisions);
			_jobCollisionsSmallVsMedium = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsMedium, out _smallVsMediumCollisions);
			_jobCollisionsSmallVsBig = _poolAsteroidsSmall.ScheduleCollisionsVs(_poolAsteroidsBig, out _smallVsBigCollisions);
		}

		void IterateOverAndDispose(NativeArray<int> collisions, PooledObjectManager pool, System.Action<Vector2> onCollision)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var pos = pool.GetPositionAtIndex(i);

					if (pool != _poolAsteroidsSmall)
						pool.Despawn(i);

					onCollision(pos);
				}

			collisions.Dispose();
		}

		void IterateOverAndDispose(NativeArray<int> collisions, PooledObjectManager poolA, System.Action<Vector2> onCollisionA, PooledObjectManager poolB, System.Action<Vector2> onCollisionB)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var otherIndex = collisions[i] - 1; // Convert to 0-based index

					var posA = poolA.GetPositionAtIndex(i);
					var posB = poolB.GetPositionAtIndex(otherIndex);

					if (poolA != _poolAsteroidsSmall)
						poolA.Despawn(i);
					poolB.Despawn(otherIndex);

					onCollisionA(posA);
					onCollisionB(posB);
				}

			collisions.Dispose();
		}

		void OnBigAsteroid(Vector2 position)
		{
			_poolAsteroidsMedium.Spawn(position, Random.insideUnitCircle.normalized * Random.Range(1f, 3f));
		}

		void OnMediumAsteroid(Vector2 position)
		{
			_poolAsteroidsSmall.Spawn(position, Random.insideUnitCircle.normalized * Random.Range(1f, 3f));
		}

		void OnNoop(Vector2 position)
		{
		}
	}
}
