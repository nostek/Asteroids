using UnityEngine;
using UnityServiceLocator;

namespace mygame
{
	[RequireComponent(typeof(Player), typeof(PlayerInput))]
	public class PlayerMover : MonoBehaviour
	{
		Transform _transform;
		Player _player;
		PlayerInput _input;

		EntitiesManager _entitiesManager;
		Tweaktable _tweaktable;

		void Awake()
		{
			_transform = GetComponent<Transform>();

			_player = GetComponent<Player>();
			_input = GetComponent<PlayerInput>();

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

			if (_input.DoFire)
				_entitiesManager.Spawn(GameEntities.Missile, pos, fwd * _tweaktable.MissileSpeed);

			if (_input.IsThrusting)
			{
				var moveDirection = _player.Entity.GetDirectionAndSpeed();

				//add forward momentum but clamp it at _maxSpeed
				moveDirection = Vector2.ClampMagnitude(moveDirection + _tweaktable.PlayerThrustSpeed * dt * fwd, _tweaktable.PlayerMaxSpeed);

				_player.Entity.SetDirectionAndSpeed(moveDirection);
			}
			else if (_input.IsBreaking)
			{
				var moveDirection = _player.Entity.GetDirectionAndSpeed();

				//since magnitude cant be negative, we calculate the magnitude of the forward movementum
				//and decrease that, clamping at 0, then scale the normalized forward magnitude with the
				//new speed
				var speed = moveDirection.magnitude;
				speed = Mathf.Max(0f, speed - _tweaktable.PlayerBreakSpeed * dt);
				moveDirection = moveDirection.normalized * speed;

				_player.Entity.SetDirectionAndSpeed(moveDirection);
			}

			if (_tweaktable.PlayerRotationSpeed != 0f)
			{
				rot *= Quaternion.Euler(0f, 0f, _input.RotateDirection * _tweaktable.PlayerRotationSpeed * dt * -1f);
				_transform.localRotation = rot;
			}
		}
	}
}
