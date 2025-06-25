using UnityEngine;
using UnityEngine.Assertions;

namespace mygame
{
	public class WorldBoundsManager : MonoBehaviour
	{
		Vector4 _bounds;

		void Awake()
		{
			var camera = Camera.main;
			Assert.IsNotNull(camera, "Main camera not found. Please ensure a camera is present in the scene.");

			_bounds = new Vector4(
				-camera.orthographicSize * camera.aspect,
				-camera.orthographicSize,
				camera.orthographicSize * camera.aspect,
				camera.orthographicSize
			);
		}

		public Vector2 GetRandomInsideBounds(float size)
		{
			return new Vector2(
				Random.Range(_bounds.x + size, _bounds.z - size),
				Random.Range(_bounds.y + size, _bounds.w - size)
			);
		}

		public Vector4 Bounds => _bounds;
	}
}
