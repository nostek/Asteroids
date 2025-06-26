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
			for (int i = 0; i < transform.childCount; i++)
				transform.GetChild(i).gameObject.SetActive(i < ev.Lives);
		}
	}
}
