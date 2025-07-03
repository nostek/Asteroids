using TMPro;
using UnityEngine;
using UnityEventsCenter;

namespace mygame
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UITextPoints : MonoBehaviour
	{
		TextMeshProUGUI _text;

		void Awake()
		{
			_text = gameObject.GetComponent<TextMeshProUGUI>();
			RefreshText(0);

			EventsCenter.Subscribe<GameEvents.Session.AddPoints>(OnAddPoints);
		}

		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.Session.AddPoints>(OnAddPoints);

		void OnAddPoints(GameEvents.Session.AddPoints ev) => RefreshText(ev.TotalPoints);

		void RefreshText(int points) => _text.SetText("Points: {0}", points);
	}
}
