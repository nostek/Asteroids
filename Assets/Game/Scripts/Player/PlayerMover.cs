using UnityEngine;
using UnityEngine.Assertions;
using UnityServiceLocator;

namespace mygame
{
	public class PlayerMover : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField] float _rotationSpeed = 180f; // Degrees per second
		[SerializeField] float _thrustSpeed = 1f;
		[SerializeField] float _maxSpeed = 2f;
		[SerializeField] float _breakSpeed = 1f;

		Transform _transform;
		Player _player;
		PlayerInput _input;

		EntitiesManager _entitiesManager;
		Tweaktable _tweaktable;

		void Awake()
		{
			_transform = GetComponent<Transform>();

			_player = GetComponent<Player>();
			Assert.IsNotNull(_player, "Player could not be found.");

			_input = GetComponent<PlayerInput>();
			Assert.IsNotNull(_input, "PlayerInput could not be found.");

			ServiceLocator.Lookup
				.Get(out _entitiesManager)
				.Get(out _tweaktable)
				.Done();
		}

		void Update()
		{
			var dt = Time.deltaTime;

			_transform.GetLocalPositionAndRotation(out var pos, out var rot);
			Vector2 fwd = rot * Vector3.up; //Implicit conversion to Vector2

			if (_input.UseShouldFire())
				_entitiesManager.Spawn(GameEntities.Missile, pos, fwd * _tweaktable.MissileSpeed);

			if (_input.IsThrusting)
			{
				var moveDirection = _player.Entity.GetDirectionAndSpeed();

				//add forward momentum but clamp it at _maxSpeed
				moveDirection = Vector2.ClampMagnitude(moveDirection + _thrustSpeed * dt * fwd, _maxSpeed);

				_player.Entity.SetDirectionAndSpeed(moveDirection);
			}
			else if (_input.IsBreaking)
			{
				var moveDirection = _player.Entity.GetDirectionAndSpeed();

				//since magnitude cant be negative, we calculate the magnitude of the forward movementum
				//and decrease that, clamping at 0, then scale the normalized forward magnitude with the
				//new speed
				var speed = moveDirection.magnitude;
				speed = Mathf.Max(0f, speed - _breakSpeed * dt);
				moveDirection = moveDirection.normalized * speed;

				_player.Entity.SetDirectionAndSpeed(moveDirection);
			}

			if (_rotationSpeed != 0f)
			{
				rot *= Quaternion.Euler(0f, 0f, _input.RotateDirection * _rotationSpeed * dt * -1f);
				_transform.localRotation = rot;
			}
		}
	}
}
