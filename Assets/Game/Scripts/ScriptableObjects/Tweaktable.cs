using UnityEngine;

namespace mygame
{
	[CreateAssetMenu(fileName = "Tweaktable", menuName = "Scriptable Objects/Tweaktable")]
	public class Tweaktable : ScriptableObject
	{
		[Header("Asteroids")]
		[SerializeField] Vector2 _randomSpeedBetween;
		public Vector2 RandomSpeedBetween => _randomSpeedBetween;

		[Header("Missiles")]
		[SerializeField] float _missileSpeed;
		public float MissileSpeed => _missileSpeed;
	}
}
