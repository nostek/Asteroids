using UnityEngine;
using UnityEventsCenter;

namespace mygame
{
	public class UILivesCounter : MonoBehaviour
	{
		void Awake() => EventsCenter.Subscribe<GameEvents.Player.LivesChanged>(OnLivesChanged);
		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.Player.LivesChanged>(OnLivesChanged);

		void OnLivesChanged(GameEvents.Player.LivesChanged ev)
		{
			//add more ui elements if needed
			while (transform.childCount < ev.Lives)
				Instantiate(transform.GetChild(0).gameObject, transform);

			for (int i = 0; i < transform.childCount; i++)
				transform.GetChild(i).gameObject.SetActive(i < ev.Lives);
		}
	}
}
