using UnityEngine;
using UnityEngine.Assertions;

namespace mygame
{
	public class WorldBoundsManager : MonoBehaviour
	{
		Vector4 _bounds;

		void Awake()
		{
			var cam = Camera.main;
			Assert.IsNotNull(cam, "Main camera not found. Please ensure a camera is present in the scene.");

			_bounds = new Vector4(
				-cam.orthographicSize * cam.aspect,
				-cam.orthographicSize,
				cam.orthographicSize * cam.aspect,
				cam.orthographicSize
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
