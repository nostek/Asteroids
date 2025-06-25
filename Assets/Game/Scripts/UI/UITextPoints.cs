using TMPro;
using UnityEngine;
using UnityEventsCenter;

namespace mygame
{
	[RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextPoints : MonoBehaviour
    {
	    TextMeshProUGUI _text;

	    int _points = 0;

	    void Awake()
	    {
		    _text = gameObject.GetComponent<TextMeshProUGUI>();
		    RefreshText();

		    EventsCenter.Subscribe<GameEvents.AddPointsEvent>(OnAddPoints);
	    }

	    void OnDestroy()
	    {
		    EventsCenter.Unsubscribe<GameEvents.AddPointsEvent>(OnAddPoints);
	    }

	    void OnAddPoints(GameEvents.AddPointsEvent ev)
	    {
		    _points += ev.Points;
		    RefreshText();
	    }

	    void RefreshText() => _text.text = $"Points: {_points}";
    }
}
