using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;

namespace mygame
{
	public class EntityPool : System.IDisposable
	{
		readonly GameObject _prefabObject;
		readonly float _objectScale;

		readonly List<int> _freeIndices = new();

		int _active = 0;
		Transform[] _objects;
		NativeArray<float2> _objectPositionsArray;
		NativeArray<float2> _objectDirectionAndSpeedArray;
		TransformAccessArray _transformAccessArray;

		Transform _parent;

		public EntityPool(GameObject prefab, int ensureCapacity)
		{
			Assert.IsNotNull(prefab, "Prefab object is not supplied");
			_prefabObject = prefab;
			_objectScale = prefab.transform.localScale.x * 0.5f; // Half the size and assuming uniform scale
			_active = 0;

			_parent = new GameObject($"Pool {prefab.name}").transform;

			EnsureCapacity(ensureCapacity);
		}

		public void Dispose()
		{
			if (_objectDirectionAndSpeedArray.IsCreated) _objectDirectionAndSpeedArray.Dispose();
			if (_objectPositionsArray.IsCreated) _objectPositionsArray.Dispose();
			if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

			if (_parent != null)
			{
				Object.Destroy(_parent.gameObject);
				_parent = null;
			}
		}

		public EntityReference Spawn(Vector2 position, Vector2 directionWithSpeed)
		{
			int index = GetFreeIndex();

			_objectPositionsArray[index] = position;
			_objectDirectionAndSpeedArray[index] = directionWithSpeed;

			var obj = _objects[index];
			obj.SetLocalPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, directionWithSpeed.normalized));
			obj.gameObject.SetActive(true);

