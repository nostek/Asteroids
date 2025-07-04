using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;

namespace mygame
{
	public partial class EntityPool : System.IDisposable
	{
		readonly GameObject _prefabObject;
		readonly float _objectHalfSize;

		readonly List<int> _freeIndices = new();

		int _active = 0;
		Transform[] _objects;
		NativeArray<float2> _objectPositionsArray;
		NativeArray<float2> _objectDirectionAndSpeedArray;
		NativeArray<float> _objectLifetime;
		TransformAccessArray _transformAccessArray;

		Transform _parent;

		public EntityPool(GameObject prefab, float halfSize, int ensureCapacity)
		{
			Assert.IsNotNull(prefab, "Prefab object is not supplied");
			_prefabObject = prefab;
			_objectHalfSize = halfSize;
			_active = 0;

			_parent = new GameObject($"Pool {prefab.name}").transform;

			EnsureCapacity(ensureCapacity);
		}

		public void Dispose()
		{
			if (_objectPositionsArray.IsCreated) _objectPositionsArray.Dispose();
			if (_objectDirectionAndSpeedArray.IsCreated) _objectDirectionAndSpeedArray.Dispose();
			if (_objectLifetime.IsCreated) _objectLifetime.Dispose();
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
			_objectLifetime[index] = Time.time;

			var obj = _objects[index];
			obj.SetLocalPositionAndRotation(position, Quaternion.LookRotation(Vector3.forward, directionWithSpeed.normalized));
			obj.gameObject.SetActive(true);

			return new EntityReference(this, index);
		}

		public void Despawn(int index)
		{
			Assert.IsTrue(index >= 0 && index < _objects.Length, "Index out of bounds for pooled objects.");
			Assert.IsTrue(index < _active, $"Index is not active in the pool. {index} < {_active}");

			if (!_freeIndices.Contains(index))
				_freeIndices.Add(index);
		}

		public void DespawnOlderThan(float seconds)
		{
			var expirationTime = Time.time - seconds;

			for (int i = 0; i < _active; i++)
				if (_objectLifetime[i] < expirationTime)
					Despawn(i);
		}

		public int FindIndex(Transform transform)
		{
			var index = System.Array.IndexOf(_objects, transform, 0, _active);
			Assert.IsTrue(index >= 0, "Object not found in the pool.");
			return index;
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
			// Not super happy about this. It needs to be sorted, so we don't remove things after _active has been decremented.

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

			_objectPositionsArray[index] = _objectPositionsArray[_active - 1]; // Only copy the values from the active element, as the current index data can be discarded
			_objectDirectionAndSpeedArray[index] = _objectDirectionAndSpeedArray[_active - 1];
			_objectLifetime[index] = _objectLifetime[_active - 1];

			(_objects[index], _objects[_active - 1]) = (_objects[_active - 1], _objects[index]); // Swap the objects in the array

			(_transformAccessArray[index], _transformAccessArray[_active - 1]) = (_transformAccessArray[_active - 1], _transformAccessArray[index]); // Swap the objects in the array

			_active--;
		}

		int GetFreeIndex()
		{
			if (_active >= _objectPositionsArray.Length)
				EnsureCapacity(_objectPositionsArray.Length * 2); // Double the capacity if no free index found

			return _active++; //return the current active count and increment it afterward
		}

		void EnsureCapacity(int capacity)
		{
			if (_objectPositionsArray.IsCreated && _objectPositionsArray.Length >= capacity)
				return;

			_objectPositionsArray = CreateOrScaleArray(_objectPositionsArray, capacity);
			_objectDirectionAndSpeedArray = CreateOrScaleArray(_objectDirectionAndSpeedArray, capacity);
			_objectLifetime = CreateOrScaleArray(_objectLifetime, capacity);

			static NativeArray<T> CreateOrScaleArray<T>(NativeArray<T> current, int capacity) where T : struct
			{
				//Make a new NativeArray and copy over existing data if any.
				//Dispose the existing array if it exists.
				var ret = new NativeArray<T>(capacity, Allocator.Persistent, NativeArrayOptions.ClearMemory);
				if (current.IsCreated)
				{
					NativeArray<T>.Copy(current, ret, current.Length);
					current.Dispose();
				}
				return ret;
			}

			//Rescale the 'objects' array and copy over existing data if any
			{
				var objects = new Transform[capacity];
				if (_objects != null)
					System.Array.Copy(_objects, objects, _objects.Length);
				for (int i = _objects?.Length ?? 0; i < capacity; i++)
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
	}
}
