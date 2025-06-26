using UnityEngine;

namespace mygame
{
	public class Player : MonoBehaviour
	{
		public EntityReference Entity;

		void OnDisable()
		{
			Entity = default;
		}
	}
}
