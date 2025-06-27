using Ami.BroAudio;
using UnityEngine;
using UnityServiceLocator;

namespace mygame
{
	[RequireComponent(typeof(PlayerInput))]
	public class PlayerSounds : MonoBehaviour
	{
		PlayerInput _input;

		GameSoundsDatabase _soundsDatabase;

		IAudioPlayer _thrust;

		void Awake()
		{
			_input = GetComponent<PlayerInput>();

			ServiceLocator.Lookup
				.Get(out _soundsDatabase)
				.Done();
		}

		void OnDisable()
		{
			if (_thrust != null)
			{
				if (_thrust.IsPlaying)
					_thrust.Stop();
				_thrust = null;
			}
		}

		void Update()
		{
			if (_input.DoFire)
				_soundsDatabase.PlayFireMissile();

			if (_input.IsThrusting)
			{
				if (_thrust == null)
					_thrust = _soundsDatabase.PlayShipThrust();
			}
			else
			{
				if (_thrust != null)
				{
					_thrust.Stop();
					_thrust = null;
				}
			}
		}
	}
}
