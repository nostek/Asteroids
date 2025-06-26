using UnityEngine;

namespace mygame
{
	public readonly struct EntityReference
	{
		public EntityReference(EntityPool pool, int index) => (_pool, _index) = (pool, index);

		readonly EntityPool _pool;
		readonly int _index;

		public readonly GameObject GetGameObject() => _pool.GetGameObjectAtIndex(_index);
		public readonly Vector2 GetPosition() => _pool.GetPositionAtIndex(_index);
		public readonly Vector2 GetDirectionAndSpeed() => _pool.GetDirectionAndSpeedAtIndex(_index);

		public readonly void SetDirectionAndSpeed(Vector2 directionAndSpeed) => _pool.SetDirectionAndSpeedAtIndex(_index, directionAndSpeed);

		public void Despawn() => _pool.Despawn(_index);
	}
}
