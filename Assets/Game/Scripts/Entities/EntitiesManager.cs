using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;

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

		readonly Dictionary<int, EntityData> _entityPools = new();

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

		class LifeTimeData
		{
			public EntityPool Pool;
			public float SecondsToLive;
		}

		readonly List<LifeTimeData> _lifeTimes = new();

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

		public void RegisterEntity(int keyEntity, GameObject prefab, float halfSize, int ensureCapacity = 10)
		{
			Assert.IsNotNull(prefab, "Prefab cant be null");
			Assert.IsFalse(_entityPools.ContainsKey(keyEntity), $"Entity is already registered: {keyEntity} Prefab: {prefab.name}");

			_entityPools.Add(keyEntity, new EntityData() { Pool = new EntityPool(prefab, halfSize, ensureCapacity) });
		}

		public void RegisterEntityLifetime(int keyEntity, float secondsToLive)
		{
			Assert.IsTrue(_entityPools.ContainsKey(keyEntity), $"Entity is not registered: {keyEntity}");

			_lifeTimes.Add(new LifeTimeData()
			{
				Pool = _entityPools[keyEntity].Pool,
				SecondsToLive = secondsToLive
			});
		}

		public void RegisterCollisionSolver(int keyEntity, CollisionSolverDelegate solver)
		{
			Assert.IsNotNull(solver, "Solver cant be null.");
			Assert.IsTrue(_entityPools.ContainsKey(keyEntity), $"Entity is not registered: {keyEntity}");

			_collisionSolvers.Add(new SolverData()
			{
				PoolA = _entityPools[keyEntity].Pool,
				PoolB = _entityPools[keyEntity].Pool,
				SolverA = solver,
				SolverB = null
			});
		}

		public void RegisterCollisionSolver(int keyEntityA, CollisionSolverDelegate solverA, int keyEntityB, CollisionSolverDelegate solverB)
		{
			Assert.IsTrue(_entityPools.ContainsKey(keyEntityA), $"Entity is not registered: {keyEntityA}");
			Assert.IsTrue(_entityPools.ContainsKey(keyEntityB), $"Entity is not registered: {keyEntityB}");
			Assert.IsNotNull(solverA, "Solver cant be null.");
			Assert.IsNotNull(solverB, "Solver cant be null.");

			_collisionSolvers.Add(new SolverData()
			{
				PoolA = _entityPools[keyEntityA].Pool,
				PoolB = _entityPools[keyEntityB].Pool,
				SolverA = solverA,
				SolverB = solverB
			});
		}

		/// <summary>
		/// Returns an EntityReference to the newly spawned Entity.
 		/// The reference is only valid until the next frame.
 		/// Use ToPermanent if a longer reference is needed.
		/// </summary>
		public EntityReference Spawn(int keyEntity, Vector2 position, Vector2 directionWithSpeed)
		{
			Assert.IsTrue(_entityPools.ContainsKey(keyEntity), $"Entity is not registered: {keyEntity}");

			return _entityPools[keyEntity].Pool.Spawn(position, directionWithSpeed);
		}

		/// <summary>
		/// Returns an EntityReference to the newly spawned Entity as ToPermanent().
		/// It has slower access to its internals, but it can be used until it is despawned.
		/// </summary>
		public EntityReference SpawnAsPermanent(int keyEntity, Vector2 position, Vector2 directionWithSpeed)
		{
			return Spawn(keyEntity, position, directionWithSpeed).ToPermanent();
		}

		public void RunUpdate(Vector4 worldBounds)
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

			//Iterate over all entities that has a lifetime
			//Run this before we flush despawned entries
			foreach (var data in _lifeTimes)
				data.Pool.DespawnOlderThan(data.SecondsToLive);

			//Free all the entities here so nothing is dependent on its values
			foreach (var data in _entityPools.Values)
				data.Pool.FlushFreeIndices();

			//We want these jobs to run in parallel, so we schedule them first, then complete them in the correct order.
			foreach (var data in _entityPools.Values)
				data.Job = data.Pool.ScheduleUpdate(worldBounds);
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
