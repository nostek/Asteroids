using UnityEngine;
using UnityEventsCenter;

namespace mygame
{
	public class UILivesCounter : MonoBehaviour
	{
		void Awake() => EventsCenter.Subscribe<GameEvents.LivesChangedEvent>(OnLivesChanged);
		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.LivesChangedEvent>(OnLivesChanged);

		void OnLivesChanged(GameEvents.LivesChangedEvent ev)
		{
			//add more ui elements if needed
			while (transform.childCount < ev.Lives)
				Instantiate(transform.GetChild(0).gameObject, transform);

			for (int i = 0; i < transform.childCount; i++)
				transform.GetChild(i).gameObject.SetActive(i < ev.Lives);
		}
	}
}
