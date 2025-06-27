using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace mygame
{
	public partial class EntityPool
	{
		public JobHandle ScheduleUpdate(Vector4 bounds)
		{
			return new UpdateJob
			{
				positions = _objectPositionsArray,
				directionWithSpeed = _objectDirectionAndSpeedArray,
				deltaTime = Time.deltaTime,
				bounds = bounds,
				scale = _objectHalfSize
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
	}
}
