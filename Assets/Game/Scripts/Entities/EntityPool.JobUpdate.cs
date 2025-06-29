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
		public JobHandle ScheduleUpdate(Vector4 worldBounds)
		{
			if (_active == 0)
				return default;

			return new UpdateJob
			{
				positions = _objectPositionsArray,
				directionWithSpeed = _objectDirectionAndSpeedArray,
				deltaTime = Time.deltaTime,
				bounds = worldBounds,
				halfSize = _objectHalfSize
			}.Schedule(_transformAccessArray);
		}

		[BurstCompile(CompileSynchronously = true, FloatPrecision = FloatPrecision.Standard, FloatMode = FloatMode.Fast)]
		struct UpdateJob : IJobParallelForTransform
		{
			[ReadOnly] public NativeArray<float2> directionWithSpeed;
			public NativeArray<float2> positions;

			public float deltaTime;
			public float4 bounds;
			public float halfSize;

			[BurstCompile]
			public void Execute(int index, TransformAccess transform)
			{
				var pos = positions[index];

				pos += directionWithSpeed[index] * deltaTime;

				// Wrap position around bounds.
				// Since middle of world is (0,0) we can negate the position to wrap it

				//if (pos.x + halfSize < bounds.x || pos.x - halfSize > bounds.z)
				//	pos.x *= -1f;
				pos.x *= math.select(1f, -1f, pos.x + halfSize < bounds.x || pos.x - halfSize > bounds.z);

				//if (pos.y + halfSize < bounds.y || pos.y - halfSize > bounds.w)
				//	pos.y *= -1f;
				pos.y *= math.select(1f, -1f, pos.y + halfSize < bounds.y || pos.y - halfSize > bounds.w);

				positions[index] = pos;

				transform.localPosition = new float3(pos.xy, 0f);
			}
		}
	}
}
