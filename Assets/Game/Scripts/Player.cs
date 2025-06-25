using UnityEngine;

namespace mygame
{
	public class Player : MonoBehaviour
	{
		[SerializeField] float _rotationSpeed = 180f; // Degrees per second
		[SerializeField] float _thrustSpeed = 1f;
		[SerializeField] float _maxSpeed = 2f;
		[SerializeField] float _breakSpeed = 1f;

		PlayerShipInput _input;

		Transform _transform;

		float _rotateDirection = 0f;
		float _moveSpeed = 0f;

		bool _isThrusting = false;
		bool _isBreaking = false;

		void Awake()
		{
			_transform = GetComponent<Transform>();

			_input = new PlayerShipInput();
			_input.Movement.Rotate.performed += ctx => _rotateDirection = ctx.ReadValue<float>();
			_input.Movement.Rotate.canceled += ctx => _rotateDirection = 0f;

			_input.Movement.Thrust.performed += ctx => _isThrusting = true;
			_input.Movement.Thrust.canceled += ctx => _isThrusting = false;

			_input.Movement.Brake.performed += ctx => _isBreaking = true;
			_input.Movement.Brake.canceled += ctx => _isBreaking = false;
		}

		void OnEnable()
		{
			_input.Enable();
		}

		void OnDisable()
		{
			_input.Disable();
		}

		void OnDestroy()
		{
			_input.Dispose();
		}

		void Update()
		{
			var dt = Time.deltaTime;

			_transform.GetLocalPositionAndRotation(out var pos, out var rot);

			if (_isThrusting)
			{
				_moveSpeed = Mathf.Min(_maxSpeed, _moveSpeed + _thrustSpeed * dt);
			}
			else if (_isBreaking)
			{
				_moveSpeed = Mathf.Max(0f, _moveSpeed - _breakSpeed * dt);
			}

			pos += _transform.up * _moveSpeed * dt;
			rot *= Quaternion.Euler(0f, 0f, _rotateDirection * _rotationSpeed * dt * -1f);

			_transform.SetLocalPositionAndRotation(pos, rot);
		}
	}
}
