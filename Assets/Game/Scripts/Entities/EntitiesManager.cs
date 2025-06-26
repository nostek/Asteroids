using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityServiceLocator;

namespace mygame
{
	public class EntitiesManager : MonoBehaviour
	{
		public delegate void CollisionSolverDelegate(EntityReference collider, EntityReference otherCollider);

		class EntityData
		{
			public EntityPool Pool;
			public JobHandle Job;
		}

		readonly Dictionary<GameObject, EntityData> _entityPools = new();

		class SolverData
		{
			public EntityPool PoolA;
			public EntityPool PoolB;

			public CollisionSolverDelegate SolverA;
			public CollisionSolverDelegate SolverB;

			public JobHandle Job;
			public NativeArray<int> Collisions;
		}

		readonly List<SolverData> _collisionSolvers = new();

		WorldBoundsManager _worldBoundsManager;

		void Awake()
		{
			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Done();
		}

		void OnDestroy()
		{
			//Complete the jobs first to ensure all operations are finished before disposing
			foreach (var data in _collisionSolvers)
				data.Job.Complete();

			foreach (var data in _collisionSolvers)
				if (data.Collisions.IsCreated) data.Collisions.Dispose();

			foreach (var data in _entityPools.Values)
				data.Pool.Dispose();
		}

		public void RegisterEntity(GameObject prefab, int ensureCapacity = 10)
		{
			Assert.IsFalse(_entityPools.ContainsKey(prefab), "Entity prefab is already registered: " + prefab.name);

			_entityPools.Add(prefab, new EntityData() { Pool = new EntityPool(prefab, ensureCapacity) });
		}

		public void RegisterCollisionSolver(GameObject prefab, CollisionSolverDelegate solver)
		{
			Assert.IsTrue(_entityPools.ContainsKey(prefab), "Entity prefab is not registered: " + prefab.name);
			Assert.IsNotNull(solver, "Solver cant be null.");

			_collisionSolvers.Add(new SolverData()
			{
				PoolA = _entityPools[prefab].Pool,
				PoolB = _entityPools[prefab].Pool,
				SolverA = solver,
				SolverB = null
			});
		}

		public void RegisterCollisionSolver(GameObject prefabA, CollisionSolverDelegate solverA, GameObject prefabB, CollisionSolverDelegate solverB)
		{
			Assert.IsTrue(_entityPools.ContainsKey(prefabA), "Entity prefab is not registered: " + prefabA.name);
			Assert.IsTrue(_entityPools.ContainsKey(prefabB), "Entity prefab is not registered: " + prefabB.name);
			Assert.IsNotNull(solverA, "Solver cant be null.");
			Assert.IsNotNull(solverB, "Solver cant be null.");

			_collisionSolvers.Add(new SolverData()
			{
				PoolA = _entityPools[prefabA].Pool,
				PoolB = _entityPools[prefabB].Pool,
				SolverA = solverA,
				SolverB = solverB
			});
		}

		public void Spawn(GameObject prefab, Vector2 position, Vector2 directionWithSpeed)
		{
			_entityPools[prefab].Pool.Spawn(position, directionWithSpeed);
		}

		void Update()
		{
			//Complete the jobs first to ensure all operations are finished before iterating over results
			foreach (var data in _collisionSolvers)
				data.Job.Complete();

			foreach (var data in _collisionSolvers)
			{
				if (data.SolverB == null)
					IterateOverAndDispose(data.Collisions, data.PoolA, data.SolverA);
				else
					IterateOverAndDispose(data.Collisions, data.PoolA, data.SolverA, data.PoolB, data.SolverB);
			}

			//Free all the entities here so nothing is dependent on its values
			foreach (var data in _entityPools.Values)
				data.Pool.FlushFreeIndices();

			//We want these jobs to run in parallel, so we schedule them first, then complete them in the correct order.
			foreach (var data in _entityPools.Values)
				data.Job = data.Pool.ScheduleUpdate(_worldBoundsManager.Bounds);
			foreach (var data in _entityPools.Values)
				data.Job.Complete();
		}

		void LateUpdate()
		{
			//Schedule the collision-check jobs so they run over to the next frame.
			//That gives us a lot more time for the jobs to finish.
			foreach (var data in _collisionSolvers)
				data.Job = data.PoolA.ScheduleCollisionsVs(data.PoolB, out data.Collisions);
		}

		static void IterateOverAndDispose(NativeArray<int> collisions, EntityPool pool, CollisionSolverDelegate onCollision)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var otherIndex = collisions[i] - 1; // Convert to 0-based index

					onCollision(new(pool, i), new(pool, otherIndex));
				}

			collisions.Dispose();
		}

		static void IterateOverAndDispose(NativeArray<int> collisions, EntityPool poolA, CollisionSolverDelegate onCollisionA, EntityPool poolB, CollisionSolverDelegate onCollisionB)
		{
			if (!collisions.IsCreated)
				return;

			for (int i = 0; i < collisions.Length; i++)
				if (collisions[i] > 0)
				{
					var otherIndex = collisions[i] - 1; // Convert to 0-based index

					onCollisionA(new(poolA, i), new(poolB, otherIndex));
					onCollisionB(new(poolB, otherIndex), new(poolA, i));
				}

			collisions.Dispose();
		}
	}
}
