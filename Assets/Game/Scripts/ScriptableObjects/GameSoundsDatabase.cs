using Ami.BroAudio;
using UnityEngine;

namespace mygame
{
	[CreateAssetMenu(fileName = "GameSoundsDatabase", menuName = "Scriptable Objects/GameSoundsDatabase")]
	public class GameSoundsDatabase : ScriptableObject
	{
		[SerializeField] SoundID _explosionBig;
		[SerializeField] SoundID _explosionMedium;
		[SerializeField] SoundID _explosionSmall;
		[Space]
		[SerializeField] SoundID _fireMissile;
		[Space]
		[SerializeField] SoundID _shipThrust;

		public void PlayExplosionBig() => _explosionBig.Play();
		public void PlayExplosionMedium() => _explosionMedium.Play();
		public void PlayExplosionSmall() => _explosionSmall.Play();

		public void PlayFireMissile() => _fireMissile.Play();

		public IAudioPlayer PlayShipThrust() => _shipThrust.Play();
	}
}
