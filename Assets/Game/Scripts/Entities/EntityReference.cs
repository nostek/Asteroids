using UnityEngine;

namespace mygame
{
	public readonly struct EntityReference
	{
		public EntityReference(EntityPool pool, int index) => (_pool, _index) = (pool, index);

		readonly EntityPool _pool;
		readonly int _index;

		public readonly Vector2 GetPosition() => _pool.GetPositionAtIndex(_index);

		public void Despawn() => _pool.Despawn(_index);
	}
}
