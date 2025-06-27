using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace mygame
{
	public partial class EntityPool
	{
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

				colliderDistance = (_objectHalfSize + other._objectHalfSize) * (_objectHalfSize + other._objectHalfSize),
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
	}
}
