using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;

namespace mygame
{
	public class PooledObjectManager : System.IDisposable
	{
		struct ObjectData
		{
			public bool active;
			public float2 directionWithSpeed;
		}

		readonly GameObject _prefabObject;
		readonly float _objectScale = 1f;

		Transform[] _pooledObjects;
		NativeArray<ObjectData> _objectDataArray;
		NativeArray<float2> _objectPositionsArray;
		TransformAccessArray _transformAccessArray;

		Transform _parent;

		public PooledObjectManager(GameObject prefab)
		{
			Assert.IsNotNull(prefab, "Prefab object is not supplied");
			_prefabObject = prefab;
			_objectScale = prefab.transform.localScale.x * 0.5f; // Half the size and assuming uniform scale

			_parent = new GameObject($"Pool {prefab.name}").transform;

			EnsureCapacity(500);
		}

		public void Dispose()
		{
			if (_objectDataArray.IsCreated) _objectDataArray.Dispose();
			if (_objectPositionsArray.IsCreated) _objectPositionsArray.Dispose();
			if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

			if (_parent != null)
			{
				GameObject.Destroy(_parent.gameObject);
				_parent = null;
			}
		}

		public void Spawn(Vector2 position, Vector2 directionWithSpeed)
		{
			int index = GetFreeIndex();

			_objectDataArray[index] = new ObjectData
			{
				active = true,
				directionWithSpeed = directionWithSpeed
			};

			_objectPositionsArray[index] = position;

			var pooledObject = _pooledObjects[index];
			pooledObject.SetLocalPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, directionWithSpeed));
			pooledObject.gameObject.SetActive(true);
		}

		public void Despawn(GameObject pooledObject)
		{
			var index = System.Array.IndexOf(_pooledObjects, pooledObject);
			Assert.IsTrue(index >= 0, "Pooled object not found in the pool.");

			Despawn(index);
		}

		public void Despawn(int index)
		{
			Assert.IsTrue(index >= 0 && index < _pooledObjects.Length, "Index out of bounds for pooled objects.");
			Assert.IsTrue(_objectDataArray[index].active, "Index is not active in the pool.");

			_objectDataArray[index] = new ObjectData { active = false };
			_pooledObjects[index].gameObject.SetActive(false);
		}

		public Vector2 GetPositionAtIndex(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objectPositionsArray.Length, "Index out of bounds for object positions.");
			return _objectPositionsArray[index];
		}

		int GetFreeIndex()
		{
			for (int i = 0; i < _objectDataArray.Length; i++)
				if (!_objectDataArray[i].active)
					return i;

			EnsureCapacity(_objectDataArray.Length * 2); // Double the capacity if no free index found
			return GetFreeIndex();
		}

		void EnsureCapacity(int capacity)
		{
			if (_objectDataArray.IsCreated && _objectDataArray.Length >= capacity)
				return;

			//TODO: Rescale the existing arrays if needed

			_objectDataArray = new NativeArray<ObjectData>(capacity, Allocator.Persistent);
			_objectPositionsArray = new NativeArray<float2>(capacity, Allocator.Persistent);

			_pooledObjects = new Transform[capacity];
			for (int i = 0; i < capacity; i++)
			{
				var go = GameObject.Instantiate(_prefabObject, _parent);
				go.SetActive(false);
				_pooledObjects[i] = go.transform;
			}

			if (_transformAccessArray.isCreated)
				_transformAccessArray.Dispose();
			_transformAccessArray = new TransformAccessArray(_pooledObjects);
		}

		#region SCHEDULE UPDATE

		public JobHandle ScheduleUpdate(Vector4 bounds)
		{
			return new UpdateJob
			{
				data = _objectDataArray,
				positions = _objectPositionsArray,
				deltaTime = Time.deltaTime,
				bounds = bounds,
				scale = _objectScale
			}.Schedule(_transformAccessArray);
		}

		[BurstCompile]
		struct UpdateJob : IJobParallelForTransform
		{
			[ReadOnly] public NativeArray<ObjectData> data;
			public NativeArray<float2> positions;

			public float deltaTime;
			public float4 bounds;
			public float scale;

			[BurstCompile]
			public void Execute(int index, TransformAccess transform)
			{
				if (!data[index].active)
					return;

				var pos = positions[index];

				pos += data[index].directionWithSpeed * deltaTime;

				if (pos.x + scale < bounds.x || pos.x - scale > bounds.z)
					pos.x *= -1f;

				if (pos.y + scale < bounds.y || pos.y - scale > bounds.w)
					pos.y *= -1f;

				positions[index] = pos;

				transform.localPosition = new float3(pos.xy, 0f);
			}
		}

		#endregion

		#region SCHEDULE COLLISIONS

		public JobHandle ScheduleCollisionsVs(PooledObjectManager other, out NativeArray<int> collisions)
		{
			collisions = new NativeArray<int>(_objectDataArray.Length, Allocator.TempJob, NativeArrayOptions.ClearMemory);

			return new CollisionsJob()
			{
				a_data = _objectDataArray,
				a_positions = _objectPositionsArray,

				b_data = other._objectDataArray,
				b_positions = other._objectPositionsArray,

				colliderDistance = (_objectScale + other._objectScale) * (_objectScale + other._objectScale),
				collisions = collisions,
				ignoreSameIndex = this == other // Avoid self-collision if comparing with itself
			}.Schedule(_objectDataArray.Length, 32);
		}

		[BurstCompile]
		struct CollisionsJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<ObjectData> a_data;
			[ReadOnly] public NativeArray<float2> a_positions;

			[ReadOnly] public NativeArray<ObjectData> b_data;
			[ReadOnly] public NativeArray<float2> b_positions;

			public float colliderDistance;
			public NativeArray<int> collisions;
			public bool ignoreSameIndex;

			[BurstCompile]
			public void Execute(int index)
			{
				if (!a_data[index].active)
					return;

				var a_pos = a_positions[index];

				for (int i = 0; i < b_data.Length; i++)
				{
					if (ignoreSameIndex && i == index)
						continue;   //skip self-collision

					if (!b_data[i].active)
						continue;

					if (math.distancesq(a_pos, b_positions[i]) < colliderDistance)
					{
						collisions[index] = i + 1; // Store the index+1 of the collision. +1 to differentiate from no collision (0)
					}
				}
			}
		}

		#endregion
	}
}
