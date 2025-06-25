using System.Collections.Generic;
using NUnit.Framework;
using Unity.Jobs;
using UnityEngine;
using UnityServiceLocator;

namespace mygame
{
	public class EntitiesManager : MonoBehaviour
	{
		class EntityData
		{
			public EntityPool Pool;
			public JobHandle Job;
		}

		readonly Dictionary<GameObject, EntityData> _entityPools = new();

		WorldBoundsManager _worldBoundsManager;

		void Awake()
		{
			ServiceLocator.Lookup
				.Get(out _worldBoundsManager)
				.Done();
		}

		void OnDestroy()
		{
			foreach (var data in _entityPools.Values)
				data.Pool.Dispose();
		}

		public void RegisterEntity(GameObject prefab)
		{
			Assert.IsFalse(_entityPools.ContainsKey(prefab), "Entity prefab is already registered: " + prefab.name);

			_entityPools.Add(prefab, new EntityData() { Pool = new EntityPool(prefab) });
		}

		public void Spawn(GameObject prefab, Vector2 position, Vector2 directionWithSpeed)
		{
			_entityPools[prefab].Pool.Spawn(position, directionWithSpeed);
		}

		void Update()
		{
			//We want these jobs to run in parallel, so we schedule them and complete them in the correct order.
			foreach (var data in _entityPools.Values)
				data.Job = data.Pool.ScheduleUpdate(_worldBoundsManager.Bounds);
			foreach (var data in _entityPools.Values)
				data.Job.Complete();
		}
	}
}
