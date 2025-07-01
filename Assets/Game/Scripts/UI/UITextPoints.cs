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

			EventsCenter.Subscribe<GameEvents.AddPointsEvent>(OnAddPoints);
		}

		void OnDestroy() => EventsCenter.Unsubscribe<GameEvents.AddPointsEvent>(OnAddPoints);

		void OnAddPoints(GameEvents.AddPointsEvent ev) => RefreshText(ev.TotalPoints);

		void RefreshText(int points) => _text.SetText("Points: {0}", points);
	}
}