			return new EntityReference(this, index);
		}

		/*
		public void Despawn(GameObject pooledObject)
		{
			var index = System.Array.IndexOf(_pooledObjects, pooledObject);
			Assert.IsTrue(index >= 0, "Pooled object not found in the pool.");

			Despawn(index);
		}
		*/

		public void Despawn(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objects.Length, "Index out of bounds for pooled objects.");
			Assert.IsTrue(index < _active, $"Index is not active in the pool. {index} < {_active}");

			if (!_freeIndices.Contains(index))
				_freeIndices.Add(index);
		}

		public GameObject GetGameObjectAtIndex(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objectPositionsArray.Length, "Index out of bounds for object positions.");
			return _objects[index].gameObject;
		}

		public Vector2 GetPositionAtIndex(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objectPositionsArray.Length, "Index out of bounds for object positions.");
			return _objectPositionsArray[index];
		}

		public Vector2 GetDirectionAndSpeedAtIndex(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objectPositionsArray.Length, "Index out of bounds for object positions.");
			return _objectDirectionAndSpeedArray[index];
		}

		public void SetDirectionAndSpeedAtIndex(int index, Vector2 directionAndSpeed)
		{
			Assert.IsTrue(index >= 0 && index < _objectPositionsArray.Length, "Index out of bounds for object positions.");
			_objectDirectionAndSpeedArray[index] = directionAndSpeed;
		}

		public void FlushFreeIndices()
		{
			// Not super happy about this. It needs a sort so we don't remove things after _active has been decremented.

			if (_freeIndices.Count == 0)
				return;

			_freeIndices.Sort();

			for (int i = _freeIndices.Count - 1; i >= 0; i--)
				DespawnNow(_freeIndices[i]);

			_freeIndices.Clear();
		}

		void DespawnNow(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objects.Length, "Index out of bounds for pooled objects.");
			Assert.IsTrue(index < _active, "Index is not active in the pool.");

			_objects[index].gameObject.SetActive(false);

			_objectPositionsArray[index] = _objectPositionsArray[_active - 1]; // Just copy the values from the active element, as the current index data can be discarded
			_objectDirectionAndSpeedArray[index] = _objectDirectionAndSpeedArray[_active - 1];

			(_objects[index], _objects[_active - 1]) = (_objects[_active - 1], _objects[index]); // Swap the objects in the array

			(_transformAccessArray[index], _transformAccessArray[_active - 1]) = (_transformAccessArray[_active - 1], _transformAccessArray[index]); // Swap the objects in the array

			_active--;
		}

		int GetFreeIndex()
		{
			if (_active >= _objectPositionsArray.Length)
				EnsureCapacity(_objectPositionsArray.Length * 2); // Double the capacity if no free index found

			return _active++; //return the current active count and increment it
		}

		void EnsureCapacity(int capacity)
		{
			if (_objectPositionsArray.IsCreated && _objectPositionsArray.Length >= capacity)
				return;

			_objectPositionsArray = CreateOrScaleArray(_objectPositionsArray, capacity);
			_objectDirectionAndSpeedArray = CreateOrScaleArray(_objectDirectionAndSpeedArray, capacity);

			static NativeArray<T> CreateOrScaleArray<T>(NativeArray<T> current, int capacity) where T : struct
			{
				//Make a new NativeArray and copy over excisting data if any.
				//Dispose the excisting array if it excists.
				var ret = new NativeArray<T>(capacity, Allocator.Persistent, NativeArrayOptions.ClearMemory);
				if (current.IsCreated)
				{
					NativeArray<T>.Copy(current, ret, current.Length);
					current.Dispose();
				}
				return ret;
			}

			//Rescale the objects array and copy over excisting data if any
			{
				var objects = new Transform[capacity];
				if (_objects != null)
					System.Array.Copy(_objects, objects, _objects.Length);
				for (int i = _objects != null ? _objects.Length : 0; i < capacity; i++)
				{
					var go = Object.Instantiate(_prefabObject, _parent);
					go.SetActive(false);
					objects[i] = go.transform;
				}
				_objects = objects;
			}

			//Recreate the transform access array with the new values
			if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
			_transformAccessArray = new TransformAccessArray(_objects);
		}

		#region SCHEDULE UPDATE

		public JobHandle ScheduleUpdate(Vector4 bounds)
		{
			return new UpdateJob
			{
				positions = _objectPositionsArray,
				directionWithSpeed = _objectDirectionAndSpeedArray,
				deltaTime = Time.deltaTime,
				bounds = bounds,
				scale = _objectScale
			}.Schedule(_transformAccessArray);
		}

		[BurstCompile]
		struct UpdateJob : IJobParallelForTransform
		{
			[ReadOnly] public NativeArray<float2> directionWithSpeed;
			public NativeArray<float2> positions;

			public float deltaTime;
			public float4 bounds;
			public float scale;

			[BurstCompile]
			public void Execute(int index, TransformAccess transform)
			{
				var pos = positions[index];

				pos += directionWithSpeed[index] * deltaTime;

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

		public JobHandle ScheduleCollisionsVs(EntityPool other, out NativeArray<int> collisions)
		{
			collisions = new NativeArray<int>(_active, Allocator.TempJob, NativeArrayOptions.ClearMemory);

			if (_objectPositionsArray.Length == 0 || other._objectPositionsArray.Length == 0)
			{
				return new JobHandle(); // No objects to compare, return an empty job handle
			}

			return new CollisionsJob()
			{
				a_positions = _objectPositionsArray,
				b_positions = other._objectPositionsArray,
				b_positions_length = other._active,

				colliderDistance = (_objectScale + other._objectScale) * (_objectScale + other._objectScale),
				collisions = collisions,
				ignoreSameIndex = this == other // Avoid self-collision if comparing with itself
			}.Schedule(_active, 32);
		}

		[BurstCompile]
		struct CollisionsJob : IJobParallelFor
		{
			[ReadOnly] public NativeArray<float2> a_positions;
			[ReadOnly] public NativeArray<float2> b_positions;
			public int b_positions_length;

			public float colliderDistance;
			public NativeArray<int> collisions;
			public bool ignoreSameIndex;

			[BurstCompile]
			public void Execute(int index)
			{
				var a_pos = a_positions[index];

				for (int i = 0; i < b_positions_length; i++)
				{
					if (ignoreSameIndex && i == index)
						continue;   //skip self-collision

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
