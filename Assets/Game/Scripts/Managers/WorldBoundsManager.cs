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

		public Vector2 GetRandomInsideBounds(float halfSize)
		{
			return new Vector2(
				Random.Range(_bounds.x + halfSize, _bounds.z - halfSize),
				Random.Range(_bounds.y + halfSize, _bounds.w - halfSize)
			);
		}

		public Vector4 Bounds => _bounds;
	}
}
