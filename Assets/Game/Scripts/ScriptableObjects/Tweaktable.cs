using UnityEngine;

namespace mygame
{
	[CreateAssetMenu(fileName = "Tweaktable", menuName = "Scriptable Objects/Tweaktable")]
	public class Tweaktable : ScriptableObject
	{
		[Header("Asteroids")]
		[SerializeField] Vector2 _randomBigAsteroidSpeedBetween;
		[SerializeField] Vector2 _randomMediumAsteroidSpeedBetween;
		[SerializeField] Vector2 _randomSmallAsteroidSpeedBetween;
		public float RandomBigAsteroidSpeedBetween => Random.Range(_randomBigAsteroidSpeedBetween.x, _randomBigAsteroidSpeedBetween.y);
		public float RandomMediumAsteroidSpeedBetween => Random.Range(_randomMediumAsteroidSpeedBetween.x, _randomMediumAsteroidSpeedBetween.y);
		public float RandomSmallAsteroidSpeedBetween => Random.Range(_randomSmallAsteroidSpeedBetween.x, _randomSmallAsteroidSpeedBetween.y);

		[Header("Asteroids Spawn Rate")]
		[SerializeField] AnimationCurve _asteroidSpawnDelayOverTime;
		public float GetNextAsteroidSpawnDelayOverTime()
		{
			var time = Time.time;
			return time + _asteroidSpawnDelayOverTime.Evaluate(time);
		}

		[Header("Missiles")]
		[SerializeField] float _missileSpeed;
		[SerializeField] float _missileSecondsToLive;
		public float MissileSpeed => _missileSpeed;
		public float MissilesSecondsToLive => _missileSecondsToLive;

		[Header("Points")]
		[SerializeField] int _pointsForBigAsteroid = 100;
		[SerializeField] int _pointsForMediumAsteroid = 200;
		[SerializeField] int _pointsForSmallAsteroid = 300;
		public int PointsForBigAsteroid => _pointsForBigAsteroid;
		public int PointsForMediumAsteroid => _pointsForMediumAsteroid;
		public int PointsForSmallAsteroid => _pointsForSmallAsteroid;

		[Header("Player")]
		[SerializeField] int _playerLives = 3;
		public int PlayerLives => _playerLives;
	}
}
