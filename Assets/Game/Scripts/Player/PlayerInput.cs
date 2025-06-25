using UnityEngine;

namespace mygame
{
    public class PlayerInput : MonoBehaviour
    {
	    PlayerShipInput _input;

	    float _rotateDirection = 0f;
	    bool _isThrusting = false;
	    bool _isBreaking = false;
	    bool _shouldFire = false;

	    void Awake()
	    {
		    _input = new PlayerShipInput();
		    _input.Movement.Rotate.performed += ctx => _rotateDirection = ctx.ReadValue<float>();
		    _input.Movement.Rotate.canceled += _ => _rotateDirection = 0f;

		    _input.Movement.Thrust.performed += _ => _isThrusting = true;
		    _input.Movement.Thrust.canceled += _ => _isThrusting = false;

		    _input.Movement.Brake.performed += _ => _isBreaking = true;
		    _input.Movement.Brake.canceled += _ => _isBreaking = false;

		    _input.Actions.Fire.performed += _ => _shouldFire = true;
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

	    public float RotateDirection => _rotateDirection;
	    public bool IsThrusting => _isThrusting;
	    public bool IsBreaking => _isBreaking;

	    public bool UseShouldFire()
	    {
		    var x = _shouldFire;
		    _shouldFire = false;
		    return x;
	    }
    }
}
