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

	    [Header("Prefabs")]
	    [SerializeField] GameObject _prefabMissile;

	    Transform _transform;
	    PlayerInput _input;

	    EntitiesManager _entitiesManager;

	    float _moveSpeed = 0f;

	    void Awake()
	    {
		    Assert.IsNotNull(_prefabMissile, "Prefab object is not assigned. Please assign a prefab in the inspector.");

		    _transform = GetComponent<Transform>();

		    _input = GetComponent<PlayerInput>();
		    Assert.IsNotNull(_input, "PlayerInput could not be found.");

		    ServiceLocator.Lookup
			    .Get(out _entitiesManager)
			    .Done();
	    }

	    void Update()
	    {
		    var dt = Time.deltaTime;

		    _transform.GetLocalPositionAndRotation(out var pos, out var rot);
		    var fwd = _transform.up;

		    if (_input.UseShouldFire())
			    _entitiesManager.Spawn(_prefabMissile, pos, fwd * 10f);

		    if (_input.IsThrusting)
		    {
			    _moveSpeed = Mathf.Min(_maxSpeed, _moveSpeed + _thrustSpeed * dt);
		    }
		    else if (_input.IsBreaking)
		    {
			    _moveSpeed = Mathf.Max(0f, _moveSpeed - _breakSpeed * dt);
		    }

		    pos += _moveSpeed * dt * fwd;
		    rot *= Quaternion.Euler(0f, 0f, _input.RotateDirection * _rotationSpeed * dt * -1f);

		    _transform.SetLocalPositionAndRotation(pos, rot);
	    }
    }
}
