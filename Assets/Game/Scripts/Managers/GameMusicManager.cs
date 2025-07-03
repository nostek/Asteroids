using Ami.BroAudio;
using UnityEngine;
using UnityEventsCenter;

namespace mygame
{
	public class GameMusicManager : MonoBehaviour
	{
		const float VolumeFull = 1f;

		[Header("Track")]
		[SerializeField] SoundID _music;

		[Header("Settings")]
		[SerializeField] float _volumeLower = .25f;
		[SerializeField] float _fadeTimeInSeconds = .5f;

		IAudioPlayer _instance;

		void Awake()
		{
			//If we dont assign any music, dont play it
			if (!_music.IsValid())
				return;

			_instance = _music.Play();
			_instance.SetVolume(_volumeLower); //Set initial volume to lower, as we will wait for spawn and dont want the volume to go from 100% to lower% directly
		}

		void OnDestroy()
		{
			if (_instance != null)
			{
				if (_instance.IsPlaying)
					_instance.Stop();
				_instance = null;
			}
		}

		void OnEnable() => EventsCenter.Subscribe<GameEvents.Player.WaitingForSpawn>(OnWaitingForSpawn);
		void OnDisable() => EventsCenter.Unsubscribe<GameEvents.Player.WaitingForSpawn>(OnWaitingForSpawn);

		void OnWaitingForSpawn(GameEvents.Player.WaitingForSpawn ev)
		{
			if (_instance == null)
				return;

			_instance.SetVolume(ev.IsWaiting ? _volumeLower : VolumeFull, _fadeTimeInSeconds);
		}
	}
}

