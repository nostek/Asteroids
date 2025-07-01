using UnityEngine;

namespace mygame
{
	[CreateAssetMenu(fileName = "PlayerTweaktable", menuName = "Scriptable Objects/PlayerTweaktable")]
	public class PlayerTweaktable : ScriptableObject
	{
		[Header("Player")]
		[SerializeField] int _playerLives = 3;
		[SerializeField] float _playerRotationSpeed = 180f; // Degrees per second
		[SerializeField] float _playerMaxSpeed = 2f;
		[SerializeField] float _playerThrustSpeed = 1f;
		[SerializeField] float _playerBreakSpeed = 1f;
		public int PlayerLives => _playerLives;
		public float PlayerRotationSpeed => _playerRotationSpeed;
		public float PlayerMaxSpeed => _playerMaxSpeed;
		public float PlayerThrustSpeed => _playerThrustSpeed;
		public float PlayerBreakSpeed => _playerBreakSpeed;
	}
}
