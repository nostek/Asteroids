using UnityEngine;
using UnityEngine.InputSystem;

namespace mygame
{
	public class PlayerInput : MonoBehaviour, PlayerShipInput.IMovementActions, PlayerShipInput.IActionsActions
	{
		PlayerShipInput _input;

		void Awake()
		{
			_input = new PlayerShipInput();
			_input.Movement.SetCallbacks(this);
			_input.Actions.SetCallbacks(this);
		}

		void OnEnable() => _input.Enable();
		void OnDisable() => _input.Disable();
		void OnDestroy() => _input.Dispose();

		void LateUpdate()
		{
			//clear action booleans in LateUpdate(), so things that read in Update() will get the same value
			DoFire = false;
		}

		public float RotateDirection { get; private set; } = 0f;
		public bool IsThrusting { get; private set; } = false;
		public bool IsBreaking { get; private set; } = false;
		public bool DoFire { get; private set; } = false;

		void PlayerShipInput.IMovementActions.OnRotate(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				RotateDirection = context.ReadValue<float>();
			else if (context.phase == InputActionPhase.Canceled)
				RotateDirection = 0f;
		}

		void PlayerShipInput.IMovementActions.OnThrust(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				IsThrusting = true;
			else if (context.phase == InputActionPhase.Canceled)
				IsThrusting = false;
		}

		void PlayerShipInput.IMovementActions.OnBrake(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				IsBreaking = true;
			else if (context.phase == InputActionPhase.Canceled)
				IsBreaking = false;
		}

		void PlayerShipInput.IActionsActions.OnFire(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Performed)
				DoFire = true;
		}
	}
}
