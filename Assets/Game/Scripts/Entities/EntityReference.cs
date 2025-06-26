using UnityEngine;

namespace mygame
{
	public readonly struct EntityReference
	{
		public EntityReference(EntityPool pool, int index) => (_pool, _index, _transform) = (pool, index, null);

		//Private constructor as you should not be able to construct this from anywhere.
		EntityReference(EntityPool pool, Transform transform) => (_pool, _index, _transform) = (pool, -1, transform);

		readonly EntityPool _pool;
		readonly int _index;
		readonly Transform _transform;

		readonly int GetIndex() => _index >= 0 ? _index : _pool.FindIndex(_transform);

		public readonly GameObject GetGameObject() => _pool.GetGameObjectAtIndex(GetIndex());
		public readonly Vector2 GetPosition() => _pool.GetPositionAtIndex(GetIndex());
		public readonly Vector2 GetDirectionAndSpeed() => _pool.GetDirectionAndSpeedAtIndex(GetIndex());

		public readonly void SetDirectionAndSpeed(Vector2 directionAndSpeed) => _pool.SetDirectionAndSpeedAtIndex(GetIndex(), directionAndSpeed);

		public void Despawn() => _pool.Despawn(GetIndex());

		public EntityReference ToPermanent()
		{
			return new EntityReference(_pool, GetGameObject().transform);
		}
	}
}
